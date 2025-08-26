using Actions.Core;
using Character;
using Character.Core;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "JumpAction", menuName = "Actions/Movement/Jump")]
    public class JumpAction : InstantAction
    {
        private void Reset()
        {
            actionName = "Jump";
            description = "Basic jump action";
            cooldown = 0f;
            priority = 20;
            staminaCost = 10f;
            requiredTags = new string[] { ActionTags.Grounded };
            blockingTags = new string[] { ActionTags.Stunned };
        }
        
        public override bool CanExecute(ActionContext context)
        {
            if (!base.CanExecute(context))
                return false;
                
            return context.ActionController.HasTag(ActionTags.Grounded);
        }
        
        protected override void OnExecute(ActionContext context)
        {
            context.Movement.StartJump();
            context.Animator.SetTrigger(AnimationHashes.Jump);
            context.Animator.SetBool(AnimationHashes.IsGrounded, false);
            
            context.ActionController.RemoveTag(ActionTags.Grounded);
            context.ActionController.AddTag(ActionTags.Airborne);
        }
    }
}