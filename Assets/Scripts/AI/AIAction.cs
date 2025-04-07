using AI.ScriptableObject;
using UnityEngine;

namespace AI
{
    public abstract class AIAction : UnityEngine.ScriptableObject
    {
        [SerializeField] private string _targetTag;
        [SerializeField]private Consideration _consideration;

        public virtual void Init(AIContext context)
        {
            // Optional init logic
        }

        public float CalculateUtility(AIContext context) => _consideration.Evaluate(context);

        public abstract void Execute(AIContext context);
    }
}
