using UnityEngine;

namespace Character
{
    public class PlayerStateMachine
    {
        private PlayerMovement _movement;
        private PlayerState _currentState;

        public PlayerStateMachine(PlayerMovement movement)
        {
            _movement = movement;

            _currentState = new MoveState(this, _movement, _movement.MoveSpeed);
            _currentState.OnEnter();
        }

        public void OnUpdate()
        {
            _currentState?.OnUpdate();
        }

        public void SetState(PlayerState newState)
        {
            _currentState?.OnExit();
            _currentState = newState;
            _currentState?.OnEnter();
        }
    }

}
