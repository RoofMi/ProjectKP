using AI.ScriptableObject;
using UnityEngine;

namespace AI
{
    public abstract class AIAction : UnityEngine.ScriptableObject
    {
        [SerializeField] private Consideration _consideration;

        [Header("Priority")]
        [Range(0, 100)]
        [SerializeField] protected int priority = 10;

        public virtual int Priority => priority;

        public virtual void Init(AIContext context)
        {
        }

        public virtual float CalculateUtility(AIContext context)
        {
            return _consideration != null ? _consideration.Evaluate(context) : 0f;
        }

        public abstract void Execute(AIContext context);
    }
}
