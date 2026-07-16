using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public struct HitInfo
    {
        // 1. Default Information
        public GameObject attacker;
        public GameObject target;
        public Vector3 hitPoint;

        // 2. Damage Information
        public float damage;

        // 3. Reaction Information
        public Vector3 attackDirection;
        public HitReactionProfile hitReactionProfile;

        public HitInfo(
            GameObject attacker,
            GameObject target,
            Vector3 hitPoint,
            Vector3 attackDirection,
            float damage,
            HitReactionProfile hitReactionProfile)
        {
            this.attacker = attacker;
            this.target = target;
            this.hitPoint = hitPoint;
            this.attackDirection = attackDirection;
            this.damage = damage;
            this.hitReactionProfile = hitReactionProfile;
        }
    }
}
