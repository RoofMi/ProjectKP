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

        // 3. Knockback Information
        public bool applyKnockback;
        public float knockbackHorizontalForce;
        public float knockbackVerticalForce;

        public HitInfo(
            GameObject attacker,
            GameObject target,
            Vector3 hitPoint,
            float damage,
            bool applyKnockback,
            float knockbackHorizontalForce,
            float knockbackVerticalForce)
        {
            this.attacker = attacker;
            this.target = target;
            this.hitPoint = hitPoint;
            this.damage = damage;
            this.applyKnockback = applyKnockback;
            this.knockbackHorizontalForce = knockbackHorizontalForce;
            this.knockbackVerticalForce = knockbackVerticalForce;
        }
    }
}
