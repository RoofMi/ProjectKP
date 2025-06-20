using UnityEngine;

namespace Character
{
    public class DashState : CharacterState
    {
        private float _dashTimer;
        private Vector2 _dashDirection;
        private Vector2 _moveInput;

        public DashState(CharacterStateMachine stateMachine)
            : base(stateMachine)
        {
        }

        public override void OnEnter()
        {
            _dashTimer = Movement.DashDuration;
            _dashDirection = _moveInput;
            
            StateMachine.SetAnimatorTrigger("dashStartTrigger");
        }

        public override void OnUpdate()
        {
            if (_dashDirection.magnitude < 0.1f)
            {
                _dashDirection = _moveInput;
                return;
            }
            
            _dashTimer -= Time.deltaTime;

            if (_dashTimer < 0f)
            {
                StateMachine.SetState(new MoveState(StateMachine));
                return;
            }
            
            Movement.UpdateMovement(_dashDirection, Time.deltaTime, true);
            Movement.UpdateRotation(Time.deltaTime, true);
        }

        public override void OnExit()
        {
            StateMachine.SetAnimatorTrigger("dashEndTrigger");
        }

        public override void HandleMoveInput(Vector2 inputValue)
        {
            _moveInput = inputValue;
        }
    }

}