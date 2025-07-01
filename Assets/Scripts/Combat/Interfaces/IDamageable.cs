using UnityEngine;

namespace Combat.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(HitInfo hitInfo);
        bool IsDead { get; }
        Transform Transform { get; }
    }
}