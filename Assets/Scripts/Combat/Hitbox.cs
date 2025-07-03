using System.Collections.Generic;
using Combat.Interfaces;
using Combat.Events;
using UnityEngine;
using UnityEngine.Android;

namespace Combat
{
    [RequireComponent(typeof(Collider))]
    public class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Settings")]
        [SerializeField] private LayerMask _targetLayers = -1;
        [SerializeField] private bool _isActiveOnStart = false;
        
        [Header("Events")]
        [SerializeField] private CombatEventChannel _combatEventChannel;
        
        [Header("Debug")]
        [SerializeField] private bool _debugDrawHitbox = true;
        [SerializeField] private Color _hitboxColor = new Color(1f, 0f, 0f, 0.3f);

        public GameObject Attacker;
        
        private Collider _hitboxCollider;
        private HashSet<Collider> _hitTargets = new HashSet<Collider>();
        
        private bool _isActive;
        
        public bool IsActive => _isActive;
        
        private void Awake()
        {
            _hitboxCollider = GetComponent<Collider>();
            _hitboxCollider.isTrigger = true;
            
            // if (!_isActiveOnStart)
            // { 
            //     DisableHitbox();
            // }
            
            EnableHitbox();
        }
        
        public void EnableHitbox()
        {
            _hitTargets.Clear();
            _hitboxCollider.enabled = true;
            _isActive = true;
        }
        
        public void DisableHitbox()
        {
            _hitboxCollider.enabled = false;
            _isActive = false;
            _hitTargets.Clear();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            
            if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
            
            if (other.transform.root == transform.root) return;
            
            if (!_hitTargets.Contains(other))
            {
                _hitTargets.Add(other);
                
                Debug.Log("Hitted!");
                
                var hitInfo = new HitInfo(Attacker, other.gameObject, other.ClosestPoint(transform.position));
                
                // 이벤트 발행
                if (_combatEventChannel != null)
                {
                    _combatEventChannel.RaiseHit(hitInfo);
                }
                else
                {
                    Debug.LogWarning("[Hitbox] CombatEventChannel is not assigned!");
                }
            }
        }
    }
}