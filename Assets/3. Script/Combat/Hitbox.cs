using System.Collections.Generic;
using Combat.Events;
using Combat.Interfaces;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Collider))]
    public class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Settings")]
        [SerializeField] private LayerMask _targetLayers = -1;
        [SerializeField] private bool _isActiveOnStart = false;

        [Header("Sweep Detection")]
        [SerializeField] private bool _useSweepDetection = true;
        [SerializeField, Min(0.01f)] private float _sweepRadius = 0.14f;
        [SerializeField, Range(2, 8)] private int _sweepSampleCount = 6;

        [Header("Events")]
        [SerializeField] private CombatEventChannel _combatEventChannel;

        [Header("Debug")]
        [SerializeField] private bool _debugDrawHitbox = true;
        [SerializeField] private Color _hitboxColor = new Color(1f, 0f, 0f, 0.3f);

        public GameObject Attacker;

        private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();
        private readonly Collider[] _overlapResults = new Collider[16];
        private readonly RaycastHit[] _sweepResults = new RaycastHit[16];
        private Collider _hitboxCollider;
        private float _damage;
        private bool _applyKnockback;
        private float _knockbackHorizontalForce;
        private float _knockbackVerticalForce;
        private bool _isActive;
        private Vector3[] _previousSweepPoints;
        private Vector3[] _currentSweepPoints;
        private bool _hasPreviousSweepPoints;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private Mesh _debugMesh;
        private Material _debugMaterial;
        private GameObject _debugVisual;
        private MeshRenderer _debugRenderer;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
#endif

        public bool IsActive => _isActive;

        public void SetTargetLayers(LayerMask targetLayers)
        {
            _targetLayers = targetLayers;
        }

        public bool TargetsLayer(int layer)
        {
            return layer >= 0 && (_targetLayers.value & (1 << layer)) != 0;
        }

        public void ConfigureAttack(
            GameObject attacker,
            float damage,
            bool applyKnockback,
            float knockbackHorizontalForce,
            float knockbackVerticalForce)
        {
            Attacker = attacker;
            _damage = Mathf.Max(0f, damage);
            _applyKnockback = applyKnockback;
            _knockbackHorizontalForce = Mathf.Max(0f, knockbackHorizontalForce);
            _knockbackVerticalForce = Mathf.Max(0f, knockbackVerticalForce);
        }

        public void ClearAttackData()
        {
            Attacker = null;
            _damage = 0f;
            _applyKnockback = false;
            _knockbackHorizontalForce = 0f;
            _knockbackVerticalForce = 0f;
        }

        private void Awake()
        {
            _hitboxCollider = GetComponent<Collider>();
            _hitboxCollider.isTrigger = true;

            if (!_isActiveOnStart)
            {
                DisableHitbox();
            }
        }

        public void EnableHitbox()
        {
            if (Attacker == null)
            {
                Debug.LogWarning("[Hitbox] Cannot enable without attack data.");
                return;
            }

            if (_isActive)
            {
                DisableHitbox();
            }

            _hitTargets.Clear();
            _hitboxCollider.enabled = true;
            _isActive = true;
            InitializeSweepDetection();
            DetectCurrentOverlaps();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UpdateDebugVisual();
#endif
        }

        public void DisableHitbox()
        {
            _hitboxCollider.enabled = false;
            _isActive = false;
            _hitTargets.Clear();
            _hasPreviousSweepPoints = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UpdateDebugVisual();
#endif
        }

        private void LateUpdate()
        {
            if (!_isActive || !_useSweepDetection || _hitboxCollider is not BoxCollider)
            {
                return;
            }

            Physics.SyncTransforms();
            CaptureSweepPoints(_currentSweepPoints);
            DetectCurrentOverlaps();

            if (_hasPreviousSweepPoints)
            {
                SweepBetweenPreviousAndCurrentPoints();
            }

            CopySweepPoints(_currentSweepPoints, _previousSweepPoints);
            _hasPreviousSweepPoints = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryRegisterHit(other, other.ClosestPoint(transform.position));
        }

        private void InitializeSweepDetection()
        {
            _hasPreviousSweepPoints = false;

            if (!_useSweepDetection || _hitboxCollider is not BoxCollider)
            {
                return;
            }

            EnsureSweepBuffers();
            Physics.SyncTransforms();
            CaptureSweepPoints(_previousSweepPoints);
            _hasPreviousSweepPoints = true;
        }

        private void DetectCurrentOverlaps()
        {
            if (_hitboxCollider is not BoxCollider boxCollider)
            {
                return;
            }

            Vector3 center = boxCollider.transform.TransformPoint(boxCollider.center);
            Vector3 halfExtents = Vector3.Scale(boxCollider.size * 0.5f, GetAbsoluteScale(boxCollider.transform.lossyScale));
            int overlapCount = Physics.OverlapBoxNonAlloc(
                center,
                halfExtents,
                _overlapResults,
                boxCollider.transform.rotation,
                _targetLayers,
                QueryTriggerInteraction.Collide);

            for (int index = 0; index < overlapCount; index++)
            {
                Collider other = _overlapResults[index];
                TryRegisterHit(other, other.ClosestPoint(center));
            }
        }

        private void SweepBetweenPreviousAndCurrentPoints()
        {
            float sweepRadius = GetWorldSweepRadius();

            for (int index = 0; index < _sweepSampleCount; index++)
            {
                Vector3 previousPoint = _previousSweepPoints[index];
                Vector3 displacement = _currentSweepPoints[index] - previousPoint;
                float distance = displacement.magnitude;
                if (distance <= Mathf.Epsilon)
                {
                    continue;
                }

                int hitCount = Physics.SphereCastNonAlloc(
                    previousPoint,
                    sweepRadius,
                    displacement / distance,
                    _sweepResults,
                    distance,
                    _targetLayers,
                    QueryTriggerInteraction.Collide);

                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    RaycastHit hit = _sweepResults[hitIndex];
                    TryRegisterHit(hit.collider, hit.point);
                }
            }
        }

        private void EnsureSweepBuffers()
        {
            _sweepSampleCount = Mathf.Clamp(_sweepSampleCount, 2, 8);
            if (_previousSweepPoints != null && _previousSweepPoints.Length == _sweepSampleCount)
            {
                return;
            }

            _previousSweepPoints = new Vector3[_sweepSampleCount];
            _currentSweepPoints = new Vector3[_sweepSampleCount];
        }

        private void CaptureSweepPoints(Vector3[] targetPoints)
        {
            BoxCollider boxCollider = (BoxCollider)_hitboxCollider;
            GetLongestLocalAxis(boxCollider.size, out Vector3 localAxis, out float halfLength);

            for (int index = 0; index < _sweepSampleCount; index++)
            {
                float normalizedPosition = index / (float)(_sweepSampleCount - 1);
                Vector3 localPoint = boxCollider.center + localAxis * Mathf.Lerp(-halfLength, halfLength, normalizedPosition);
                targetPoints[index] = boxCollider.transform.TransformPoint(localPoint);
            }
        }

        private float GetWorldSweepRadius()
        {
            Vector3 scale = GetAbsoluteScale(transform.lossyScale);
            return _sweepRadius * Mathf.Max(scale.x, scale.y, scale.z);
        }

        private static void GetLongestLocalAxis(Vector3 size, out Vector3 axis, out float halfLength)
        {
            if (size.x >= size.y && size.x >= size.z)
            {
                axis = Vector3.right;
                halfLength = size.x * 0.5f;
                return;
            }

            if (size.y >= size.z)
            {
                axis = Vector3.up;
                halfLength = size.y * 0.5f;
                return;
            }

            axis = Vector3.forward;
            halfLength = size.z * 0.5f;
        }

        private static Vector3 GetAbsoluteScale(Vector3 scale)
        {
            return new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }

        private static void CopySweepPoints(Vector3[] source, Vector3[] destination)
        {
            for (int index = 0; index < source.Length; index++)
            {
                destination[index] = source[index];
            }
        }

        private void TryRegisterHit(Collider other, Vector3 hitPoint)
        {
            if (!_isActive || other == null || !TargetsLayer(other.gameObject.layer))
            {
                return;
            }

            if (Attacker != null && other.transform.root == Attacker.transform.root)
            {
                return;
            }

            IDamageable damageable = other.GetComponent<IDamageable>() ?? other.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.IsDead || !_hitTargets.Add(damageable))
            {
                return;
            }

            var hitInfo = new HitInfo(
                Attacker,
                other.gameObject,
                hitPoint,
                _damage,
                _applyKnockback,
                _knockbackHorizontalForce,
                _knockbackVerticalForce);

            if (_combatEventChannel != null)
            {
                _combatEventChannel.RaiseHit(hitInfo);
            }
            else
            {
                Debug.LogWarning("[Hitbox] CombatEventChannel is not assigned!");
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void UpdateDebugVisual()
        {
            if (!_debugDrawHitbox || !_isActive || _hitboxCollider is not BoxCollider boxCollider)
            {
                if (_debugRenderer != null)
                {
                    _debugRenderer.enabled = false;
                }

                return;
            }

            EnsureDebugResources();
            if (_debugRenderer == null)
            {
                return;
            }

            _debugVisual.transform.localPosition = boxCollider.center;
            _debugVisual.transform.localRotation = Quaternion.identity;
            _debugVisual.transform.localScale = boxCollider.size;
            _debugRenderer.enabled = true;
        }

        private void EnsureDebugResources()
        {
            if (_debugMesh == null)
            {
                _debugMesh = new Mesh { name = "HitboxDebugWire" };
                _debugMesh.vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
                    new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
                    new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
                    new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f)
                };
                _debugMesh.SetIndices(new[]
                {
                    0, 1, 1, 2, 2, 3, 3, 0,
                    4, 5, 5, 6, 6, 7, 7, 4,
                    0, 4, 1, 5, 2, 6, 3, 7
                }, MeshTopology.Lines, 0);
                _debugMesh.RecalculateBounds();
            }

            if (_debugMaterial != null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Debug.LogWarning("[Hitbox] Debug shader was not found.");
                return;
            }

            _debugMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            Color color = _hitboxColor;
            color.a = 1f;
            if (_debugMaterial.HasProperty(BaseColorId))
            {
                _debugMaterial.SetColor(BaseColorId, color);
            }
            else if (_debugMaterial.HasProperty(ColorId))
            {
                _debugMaterial.SetColor(ColorId, color);
            }

            _debugVisual = new GameObject("HitboxDebugVisual")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = gameObject.layer
            };
            _debugVisual.transform.SetParent(transform, false);

            var meshFilter = _debugVisual.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = _debugMesh;

            _debugRenderer = _debugVisual.AddComponent<MeshRenderer>();
            _debugRenderer.sharedMaterial = _debugMaterial;
        }

        private void OnDestroy()
        {
            if (_debugVisual != null)
            {
                Destroy(_debugVisual);
            }

            if (_debugMesh != null)
            {
                Destroy(_debugMesh);
            }

            if (_debugMaterial != null)
            {
                Destroy(_debugMaterial);
            }
        }
#endif
    }
}
