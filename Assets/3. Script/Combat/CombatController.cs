using UnityEngine;
using Combat.Interfaces;
using Combat.Events;

namespace Combat
{
    public class CombatController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private CombatEventChannel _combatEventChannel;
        
        void OnEnable()
        {
            if (_combatEventChannel != null)
                _combatEventChannel.Subscribe(HandleHitEvent);
        }
        
        void OnDisable()
        {
            if (_combatEventChannel != null)
                _combatEventChannel.Unsubscribe(HandleHitEvent);
        }
        
        void HandleHitEvent(HitInfo hitInfo)
        {
            // 내 공격인지 확인
            if (hitInfo.attacker != gameObject) return;
            
            // 타겟이 데미지를 받을 수 있는지 확인
            var damageable = hitInfo.target.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hitInfo.target.GetComponentInParent<IDamageable>();
            }
            
            if (damageable != null)
            {
                // TODO: 실제 데미지 계산 (무기, 스탯 등)
                float damage = 10f;
                Vector3 knockbackDirection = (damageable.Transform.position - hitInfo.attacker.transform.position).normalized;
                knockbackDirection.y = 0;

                damageable.TakeDamage(damage);

                //if (hitInfo.applyKnockback)
                    damageable.TakeKnockback(knockbackDirection, 10f, 5f);

                Debug.Log($"[CombatController] Dealt damage to {hitInfo.target.name}");
            }
        }
    }
}
