using UnityEngine;

namespace Character
{
    public class DashState : IState
    {
        private readonly CharacterStateMachine.CharacterContext _context;
        private float _dashTimer;
        private Vector2 _dashDirection;
        private Vector2 _lastMoveInput;

        public DashState(CharacterStateMachine.CharacterContext context, Vector2 initialDirection)
        {
            _context = context;
            _lastMoveInput = initialDirection;
        }

        public void OnEnter()
        {
            _dashTimer = _context.Movement.DashDuration;
            _dashDirection = _lastMoveInput;

            _context.Animator.SetTrigger("dashStartTrigger");
        }

        public StateTransition Update()
        {
            // 애니메이션 전환 중 방향값 손실 방지를 위한 안전장치
            if (_dashDirection.magnitude < 0.1f)
            {
                _dashDirection = _lastMoveInput;
                if (_dashDirection.magnitude < 0.1f)
                {
                    return new StateTransition(StateType.Move);
                }
            }

            _dashTimer -= Time.deltaTime;

            if (_dashTimer <= 0f)
            {
                return new StateTransition(StateType.Move);
            }

            _context.Movement.UpdateMovement(_dashDirection, Time.deltaTime, true);
            _context.Movement.UpdateRotation(Time.deltaTime, true);

            return null;
        }

        public StateTransition HandleInput(InputData input)
        {
            switch (input.Type)
            {
                case InputType.Movement:
                    _lastMoveInput = input.Direction;
                    break;
            }

            return null;
        }

        public void OnExit()
        {
            _context.Animator.SetTrigger("dashEndTrigger");
        }
    }
}