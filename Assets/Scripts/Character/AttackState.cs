using UnityEngine;
using Combat;

namespace Character
{
    public class AttackState : IState
    {
        private readonly CharacterStateMachine.CharacterContext _context;
        private RuntimeComboNode _currentComboNode;
        private bool _hasCheckedComboWindow;
        private bool _isInComboWindow = false;
        private float _windowStart;
        private float _windowEnd;
        private bool _isTransitionComplete = false;
        private string _attackAnimName;
        
        public AttackState(CharacterStateMachine.CharacterContext context, RuntimeComboNode comboNode)
        {
            _context = context;
            _currentComboNode = comboNode;
            CacheWindowValues();
        }
        
        private void CacheWindowValues()
        {
            if (_currentComboNode != null)
            {
                _windowStart = _currentComboNode.WindowStart > 0 ? _currentComboNode.WindowStart : 0.5f;
                _windowEnd = _currentComboNode.WindowEnd > 0 ? _currentComboNode.WindowEnd : 0.9f;
            }
        }
        
        public void OnEnter()
        {
            _hasCheckedComboWindow = false;
            _isTransitionComplete = false;
            
            if (_currentComboNode?.StepNode?.AnimClip == null)
            {
                Debug.LogWarning("No animation clip found for current combo node!");
                return;
            }
            
            _attackAnimName = _currentComboNode.StepNode.AnimClip.name;
            _context.Animator.CrossFade(_attackAnimName, 0.1f);
        }
        
        public StateTransition Update()
        {
            AnimatorStateInfo stateInfo;
            float normalizedTime;
            
            if (!_isTransitionComplete)
            {
                bool isInTransition = _context.Animator.IsInTransition(0);
                
                if (isInTransition)
                {
                    var nextState = _context.Animator.GetNextAnimatorStateInfo(0);
                    if (!nextState.IsName(_attackAnimName))
                        return null;
                    
                    stateInfo = nextState;
                    normalizedTime = nextState.normalizedTime;
                }
                else
                {
                    _isTransitionComplete = true;
                    stateInfo = _context.Animator.GetCurrentAnimatorStateInfo(0);
                    
                    if (!stateInfo.IsName(_attackAnimName))
                        return null;
                        
                    normalizedTime = stateInfo.normalizedTime;
                }
            }
            else
            {
                stateInfo = _context.Animator.GetCurrentAnimatorStateInfo(0);
                normalizedTime = stateInfo.normalizedTime;
            }
            
            if (_currentComboNode != null)
            {
                bool wasInWindow = _isInComboWindow;
                _isInComboWindow = normalizedTime >= _windowStart && normalizedTime <= _windowEnd;
                
                if (wasInWindow && !_isInComboWindow && !_hasCheckedComboWindow)
                {
                    _hasCheckedComboWindow = true;
                    _context.InputBuffer.ClearAllInputs();
                }
            }
            
            if (_isTransitionComplete && normalizedTime >= 0.95f)
            {
                // Stateless: ComboManager는 더 이상 상태를 추적하지 않음
                return new StateTransition(StateType.Move);
            }
            
            return null;
        }
        
        public StateTransition HandleInput(InputData input)
        {
            switch (input.Type)
            {
                case InputType.Attack:
                    if (!_isInComboWindow || _currentComboNode == null)
                        return null;
                    
                    // Stateless: 현재 노드를 전달하여 다음 노드 얻기
                    var nextNode = _context.ComboManager.GetNextComboNode(_currentComboNode, input.Key);
                    if (nextNode != null && _context.Stamina.TryUseStamina(nextNode.StaminaCost))
                    {
                        UpdateToNextCombo(nextNode);
                    }
                    break;
                    
                case InputType.Movement:
                    // TODO: 이동 캔슬 기능은 나중에 구현
                    // if (_currentComboNode?.IsCancelable == true)
                    // {
                    //     return new StateTransition(StateType.Move);
                    // }
                    break;
            }
            
            return null;
        }
        
        public void OnExit()
        {
            _context.Animator.CrossFade("Idle/Run", 0.1f);
        }
        
        private void UpdateToNextCombo(RuntimeComboNode nextNode)
        {
            _currentComboNode = nextNode;
            _hasCheckedComboWindow = false;
            _isTransitionComplete = false;
            CacheWindowValues();
            
            if (nextNode?.StepNode?.AnimClip != null)
            {
                _attackAnimName = nextNode.StepNode.AnimClip.name;
                _context.Animator.CrossFade(_attackAnimName, 0.1f);
            }
        }
    }
}