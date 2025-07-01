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
                
                // StepNode가 다르면 빠르게 스킵
                RuntimeComboNode existingNode = null;
                foreach (var node in childList)
                {
                    if (node.StepNode == stepRef.ComboNode &&
                        node.Damage == stepRef.Damage &&
                        node.WindowStart == stepRef.WindowStart &&
                        node.WindowEnd == stepRef.WindowEnd &&
                        node.StaminaCost == stepRef.StaminaCost)
                    {
                        existingNode = node;
                        break;
                    }
                }

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
