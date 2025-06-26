using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class RuntimeComboNode
    {
        public ComboNode StepNode;
        
        public float Damage;
        public float WindowStart;
        public float WindowEnd;
        public float StaminaCost;
        
        // Debugging
        public string InputKey;
        
        public Dictionary<string, List<RuntimeComboNode>> Children = new Dictionary<string, List<RuntimeComboNode>>();
    }
}
