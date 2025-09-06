using AI.ScriptableObject;
using UnityEngine;

namespace AI
{
    public abstract class AIAction : UnityEngine.ScriptableObject
    {
        [SerializeField] private string _targetTag;
        [SerializeField] private Consideration _consideration;
        
        [Header("Priority")]
        [Range(0, 100)]
        [SerializeField] protected int priority = 10;
        
        public virtual int Priority => priority;

        public virtual void Init(AIContext context)
        {
        }

        public virtual float CalculateUtility(AIContext context) => _consideration.Evaluate(context);

        public abstract void Execute(AIContext context);
    }
}
