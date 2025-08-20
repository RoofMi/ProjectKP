using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/MoveToTargetAction")]
    public class MoveToTargetAIAction : AIAction
    {
        public override void Init(AIContext context)
        {
        }
        
        public override void Execute(AIContext context)
        {
            context.SetAgentDestinationToTarget();
        }
    }
}
