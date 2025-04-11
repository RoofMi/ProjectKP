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
            
        }

        public override void OnExit()
        {
            
        }

        public override void HandleMoveInput(Vector2 inputValue)
        {  
            
        }

        public override void HandleComboInput()
        {
            StateMachine.SetAnimatorTrigger("comboTrigger");
        }
    }
}
