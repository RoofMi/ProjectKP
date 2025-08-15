using UnityEngine;

namespace Actions.Core
{
    [System.Serializable]
    public class ActiveAction
    {
        public ActionBase Action { get; private set; }
        public object Data { get; private set; }
        public float StartTime { get; private set; }
        public Coroutine Coroutine { get; set; }
        public Vector2 InputDirection { get; private set; }
        
        public ActiveAction(ActionBase action, object data, Vector2 inputDirection)
        {
            Action = action;
            Data = data;
            StartTime = Time.time;
            InputDirection = inputDirection;
        }
        
        public float ElapsedTime => Time.time - StartTime;
    }
}