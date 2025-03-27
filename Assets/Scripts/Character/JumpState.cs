using UnityEngine;

namespace Character
{
    public class JumpState : CharacterState
    {
        private float _verticalVelocity;
        private Vector2 _moveInput;

        public JumpState(CharacterStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            Movement.StartJump();
            StateMachine.SetAnimatorTrigger("jumpTrigger");
            StateMachine.SetAnimatorBool("isGrounded", false);
        }

        public override void OnUpdate()
        {
            Movement.UpdateMovement(_moveInput, Time.deltaTime);
            Movement.UpdateRotation(Time.deltaTime);
            
            if (Movement.IsGrounded())
            {
                StateMachine.SetState(new MoveState(StateMachine));
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
    }

}