using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class ComboManager : MonoBehaviour
    {
        private RuntimeComboTree comboTree;

        public RuntimeComboNode RootNode => comboTree?.Root;
        
		// 무기마다 고정적으로 할당된 콤보가 있다는 가정 하에 구현된 함수
        public void SetWeaponCombos(ComboDefinition[] weaponCombos)
        {
            if (weaponCombos == null || weaponCombos.Length == 0)
            {
                Debug.LogWarning("ComboManager: No combos provided for weapon");
                return;
            }
            
            comboTree = ComboTreeBuilder.BuildTree(weaponCombos);
        }

        // Stateless: 현재 노드를 파라미터로 받아 다음 노드 반환
        public RuntimeComboNode GetNextComboNode(RuntimeComboNode currentNode, string inputKey)
        {
            if (currentNode == null || !currentNode.Children.TryGetValue(inputKey, out var childList))
            {
                return null;
            }

            // 첫 번째 매칭된 노드 선택 (추후 조건 기반 선택 로직 추가 가능)
            return childList[0];
        }

        // Stateless: 루트에서 시작하는 첫 콤보 노드 반환
        public RuntimeComboNode GetFirstComboNode(string inputKey)
        {
            if (comboTree?.Root == null)
                return null;

            if (comboTree.Root.Children.TryGetValue(inputKey, out var childList) && childList.Count > 0)
            {
                return childList[0];
            }

            return null;
        }
    }
}