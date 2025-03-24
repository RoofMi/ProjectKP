using UnityEngine;

namespace Character
{
    public class MoveState : PlayerState
    {
        private float _moveSpeed;

        public MoveState(PlayerStateMachine stateMachine, PlayerMovement movement, float moveSpeed)
            : base(stateMachine, movement)
        {
            _moveSpeed = moveSpeed;
        }

        public override void OnEnter()
        {
            
        }

        public override void OnUpdate()
        {
            Vector2 input2D = Movement.GetMoveInput();

            Vector3 moveDirection = Movement.GetCameraAlignedDirection(input2D);
            
            Movement.SetHorizontalVelocity(moveDirection * _moveSpeed);
            
            if (Movement.IsDashTriggered() && moveDirection.sqrMagnitude > 0.01f)
            {
                StateMachine.SetState(new DashState(StateMachine, Movement));
                return;
            }

            if (Movement.IsJumpTriggered() && Movement.IsGrounded())
            {
                StateMachine.SetState(new JumpState(StateMachine, Movement));
                return;
            }
        }

        public override void OnExit()
        {
            // TODO: 종료시
        }
    }

}
