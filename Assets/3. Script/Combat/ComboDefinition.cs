using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat
{
    [System.Serializable]
    public class ComboStepReference
    {
        public ComboNode ComboNode;
        public string InputKey;
        [FormerlySerializedAs("Damage")]
        [Min(0f)] public float DamageMultiplier = 1f;
        public bool ApplyKnockback = true;
        [Min(0f)] public float KnockbackHorizontalForce = 10f;
        [Min(0f)] public float KnockbackVerticalForce = 5f;
        public float WindowStart = 0.5f;
        public float WindowEnd = 0.9f;
        public float StaminaCost = 10f;
    }
    
    [CreateAssetMenu(fileName = "ComboDefinition", menuName = "Scriptable Objects/ComboDefinition")]
    public class ComboDefinition : ScriptableObject
    {
        public string ComboName;
        public List<ComboStepReference> ComboSteps;
    }
}
