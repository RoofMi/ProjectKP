using UnityEngine;
using System.Collections;
using Character;

namespace ProjectKP.Actions
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
            blockingTags = new string[] { "Stunned", "Dashing" };
            
            dashSpeed = 20f;
            dashDuration = 0.3f;
            dashStartDelay = 0.1f;
        }
        
        public override bool CanExecute(GameObject owner)
        {
            if (!base.CanExecute(owner))
                return false;
                
            var controller = owner.GetComponent<ActionController>();
            
            if (controller.IsActionActive(typeof(DashAction)))
                return false;
            
            if (controller.HasTag("Stunned"))
                return false;
            
            if (controller.HasTag("Airborne") && controller.HasTag("AirDashUsed"))
                return false;
            
            return true;
        }
        
        public override IEnumerator ExecuteOverTime(GameObject owner, object data = null)
        {
            var controller = owner.GetComponent<ActionController>();
            var movement = owner.GetComponent<CharacterMovement>();
            var animator = owner.GetComponent<Animator>();
            
            bool isAirDash = controller.HasTag("Airborne");
            
            if (isAirDash)
            {
                controller.AddTag("AirDashUsed");
                movement.SetGravityEnabled(false);
            }
            
            controller.AddTag("Dashing");
            animator.SetTrigger(AnimationHashes.DashStart);
            
            Vector2 dashDirection = data as Vector2? ?? new Vector2(0, 0);
            if (dashDirection.magnitude < 0.1f)
            {
                Vector3 forward = owner.transform.forward;
                dashDirection = new Vector2(forward.x, forward.z).normalized;
            }
            
            Vector3 worldDashDirection = new Vector3(dashDirection.x, 0, dashDirection.y);
            if (movement.CameraTransform != null)
            {
                Vector3 camForward = movement.CameraTransform.forward;
                Vector3 camRight = movement.CameraTransform.right;
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
            movement.SetDashVelocity(dashVelocity);
            
            while (elapsed < dashDuration)
            {
                movement.UpdateRotation(Time.deltaTime, true);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            movement.ResetDashVelocity();
            
            controller.RemoveTag("Dashing");
            animator.SetTrigger(AnimationHashes.DashEnd);
            
            if (isAirDash)
            {
                movement.SetGravityEnabled(true);
                movement.ResetVerticalVelocity();
            }
        }
    }
}