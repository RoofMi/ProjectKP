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

        public override void HandleComboInput()
        {
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime;
            
            if (normalizedTime is <= 0.5f or >= 0.95f)
            {
                return;
            }
            
            StateMachine.SetAnimatorTrigger("comboTrigger");
        }
    }
}
