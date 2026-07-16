using UnityEngine;
using System.Collections.Generic;
using Character;
using Actions;
using Combat.Interfaces;

namespace Combat
{
    public class ComboManager : MonoBehaviour
    {
        private RuntimeComboTree comboTree;
        
        // 콤보 상태 관리
        private RuntimeComboNode _currentNode;
        private bool _isInComboWindow;
        private float _lastAttackTime;
        private IDamageable _comboTarget;
        private int _comboDepth = 0; // 현재 콤보 깊이 추적
        private const float COMBO_TIMEOUT = 1.0f; // 콤보 타임아웃 시간
        
        // 의존성
        [Header("Dependencies")]
        [SerializeField] private ActionController _actionController;
        [SerializeField] private ComboAction _comboAction;

        public RuntimeComboNode RootNode => comboTree?.Root;
        public RuntimeComboNode CurrentNode => _currentNode;
        public bool IsInComboWindow => _isInComboWindow;
        public Transform ComboTarget
        {
            get
            {
                if (!IsValidTarget(_comboTarget))
                {
                    _comboTarget = null;
                    return null;
                }

                return _comboTarget.Transform;
            }
        }
        
        private void Update()
        {
            if (!IsValidTarget(_comboTarget))
            {
                _comboTarget = null;
            }

            // 콤보 타임아웃 체크
            if (_currentNode != null && !_isInComboWindow && Time.time - _lastAttackTime > COMBO_TIMEOUT)
            {
                ResetCombo();
            }
        }
        
		// 무기마다 고정적으로 할당된 콤보가 있다는 가정 하에 구현된 함수
        public void SetWeaponCombos(ComboDefinition[] weaponCombos)
        {
            if (weaponCombos == null || weaponCombos.Length == 0)
            {
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
        
        // 콤보 실행 시도
        public bool TryExecuteCombo(string inputKey)
        {
            
            if (_actionController == null || _comboAction == null)
            {
                Debug.LogWarning($"[ComboManager] Missing components - ActionController: {_actionController != null}, ComboAction: {_comboAction != null}");
                return false;
            }

            if (_actionController.HasTag(Character.Core.ActionTags.Stunned))
            {
                return false;
            }

            RuntimeComboNode targetNode = null;
            
            // 현재 공격 중이 아닌 경우 - 새 콤보 시작
            if (_currentNode == null)
            {
                targetNode = GetFirstComboNode(inputKey);
            }
            // 콤보 윈도우 내에서 입력한 경우 - 콤보 연계
            else if (_isInComboWindow)
            {
                targetNode = GetNextComboNode(_currentNode, inputKey);
            }
            else
            {
                // 콤보 윈도우 밖에서의 입력은 무시
                return false;
            }

            // 실행 가능한 노드가 있으면 AttackAction 실행
            if (targetNode != null)
            {
                
                // 스태미나 체크 및 사용
                var staminaComponent = GetComponent<StaminaComponent>();
                if (staminaComponent != null && !staminaComponent.TryUseStamina(targetNode.StaminaCost))
                {
                    Debug.LogWarning("[ComboManager] Not enough stamina!");
                    return false;
                }
                
                bool result = _actionController.TryExecuteAction(_comboAction, targetNode);
                
                // TryExecuteAction이 성공했을 때만 currentNode 업데이트
                if (result)
                {
                    _currentNode = targetNode;
                    _lastAttackTime = Time.time;
                    _comboDepth++;
                }
                else
                {
                    Debug.LogWarning("[ComboManager] TryExecuteAction failed!");
                }
                
                return result;
            }
            else
            {
            }
            
            return false;
        }
        
        // AttackAction이 콤보 윈도우 상태를 알려줄 때 호출
        public void SetComboWindow(bool active)
        {
            _isInComboWindow = active;
            
            // 콤보 윈도우가 닫혔을 때 InputBuffer 정리
            if (!active && _currentNode != null)
            {
                var inputHandler = GetComponent<PlayerInputHandler>();
                inputHandler?.ClearInputBuffer();
            }
        }
        
        // AttackAction이 공격 종료를 알려줄 때 호출
        public void OnComboEnd()
        {
            _currentNode = null;
            _isInComboWindow = false;
            _comboDepth = 0;
            _comboTarget = null;
        }
        
        // 콤보가 실제로 끊겼을 때 (타임아웃, 다른 액션 등) 호출
        public void ResetCombo()
        {
            _currentNode = null;
            _isInComboWindow = false;
            _comboDepth = 0;
            _comboTarget = null;
        }

        public void RecordHitTarget(IDamageable target)
        {
            if (_currentNode == null || IsValidTarget(_comboTarget))
            {
                return;
            }

            _comboTarget = IsValidTarget(target) ? target : null;
        }

        private static bool IsValidTarget(IDamageable target)
        {
            if (target == null || target is Object unityObject && unityObject == null)
            {
                return false;
            }

            Transform targetTransform = target.Transform;
            return targetTransform != null && targetTransform.gameObject.activeInHierarchy && !target.IsDead;
        }

        public bool IsInCombo()
        {
            return _currentNode != null;
        }
        
        public int GetComboDepth()
        {
            return _comboDepth;
        }
    }
}
