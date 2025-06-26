using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public static class ComboTreeBuilder
    {
        public static RuntimeComboTree BuildTree(IEnumerable<ComboDefinition> comboDefs)
        {
            var tree = new RuntimeComboTree();
            var rootNode = tree.Root;
            
            foreach (var def in comboDefs)
            {
                InsertCombo(def, rootNode);
            }

            return tree;
        }
        
        private static void InsertCombo(ComboDefinition comboDef, RuntimeComboNode root)
        {
            RuntimeComboNode current = root;
            
            foreach (var stepRef in comboDef.ComboSteps)
            {
                float dmg = (stepRef.OverrideDamage > 0) ? stepRef.OverrideDamage : stepRef.ComboNode.BaseDamage;
                float wStart = (stepRef.OverrideWindowStart > 0) ? stepRef.OverrideWindowStart : stepRef.ComboNode.BaseWindowStart;
                float wEnd = (stepRef.OverrideWindowEnd > 0) ? stepRef.OverrideWindowEnd : stepRef.ComboNode.BaseWindowEnd;
                
                if (!current.Children.TryGetValue(stepRef.InputKey, out var childList))
                {
                    childList = new List<RuntimeComboNode>();
                    current.Children[stepRef.InputKey] = childList;
                }
                
                var existingNode = childList.Find(n =>
                    n.StepNode == stepRef.ComboNode &&
                    Mathf.Approximately(n.Damage, dmg) &&
                    Mathf.Approximately(n.WindowStart, wStart) &&
                    Mathf.Approximately(n.WindowEnd, wEnd)
                );

                if (existingNode != null)
                {
                    current = existingNode;
                }
                else
                {
                    var newNode = new RuntimeComboNode
                    {
                        StepNode = stepRef.ComboNode,
                        Damage = dmg,
                        WindowStart = wStart,
                        WindowEnd = wEnd,
                        StaminaCost = stepRef.ComboNode.StaminaCost,
                        InputKey = stepRef.InputKey
                    };
                    childList.Add(newNode);
                    current = newNode;
                }
            }
        }
    }
}
