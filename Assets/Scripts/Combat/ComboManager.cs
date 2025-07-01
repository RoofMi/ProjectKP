using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class ComboManager : MonoBehaviour
    {
        [Header("ComboDefinition List")]
        public List<ComboDefinition> comboDefinitions;

        private RuntimeComboTree comboTree;
        private RuntimeComboNode currentNode;

        public RuntimeComboNode CurrentNode => currentNode;
        public RuntimeComboNode RootNode => comboTree?.Root;

        void Start()
        {
            comboTree = ComboTreeBuilder.BuildTree(comboDefinitions);
            currentNode = comboTree.Root;
        }

        public RuntimeComboNode TryAdvanceCombo(string inputKey)
        {
            if (currentNode == null || !currentNode.Children.TryGetValue(inputKey, out var childList))
            {
                return null;
            }

            // 첫 번째 매칭된 노드 선택 (추후 조건 기반 선택 로직 추가 가능)
            var nextNode = childList[0];
            currentNode = nextNode;
            
            return nextNode;
        }

        public void ResetCombo()
        {
            if (comboTree != null)
            {
                currentNode = comboTree.Root;
            }
        }

        public RuntimeComboNode GetFirstComboNode(string inputKey)
        {
            if (comboTree?.Root == null)
                return null;

            if (comboTree.Root.Children.TryGetValue(inputKey, out var childList) && childList.Count > 0)
            {
                currentNode = childList[0];
                return currentNode;
            }

            return null;
        }

        public float GetCurrentDamage()
        {
            return currentNode?.Damage ?? 0f;
        }

        public float GetCurrentStaminaCost()
        {
            return currentNode?.StaminaCost ?? 0f;
        }
    }
}