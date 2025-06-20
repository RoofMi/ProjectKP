using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public class ComboStepReference
    {
        public ComboNode ComboNode;
        public string InputKey;
        public float OverrideDamage;
        public float OverrideWindowStart;
        public float OverrideWindowEnd;
    }
    
    [CreateAssetMenu(fileName = "ComboDefinition", menuName = "Scriptable Objects/ComboDefinition")]
    public class ComboDefinition : ScriptableObject
    {
        public string ComboName;
        public List<ComboStepReference> ComboSteps;
    }
}
