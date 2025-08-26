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

        // 2. Knockback Information
        public bool applyKnockback;
        public float knockbackHorizontalForce;
        public float knockbackVerticalForce;

        // 3. Status Effect Information

        public HitInfo(GameObject attacker, GameObject target, Vector3 hitPoint)
        {
            this.attacker = attacker;
            this.target = target;
            this.hitPoint = hitPoint;

            this.applyKnockback = false;
            this.knockbackHorizontalForce = 0f;
            this.knockbackVerticalForce = 0f;
        }
    }
}