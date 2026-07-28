using Actions;
using Character.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/DashAction", fileName = "DashAIAction")]
    public class DashAIAction : AIAction
    {
        public enum DashDirection
        {
            TowardTarget,
            AwayFromTarget
        }

        [Header("Dash")]
        [FormerlySerializedAs("wrappedAction")]
        [SerializeField] private DashAction _dashAction;

        [FormerlySerializedAs("invertDirection")]
        [SerializeField] private DashDirection _direction;

        [Min(0f)]
        [SerializeField] private float maxTargetDistance;

        [Min(0f)]
        [SerializeField] private float minTargetDistance;

        [Min(0f)]
        [SerializeField] private float minimumReuseDelay;

        [SerializeField] private bool requiresTargetOpportunity;

        public override int Priority => _dashAction != null ? _dashAction.priority : base.Priority;

        public override void Init(AIContext context)
        {
            if (_dashAction == null)
            {
                Debug.LogError($"[DashAIAction] DashAction is not assigned on {name}.", this);
            }
        }

        public override float CalculateUtility(AIContext context)
        {
            if (_dashAction == null || context.CurrentTarget == null)
            {
                return 0f;
            }

            float targetDistance =
                Vector3.Distance(context.Brain.transform.position, context.CurrentTarget.position);

            if (minTargetDistance > 0f && targetDistance < minTargetDistance)
            {
                return 0f;
            }

            if (maxTargetDistance > 0f && targetDistance > maxTargetDistance)
            {
                return 0f;
            }

            if (requiresTargetOpportunity &&
                context.GetFloat(ContextKeys.TargetOpportunity) < 0.5f)
            {
                return 0f;
            }

            if (minimumReuseDelay > 0f &&
                context.GetFloat(ContextKeys.TimeSinceLastDash) < minimumReuseDelay)
            {
                return 0f;
            }

            if (context.IsActionOnCooldown(_dashAction))
            {
                return 0f;
            }

            return base.CalculateUtility(context);
        }

        public override void Execute(AIContext context)
        {
            if (CalculateUtility(context) <= 0f ||
                context.ActionController.HasTag(ActionTags.ExecutingAction))
            {
                return;
            }

            Vector3 direction =
                context.CurrentTarget.position - context.Brain.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            direction.Normalize();
            if (_direction == DashDirection.AwayFromTarget)
            {
                direction = -direction;
            }

            Vector2 input = new(direction.x, direction.z);
            if (context.ActionController.TryExecuteAction(_dashAction, input.normalized))
            {
                context.Brain.NotifyDashExecuted();
            }
        }
    }
}
