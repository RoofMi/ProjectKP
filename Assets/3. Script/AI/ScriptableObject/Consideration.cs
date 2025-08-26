using UnityEngine;

namespace AI.ScriptableObject
{
    public abstract class Consideration : UnityEngine.ScriptableObject
    {
        public abstract float Evaluate(AIContext context);
    }
}
