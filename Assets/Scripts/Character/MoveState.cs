using UnityEngine;

namespace Character
{
    public class MoveState : CharacterState
    {
        // private float _moveSpeed;
        private Vector2 _moveInput;

        public MoveState(CharacterStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            StateMachine.SetAnimatorBool("isGrounded", true);
        }

        public override void OnUpdate()
        {   
            // AI
            if (StateMachine.bIsAI && StateMachine.Context.Agent is not null)
            {
                float speed = StateMachine.Context.Agent.velocity.magnitude;
                StateMachine.SetAnimatorFloat("speed", speed);
            }
            // Player
            else if (Movement is not null)
            {
                Movement.UpdateMovement(_moveInput, Time.deltaTime);
                Movement.UpdateRotation(Time.deltaTime);
                StateMachine.SetAnimatorFloat("speed", Movement.GetSpeed());
            }
        }

        public override void OnExit()
        {
            // TODO: 종료시
        }

        public override void HandleMoveInput(Vector2 inputValue)
        {
            _moveInput = inputValue;
        }

        public override bool CanJump() => true;

        public override bool CanDash()
        {
            return !(_moveInput.magnitude < 0.1f);
        }
    }

}
