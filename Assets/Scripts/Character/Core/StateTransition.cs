namespace Character
{
    public class StateTransition
    {
        public StateType NextState { get; }
        public object TransitionData { get; }
        
        public StateTransition(StateType nextState, object data = null)
        {
            NextState = nextState;
            TransitionData = data;
        }
    }
    
    public enum StateType
    {
        Move,
        Jump,
        Attack
    }
}