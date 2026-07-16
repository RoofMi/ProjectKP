using Combat.Events;
using Combat.Interfaces;
using UnityEngine;

namespace Combat
{
    public class CombatController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private CombatEventChannel _combatEventChannel;

        private ComboManager _comboManager;

        private void Awake()
        {
            _comboManager = GetComponent<ComboManager>();
        }

        private void OnEnable()
        {
            if (_combatEventChannel != null)
            {
                _combatEventChannel.Subscribe(HandleHitEvent);
            }
        }

        private void OnDisable()
        {
            if (_combatEventChannel != null)
            {
                _combatEventChannel.Unsubscribe(HandleHitEvent);
            }
        }

        private void HandleHitEvent(HitInfo hitInfo)
        {
            if (hitInfo.attacker != gameObject || hitInfo.target == null)
            {
                return;
            }

            IDamageable damageable = hitInfo.target.GetComponent<IDamageable>()
                ?? hitInfo.target.GetComponentInParent<IDamageable>();
            if (damageable == null || damageable.IsDead)
            {
                return;
            }

            damageable.ReceiveHit(hitInfo);

            if (!damageable.IsDead)
            {
                _comboManager?.RecordHitTarget(damageable);
            }

            Debug.Log($"[CombatController] Dealt {hitInfo.damage} damage to {hitInfo.target.name}");
        }
    }
}
