using Actions.Core;
using Character.Core;
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
        
        // Use wrapped action's priority if available
        public override int Priority => wrappedAction != null ? wrappedAction.priority : base.Priority;
        
        public override void Init(AIContext context)
        {
            
        }
        
        // Override CalculateUtility to check cooldown
        public override float CalculateUtility(AIContext context)
        {
            // If the wrapped action is on cooldown, return 0 utility
            if (wrappedAction != null && context.IsActionOnCooldown(wrappedAction))
            {
                return 0f;
            }
            
            // Otherwise use the normal consideration evaluation
            return base.CalculateUtility(context);
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
            
            // Don't try to execute if already executing a blocking action
            if (context.ActionController.HasTag(ActionTags.ExecutingAction))
            {
                return;
            }
            
            object data = null;
            
            Debug.Log($"[WrapperAIAction] Wrapped action type: {wrappedAction?.GetType()?.FullName}, is DashAction: {wrappedAction is Actions.DashAction}");
            
            // Special handling for DashAction - provide direction data
            if (wrappedAction is Actions.DashAction)
            {
                if (context.CurrentTarget != null)
                {
                    Vector3 toTarget = context.CurrentTarget.position - context.Brain.transform.position;
                    toTarget.y = 0; // Y축 제거하여 수평 방향만 계산
                    Vector3 direction = toTarget.normalized;
                    
                    Debug.Log($"[WrapperAIAction] DashAction - Original direction to target: {direction}, invertDirection: {invertDirection}");
                    
                    if (invertDirection)
                    {
                        direction = -direction;
                        Debug.Log($"[WrapperAIAction] Direction inverted: {direction}");
                    }
                    
                    // 정규화된 방향을 Vector2로 변환
                    data = new Vector2(direction.x, direction.z).normalized;
                    Debug.Log($"[WrapperAIAction] Final Vector2 data: {data}");
                }
            }
            // Other actions that need data can be added here in the future
            
            // Execute with or without data based on validity
            bool executionSuccess;
            if (data != null)
            {
                executionSuccess = context.ActionController.TryExecuteAction(wrappedAction, data);
            }
            else
            {
                executionSuccess = context.ActionController.TryExecuteAction(wrappedAction);
            }
            
            // Handle execution failure (e.g., due to cooldown)
            if (!executionSuccess)
            {
                // Force re-evaluation on next frame by setting a flag in context
                context.SetData("ForceReEvaluation", true);
            }
        }
    }
}