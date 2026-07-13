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

        [Min(0f)]
        [SerializeField] private float maxTargetDistance;

        [Min(0f)]
        [SerializeField] private float minTargetDistance;

        [Min(0f)]
        [SerializeField] private float minimumReuseDelay;

        [SerializeField] private bool requiresTargetOpportunity;
        
        // Use wrapped action's priority if available
        public override int Priority => wrappedAction != null ? wrappedAction.priority : base.Priority;
        
        public override void Init(AIContext context)
        {
            
        }
        
        // Override CalculateUtility to check cooldown
        public override float CalculateUtility(AIContext context)
        {
            if (context.CurrentTarget == null)
            {
                return 0f;
            }

            float targetDistance = Vector3.Distance(context.Brain.transform.position, context.CurrentTarget.position);
            if (minTargetDistance > 0f && targetDistance < minTargetDistance)
            {
                return 0f;
            }

            if (maxTargetDistance > 0f && targetDistance > maxTargetDistance)
            {
                return 0f;
            }

            if (requiresTargetOpportunity && context.GetData<float>(ContextKeys.TargetOpportunity) < 0.5f)
            {
                return 0f;
            }

            if (minimumReuseDelay > 0f &&
                context.GetData<float>(ContextKeys.TimeSinceLastDash) < minimumReuseDelay)
            {
                return 0f;
            }

            if (wrappedAction != null && context.IsActionOnCooldown(wrappedAction))
            {
                return 0f;
            }

            return base.CalculateUtility(context);
        }
        
        public override void Execute(AIContext context)
        {
            if (CalculateUtility(context) <= 0f)
            {
                return;
            }

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
            
            
            // Special handling for DashAction - provide direction data
            if (wrappedAction is Actions.DashAction)
            {
                if (context.CurrentTarget != null)
                {
                    Vector3 toTarget = context.CurrentTarget.position - context.Brain.transform.position;
                    toTarget.y = 0; // Y축 제거하여 수평 방향만 계산
                    Vector3 direction = toTarget.normalized;
                    
                    
                    if (invertDirection)
                    {
                        direction = -direction;
                    }
                    
                    // 정규화된 방향을 Vector2로 변환
                    data = new Vector2(direction.x, direction.z).normalized;
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
            else if (wrappedAction is Actions.DashAction)
            {
                context.Brain.NotifyDashExecuted();
            }
        }
    }
}
