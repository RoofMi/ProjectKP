using UnityEngine;

namespace Character
{
    public class JumpState : PlayerState
    {
        private float _verticalVelocity;

        public JumpState(PlayerStateMachine stateMachine, PlayerMovement movement)
            : base(stateMachine, movement)
        {
        }

        public override void OnEnter()
        {
            Movement.SetVerticalVelocity(Movement.JumpPower);
            Movement.SetAnimatorTrigger("jumpTrigger");
        }

        public override void OnUpdate()
        {
            if (Movement.IsGrounded())
            {
                StateMachine.SetState(new MoveState(StateMachine, Movement, Movement.MoveSpeed));
            }
        }

        public override void OnExit()
        {
            // TODO: 종료시
        }
    }

}