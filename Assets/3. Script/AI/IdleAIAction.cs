using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/IdleAction")]
    public class IdleAIAction : AIAction
    {
        private void Reset()
        {
            priority = 0;
        }

        public override void Execute(AIContext context)
        {
            context.Navigation.SetDestination(context.Brain.transform.position);
        }
    }
}
