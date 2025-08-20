using UnityEngine;

namespace Combat.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
        void TakeKnockback(Vector3 direction, float horizontalForce, float verticalForce);
        bool IsDead { get; }
        Transform Transform { get; }
    }
}