using System.Collections;
using Actions.Core;
using Character;
using Character.Core;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "DashAction", menuName = "Actions/Movement/Dash")]
    public class DashAction : DurationAction
    {
        [Header("Dash Settings")]
        public float dashSpeed;
        public float dashDuration;
        public float dashStartDelay = 0.1f;
        
        private void Reset()
        {
            actionName = "Dash";
            description = "Quick dash in movement direction";
            cooldown = 0.5f;
            priority = 30;
            staminaCost = 20f;
            requiredTags = null;
            blockingTags = new string[] { ActionTags.Stunned, ActionTags.Dashing };
            
            dashSpeed = 20f;
            dashDuration = 0.3f;
            dashStartDelay = 0.1f;
        }
        
        public override bool CanExecute(ActionContext context)
        {
            if (!base.CanExecute(context))
                return false;
                
            if (context.ActionController.IsActionActive(typeof(DashAction)))
                return false;
            
            if (context.ActionController.HasTag(ActionTags.Stunned))
                return false;
            
            if (context.ActionController.HasTag(ActionTags.Airborne) && context.ActionController.HasTag(ActionTags.AirDashUsed))
                return false;
            
            return true;
        }
        
        public override IEnumerator ExecuteOverTime(ActionContext context, ActiveAction activeAction)
        {
            bool isAirDash = context.ActionController.HasTag(ActionTags.Airborne);
            
            if (isAirDash)
            {
                context.ActionController.AddTag(ActionTags.AirDashUsed);
                context.Movement.SetGravityEnabled(false);
            }
            
            context.ActionController.AddTag(ActionTags.Dashing);
            context.Animator.SetTrigger(AnimationHashes.DashStart);
            
            Vector2 dashDirection = activeAction.Data as Vector2? ?? new Vector2(0, 0);
            if (dashDirection.magnitude < 0.1f)
            {
                Vector3 forward = context.Owner.transform.forward;
                dashDirection = new Vector2(forward.x, forward.z).normalized;
            }
            
            Vector3 worldDashDirection = new Vector3(dashDirection.x, 0, dashDirection.y);
            if (context.Movement.CameraTransform != null)
            {
                Vector3 camForward = context.Movement.CameraTransform.forward;
                Vector3 camRight = context.Movement.CameraTransform.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();
                worldDashDirection = (camForward * dashDirection.y + camRight * dashDirection.x).normalized;
            }
            
            float elapsed = 0;
            
            if (dashStartDelay > 0)
            {
                yield return new WaitForSeconds(dashStartDelay);
            }
            
            Vector3 dashVelocity = worldDashDirection * dashSpeed;
            context.Movement.SetDashVelocity(dashVelocity);
            
            while (elapsed < dashDuration)
            {
                context.Movement.UpdateRotation(Time.deltaTime, true);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            context.Movement.ResetDashVelocity();
            
            context.ActionController.RemoveTag(ActionTags.Dashing);
            context.Animator.SetTrigger(AnimationHashes.DashEnd);
            
            if (isAirDash)
            {
                context.Movement.SetGravityEnabled(true);
                context.Movement.ResetVerticalVelocity();
            }
        }
    }
}