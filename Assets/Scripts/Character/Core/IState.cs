namespace Character
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        StateTransition Update();
        StateTransition HandleInput(InputData input);
    }
}