using System.Collections.Generic;
using Combat.Interfaces;
using UnityEngine;

namespace Combat
{
    [RequireComponent(typeof(Collider))]
    public class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Settings")]
        [SerializeField] private LayerMask targetLayers = -1;
        [SerializeField] private bool isActiveOnStart = false;
        
        [Header("Debug")]
        [SerializeField] private bool debugDrawHitbox = true;
        [SerializeField] private Color hitboxColor = new Color(1f, 0f, 0f, 0.3f);
        
        private Collider hitboxCollider;
        private HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        
        private float currentDamage;
        private GameObject attacker;
        private bool isActive;
        
        public bool IsActive => isActive;
        
        private void Awake()
        {
            hitboxCollider = GetComponent<Collider>();
            hitboxCollider.isTrigger = true;
            
            if (!isActiveOnStart)
            {
                DisableHitbox();
            }
        }
        
        public void EnableHitbox(float damage, GameObject attackerObject = null)
        {
            currentDamage = damage;
            attacker = attackerObject ?? transform.root.gameObject;
            
            hitTargets.Clear();
            hitboxCollider.enabled = true;
            isActive = true;
        }
        
        public void DisableHitbox()
        {
            hitboxCollider.enabled = false;
            isActive = false;
            hitTargets.Clear();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            
            // 레이어 체크
            if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
            
            // 자기 자신 공격 방지
            if (other.transform.root == transform.root) return;
            
            // IDamageable 컴포넌트 찾기
            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }
            
            if (damageable != null && !hitTargets.Contains(damageable))
            {
                // 중복 히트 방지
                hitTargets.Add(damageable);
                
                // 히트 정보 생성
                var hitInfo = new HitInfo(
                    currentDamage,
                    attacker,
                    other.ClosestPoint(transform.position),
                    (other.transform.position - transform.position).normalized
                );
                
                // 데미지 적용
                damageable.TakeDamage(hitInfo);
                
                // 히트 이펙트나 사운드는 여기서 재생
                OnHitTarget(damageable, hitInfo);
            }
        }
        
        protected virtual void OnHitTarget(IDamageable target, HitInfo hitInfo)
        {
            // 히트 이펙트, 사운드 등 처리
            // 추후 구현
        }
        
        private void OnDrawGizmos()
        {
            if (!debugDrawHitbox) return;
            
            if (hitboxCollider == null)
                hitboxCollider = GetComponent<Collider>();
                
            if (hitboxCollider != null)
            {
                Gizmos.color = hitboxColor;
                
                if (hitboxCollider is BoxCollider box)
                {
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.matrix = oldMatrix;
                }
                else if (hitboxCollider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.lossyScale.x);
                }
                else if (hitboxCollider is CapsuleCollider capsule)
                {
                    // 캡슐 기즈모는 복잡하므로 간단히 구 2개로 표현
                    Gizmos.DrawWireCube(transform.position + capsule.center, 
                        new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
                }
            }
        }
    }
}