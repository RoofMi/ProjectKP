using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public struct HitInfo
    {
        public float damage;
        public GameObject attacker;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        
        public HitInfo(float damage, GameObject attacker, Vector3 hitPoint, Vector3 hitNormal)
        {
            this.damage = damage;
            this.attacker = attacker;
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
        }
        
        // 단순 데미지만 필요한 경우를 위한 간단한 생성자
        public HitInfo(float damage) : this(damage, null, Vector3.zero, Vector3.zero)
        {
        }
    }
}