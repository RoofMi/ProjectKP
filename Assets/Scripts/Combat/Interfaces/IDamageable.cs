using UnityEngine;

namespace Combat.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
        bool IsDead { get; }
        Transform Transform { get; }
    }
}