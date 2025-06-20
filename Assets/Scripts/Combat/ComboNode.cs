using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "ComboNode", menuName = "Scriptable Objects/ComboNode")]
    public class ComboNode : ScriptableObject
    {
        public string NodeName;
        public AnimationClip AnimClip;
        public float BaseDamage = 10f;
        public float BaseWindowStart = 0.7f;
        public float BaseWindowEnd = 0.9f;
    }
}
