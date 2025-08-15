using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public struct HitInfo
    {
        public GameObject attacker;
        public GameObject target;
        public Vector3 hitPoint;
        
        public HitInfo(GameObject attacker, GameObject target, Vector3 hitPoint)
        {
            this.attacker = attacker;
            this.target = target;
            this.hitPoint = hitPoint;
        }
    }
}