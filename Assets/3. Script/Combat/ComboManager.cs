using UnityEngine;
using System.Collections.Generic;
using Character;
using Actions;
using Combat.Interfaces;

namespace Combat
{
    public class ComboManager : MonoBehaviour
    {
        private RuntimeComboNode _comboRoot;
        
        private RuntimeComboNode _currentNode;
        private bool _isInComboWindow;
        private float _lastAttackTime;
        private IDamageable _comboTarget;
        private int _comboDepth;
        private const float COMBO_TIMEOUT = 1.0f;
        
        [Header("Dependencies")]
        [SerializeField] private ActionController _actionController;
        [SerializeField] private ComboAction _comboAction;

        public RuntimeComboNode RootNode => _comboRoot;
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

            if (_currentNode != null && !_isInComboWindow && Time.time - _lastAttackTime > COMBO_TIMEOUT)
            {
                ResetCombo();
            }
        }
        
        public void SetWeaponCombos(ComboDefinition[] weaponCombos)
        {
            _comboRoot = weaponCombos == null || weaponCombos.Length == 0
                ? null
                : ComboTreeBuilder.BuildTree(weaponCombos);
        }

        public RuntimeComboNode GetNextComboNode(
            RuntimeComboNode currentNode,
            string inputKey)
        {
            if (currentNode == null ||
                !currentNode.Children.TryGetValue(inputKey, out List<RuntimeComboNode> childNodes) ||
                childNodes.Count == 0)
            {
                return null;
            }

            return childNodes[0];
        }

        public RuntimeComboNode GetFirstComboNode(string inputKey)
        {
            if (_comboRoot == null ||
                !_comboRoot.Children.TryGetValue(inputKey, out List<RuntimeComboNode> childNodes) ||
                childNodes.Count == 0)
            {
                return null;
            }

            return childNodes[0];
        }
        
        public bool TryExecuteCombo(string inputKey)
        {
            if (_actionController == null || _comboAction == null)
            {
                Debug.LogWarning(
                    $"[ComboManager] Missing components - ActionController: {_actionController != null}, ComboAction: {_comboAction != null}",
                    this);
                return false;
            }

            if (_actionController.HasTag(Character.Core.ActionTags.Stunned))
            {
                return false;
            }

            RuntimeComboNode targetNode = _currentNode == null
                ? GetFirstComboNode(inputKey)
                : _isInComboWindow
                    ? GetNextComboNode(_currentNode, inputKey)
                    : null;

            if (targetNode == null)
            {
                return false;
            }

            StaminaComponent stamina = GetComponent<StaminaComponent>();
            if (stamina != null && !stamina.CanAfford(targetNode.StaminaCost))
            {
                Debug.LogWarning("[ComboManager] Not enough stamina.", this);
                return false;
            }

            if (!_actionController.TryExecuteAction(_comboAction, targetNode))
            {
                Debug.LogWarning("[ComboManager] Action execution failed.", this);
                return false;
            }

            stamina?.UseStamina(targetNode.StaminaCost);
            _currentNode = targetNode;
            _lastAttackTime = Time.time;
            _comboDepth++;
            return true;
        }
        
        public void SetComboWindow(bool active)
        {
            _isInComboWindow = active;
            
            if (!active && _currentNode != null)
            {
                var inputHandler = GetComponent<PlayerInputHandler>();
                inputHandler?.ClearInputBuffer();
            }
        }
        
        public void OnComboEnd()
        {
            _currentNode = null;
            _isInComboWindow = false;
            _comboDepth = 0;
            _comboTarget = null;
        }
        
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
