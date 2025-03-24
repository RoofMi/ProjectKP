namespace Character
{
    public abstract class PlayerState
    {
        protected PlayerStateMachine StateMachine;
        protected PlayerMovement Movement;

        public PlayerState(PlayerStateMachine stateMachine, PlayerMovement movement)
        {
            StateMachine = stateMachine;
            Movement = movement;
        }

        public abstract void OnEnter();

        public abstract void OnUpdate();
        public abstract void OnExit();
    }

}
