using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/IdleAction")]
    public class IdleAIAction : AIAction
    {
        public override void Init(AIContext context)
        {
        }

        public override void Execute(AIContext context)
        {
            context.Agent.SetDestination(context.Agent.transform.position);
        }
    }
}
