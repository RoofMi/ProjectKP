using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class ComboManager : MonoBehaviour
    {
        [Header("Test Combos (임시)")]
        [SerializeField] private ComboDefinition[] testCombos;

        private RuntimeComboTree comboTree;
        private RuntimeComboNode currentNode;

        public RuntimeComboNode CurrentNode => currentNode;
        public RuntimeComboNode RootNode => comboTree?.Root;

        void Start()
        {
            // 테스트용 콤보로 시작
            if (testCombos != null && testCombos.Length > 0)
            {
                SetWeaponCombos(testCombos);
            }
            else
            {
                Debug.LogWarning("ComboManager: No test combos assigned!");
            }
        }
        
		// 무기마다 고정적으로 할당된 콤보가 있다는 가정 하에 구현된 함수
        public void SetWeaponCombos(ComboDefinition[] weaponCombos)
        {
            if (weaponCombos == null || weaponCombos.Length == 0)
            {
                Debug.LogWarning("ComboManager: No combos provided for weapon");
                return;
            }
            
            comboTree = ComboTreeBuilder.BuildTree(weaponCombos);
            ResetCombo();
            
            Debug.Log($"ComboManager: Loaded {weaponCombos.Length} combos");
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