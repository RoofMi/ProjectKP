using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public static class ComboTreeBuilder
    {
        public static RuntimeComboTree BuildTree(IEnumerable<ComboDefinition> comboDefs)
        {
            if (comboDefs == null)
            {
                Debug.LogError("ComboTreeBuilder: comboDefs is null");
                return new RuntimeComboTree();
            }
            
            var tree = new RuntimeComboTree();
            var rootNode = tree.Root;
            
            foreach (var def in comboDefs)
            {
                if (def == null || def.ComboSteps == null || def.ComboSteps.Count == 0)
                {
                    Debug.LogWarning($"ComboTreeBuilder: Invalid combo definition {def?.name}");
                    continue;
                }
                
                InsertCombo(def, rootNode);
            }

            return tree;
        }
        
        private static void InsertCombo(ComboDefinition comboDef, RuntimeComboNode root)
        {
            RuntimeComboNode current = root;
            
            foreach (var stepRef in comboDef.ComboSteps)
            {
                if (stepRef == null || stepRef.ComboNode == null)
                {
                    Debug.LogWarning($"ComboTreeBuilder: Invalid step reference in {comboDef.name}");
                    continue;
                }
                
                if (!current.Children.TryGetValue(stepRef.InputKey, out var childList))
                {
                    childList = new List<RuntimeComboNode>();
                    current.Children[stepRef.InputKey] = childList;
                }
                
                // 중복 노드 검사
                RuntimeComboNode existingNode = null;
                foreach (var node in childList)
                {
                    if (node.StepNode == stepRef.ComboNode &&
                        node.DamageMultiplier == stepRef.DamageMultiplier &&
                        node.ApplyKnockback == stepRef.ApplyKnockback &&
                        node.KnockbackHorizontalForce == stepRef.KnockbackHorizontalForce &&
                        node.KnockbackVerticalForce == stepRef.KnockbackVerticalForce &&
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
                    // 새로운 생성자 사용
                    var newNode = new RuntimeComboNode(stepRef);
                    childList.Add(newNode);
                    current = newNode;
                }
            }
        }
    }
}
