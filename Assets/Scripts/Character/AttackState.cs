using UnityEngine;

namespace Character
{
    public class AttackState : CharacterState
    {
        public AttackState(CharacterStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            StateMachine.SetAnimatorTrigger("qSkillStartTrigger");
        }

        public override void OnUpdate()
        {
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsTag("PreAttack"))
            {
                return;
            }
            
            float normalizedTime = stateInfo.normalizedTime;
            
            // 콤보 추가입력 가능 구간
            if (normalizedTime is >= 0.8f or < 0.95f)
            {
                string nextInput = StateMachine.InputBuffer.GetNextInput();
                if (nextInput is "Q" or "Click")
                {
                    StateMachine.SetAnimatorTrigger("comboTrigger");
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
