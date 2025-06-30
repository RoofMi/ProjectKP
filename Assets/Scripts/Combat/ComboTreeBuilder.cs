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
                if (!current.Children.TryGetValue(stepRef.InputKey, out var childList))
                {
                    childList = new List<RuntimeComboNode>();
                    current.Children[stepRef.InputKey] = childList;
                }
                
                var existingNode = childList.Find(n =>
                    n.StepNode == stepRef.ComboNode &&
                    Mathf.Approximately(n.Damage, stepRef.Damage) &&
                    Mathf.Approximately(n.WindowStart, stepRef.WindowStart) &&
                    Mathf.Approximately(n.WindowEnd, stepRef.WindowEnd)
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
                        Damage = stepRef.Damage,
                        WindowStart = stepRef.WindowStart,
                        WindowEnd = stepRef.WindowEnd,
                        StaminaCost = stepRef.StaminaCost,
                        InputKey = stepRef.InputKey
                    };
                    childList.Add(newNode);
                    current = newNode;
                }
            }
        }
    }
}
