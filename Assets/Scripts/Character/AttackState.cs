using UnityEngine;
using Combat;

namespace Character
{
    public class AttackState : CharacterState
    {
        private readonly ComboManager _comboManager;
        private RuntimeComboNode _currentComboNode;
        private bool _hasCheckedComboWindow;
        
        public AttackState(CharacterStateMachine stateMachine, ComboManager comboManager, RuntimeComboNode comboNode)
            : base(stateMachine)
        {
            _comboManager = comboManager;
            _currentComboNode = comboNode;
        }

        public override void OnEnter()
        {
            _hasCheckedComboWindow = false;
            
            // 콤보 노드에 애니메이션이 있으면 재생
            if (_currentComboNode?.StepNode?.AnimClip != null)
            {
                string stateName = _currentComboNode.StepNode.AnimClip.name;
                StateMachine.PlayAnimation(stateName, 0.1f);
            }
            else
            {
                // 기본 공격 애니메이션 트리거
                StateMachine.SetAnimatorTrigger("qSkillStartTrigger");
            }
        }

        public override void OnUpdate()
        {
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsTag("PreAttack"))
            {
                return;
            }
            
            float normalizedTime = stateInfo.normalizedTime;
            
            // 콤보 추가입력 가능 구간 체크
            if (_currentComboNode != null)
            {
                float windowStart = _currentComboNode.WindowStart > 0 ? _currentComboNode.WindowStart : 0.5f;
                float windowEnd = _currentComboNode.WindowEnd > 0 ? _currentComboNode.WindowEnd : 0.9f;
                
                if (normalizedTime >= windowStart && normalizedTime <= windowEnd)
                {
                    // 매 프레임 InputBuffer 체크 (연타 입력 대응)
                    string nextInput = StateMachine.InputBuffer.GetNextInput();
                    
                    if (!string.IsNullOrEmpty(nextInput))
                    {
                        var nextNode = _comboManager.TryAdvanceCombo(nextInput);
                        if (nextNode != null)
                        {
                            // 다음 콤보로 전환
                            StateMachine.SetState(new AttackState(StateMachine, _comboManager, nextNode));
                            return;
                        }
                        // 잘못된 입력은 무시하고 다음 입력 확인
                    }
                }
                else if (normalizedTime > windowEnd && !_hasCheckedComboWindow)
                {
                    // 윈도우를 놓쳤을 때 버퍼 확인
                    _hasCheckedComboWindow = true;
                    StateMachine.InputBuffer.ClearAllInputs();
                }
            }
            
            if (normalizedTime >= 1f)
            {
                StateMachine.HandleComboEnd();
            }
        }

        public override void OnExit()
        {
        }

        public override void HandleMoveInput(Vector2 inputValue)
        {  
            
        }
    }
}
