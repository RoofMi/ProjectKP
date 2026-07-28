using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/MoveToTargetAction")]
    public class MoveToTargetAIAction : AIAction
    {
        [SerializeField, Min(0f)] private float combatStopDistance = 1.65f;

        private void Reset()
        {
            priority = 5;
        }

        public override void Execute(AIContext context)
        {
            if (context.CurrentTarget != null)
            {
                context.Navigation.SetDestination(
                    context.CurrentTarget.position,
                    combatStopDistance);
            }
        }
    }
}
