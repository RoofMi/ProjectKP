using Actions.Core;
using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/WrapperAction", fileName = "WrapperAIAction")]
    public class WrapperAIAction : AIAction
    {
        [Header("Player Action")]
        [SerializeField] private ActionBase wrappedAction;
        
        [Header("Dash Settings")]
        [Tooltip("For dash actions: if true, dash away from target; if false, dash towards target")]
        [SerializeField] private bool invertDirection;
        
        public override void Init(AIContext context)
        {
            
        }
        
        public override void Execute(AIContext context)
        {
            if (wrappedAction == null)
            {
                Debug.LogWarning("WrapperAIAction: No wrapped action assigned!");
                return;
            }
            
            if (context.ActionController == null)
            {
                Debug.LogError("WrapperAIAction: ActionController not found in context!");
                return;
            }
            
            object data = null;
            
            // Special handling for DashAction - provide direction data
            if (wrappedAction is Actions.DashAction)
            {
                if (context.CurrentTarget != null)
                {
                    Vector3 direction = (context.CurrentTarget.position - context.Brain.transform.position).normalized;
                    if (invertDirection)
                    {
                        direction = -direction;
                    }
                    data = new Vector2(direction.x, direction.z);
                }
            }
            // Other actions that need data can be added here in the future
            
            // Execute with or without data based on validity
            if (data != null)
            {
                context.ActionController.TryExecuteAction(wrappedAction, data);
            }
            else
            {
                context.ActionController.TryExecuteAction(wrappedAction);
            }
        }
    }
}