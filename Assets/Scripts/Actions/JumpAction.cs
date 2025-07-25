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
            blockingTags = new string[] { "Stunned" };
        }
        
        public override bool CanExecute(GameObject owner)
        {
            if (!base.CanExecute(owner))
                return false;
                
            var controller = owner.GetComponent<ActionController>();
            
            return controller.HasTag(ActionTags.Grounded);
        }
        
        protected override void OnExecute(GameObject owner)
        {
            var movement = owner.GetComponent<CharacterMovement>();
            var animator = owner.GetComponent<Animator>();
            var controller = owner.GetComponent<ActionController>();
            
            movement.StartJump();
            animator.SetTrigger(AnimationHashes.Jump);
            animator.SetBool(AnimationHashes.IsGrounded, false);
            
            controller.RemoveTag(ActionTags.Grounded);
            controller.AddTag(ActionTags.Airborne);
        }
    }
}