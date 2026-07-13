using Character;
using Character.Core;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CombatDemo
{
    [DisallowMultipleComponent]
    public sealed class CombatDemoLockOn : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private Transform _target;
        [SerializeField] private float _rotationSpeed = 14f;

        private ActionController _actionController;
        private HealthComponent _targetHealth;
        private Transform _defaultLookAt;

        public bool IsLockedOn { get; private set; }
        public string TargetName => _target != null ? _target.name : "OFF";

        private void Awake()
        {
            _actionController = GetComponent<ActionController>();
            _camera ??= Object.FindFirstObjectByType<CinemachineCamera>();
            _defaultLookAt = _camera != null ? _camera.LookAt : null;
            ResolveTarget();
        }

        private void OnDisable()
        {
            SetLock(false);
        }

        private void Update()
        {
            if (Keyboard.current?.tabKey.wasPressedThisFrame == true)
            {
                SetLock(!IsLockedOn);
            }

            if (!IsLockedOn)
            {
                return;
            }

            if (_target == null || (_targetHealth != null && _targetHealth.IsDead))
            {
                SetLock(false);
                return;
            }

            _camera.LookAt = _target;
            RotateTowardsTarget();
        }

        private void SetLock(bool shouldLock)
        {
            if (shouldLock)
            {
                ResolveTarget();
            }

            IsLockedOn = shouldLock && _target != null && _camera != null;
            if (_camera != null)
            {
                _camera.LookAt = IsLockedOn ? _target : _defaultLookAt;
            }
        }

        private void ResolveTarget()
        {
            if (_target == null)
            {
                var enemy = GameObject.FindGameObjectWithTag("Enemy");
                _target = enemy != null ? enemy.transform : null;
            }

            _targetHealth = _target != null ? _target.GetComponent<HealthComponent>() : null;
        }

        private void RotateTowardsTarget()
        {
            if (_actionController != null &&
                (_actionController.HasTag(ActionTags.Attacking) || _actionController.HasTag(ActionTags.Dashing)))
            {
                return;
            }

            var direction = _target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }
}
