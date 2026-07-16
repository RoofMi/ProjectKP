using UnityEngine;

namespace Combat.Interfaces
{
    public interface IDamageable
    {
        void ReceiveHit(HitInfo hitInfo);
        bool IsDead { get; }
        Transform Transform { get; }
    }
}
