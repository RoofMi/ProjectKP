using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    [System.Serializable]
    public class ComboStepReference
    {
        public ComboNode ComboNode;
        public string InputKey;
        public float Damage = 10f;
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
