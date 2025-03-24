using UnityEngine;

namespace Character
{
    public class DashState : PlayerState
    {
        private float _dashTimer;
        private Vector3 _dashDirection;

        public DashState(PlayerStateMachine stateMachine, PlayerMovement movement)
            : base(stateMachine, movement)
        {
        }

        public override void OnEnter()
        {
            _dashTimer = Movement.DashDuration;

            Vector2 input2D = Movement.GetMoveInput();

            _dashDirection = Movement.GetCameraAlignedDirection(input2D);

            Movement.SetAnimatorTrigger("dashStartTrigger");
        }

        public override void OnUpdate()
        {
            _dashTimer -= Time.deltaTime;
    
            Movement.SetHorizontalVelocity(_dashDirection * Movement.DashSpeed);

            if (_dashTimer <= 0f)
            {
                StateMachine.SetState(new MoveState(StateMachine, Movement, Movement.MoveSpeed));
            }
        }

        public override void OnExit()
        {
            Movement.SetAnimatorTrigger("dashEndTrigger");
        }
    }

}