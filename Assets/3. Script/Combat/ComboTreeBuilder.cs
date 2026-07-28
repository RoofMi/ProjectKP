using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public static class ComboTreeBuilder
    {
        public static RuntimeComboNode BuildTree(IEnumerable<ComboDefinition> comboDefinitions)
        {
            var root = new RuntimeComboNode();
            if (comboDefinitions == null)
            {
                Debug.LogError("ComboTreeBuilder: comboDefs is null");
                return root;
            }

            foreach (ComboDefinition definition in comboDefinitions)
            {
                if (definition == null || definition.ComboSteps == null || definition.ComboSteps.Count == 0)
                {
                    Debug.LogWarning($"ComboTreeBuilder: Invalid combo definition {definition?.name}");
                    continue;
                }

                InsertCombo(definition, root);
            }

            return root;
        }

        private static void InsertCombo(ComboDefinition comboDefinition, RuntimeComboNode root)
        {
            RuntimeComboNode current = root;

            foreach (ComboStepReference stepReference in comboDefinition.ComboSteps)
            {
                if (stepReference == null || stepReference.ComboNode == null)
                {
                    Debug.LogWarning($"ComboTreeBuilder: Invalid step reference in {comboDefinition.name}");
                    continue;
                }

                if (!current.Children.TryGetValue(stepReference.InputKey, out List<RuntimeComboNode> childNodes))
                {
                    childNodes = new List<RuntimeComboNode>();
                    current.Children[stepReference.InputKey] = childNodes;
                }

                RuntimeComboNode existingNode = childNodes.Find(node =>
                    node.StepNode == stepReference.ComboNode &&
                    node.DamageMultiplier == stepReference.DamageMultiplier &&
                    node.HitReaction == stepReference.HitReaction &&
                    node.WindowStart == stepReference.WindowStart &&
                    node.WindowEnd == stepReference.WindowEnd &&
                    node.StaminaCost == stepReference.StaminaCost);

                if (existingNode != null)
                {
                    current = existingNode;
                    continue;
                }

                var newNode = new RuntimeComboNode(stepReference);
                childNodes.Add(newNode);
                current = newNode;
            }
        }
    }
}
