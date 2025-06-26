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
            
            #if UNITY_EDITOR
            // 에디터에서만 디버그 출력
            DebugPrintComboTree();
            #endif
        }
        
        private void DebugPrintComboTree()
        {
            PrintNode(comboTree.Root, "", 0);
        }
        
        private void PrintNode(RuntimeComboNode node, string prefix, int depth)
        {
            foreach (var child in node.Children)
            {
                foreach (var childNode in child.Value)
                {
                    PrintNode(childNode, prefix + "      ", depth + 1);
                }
            }
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
    }
}