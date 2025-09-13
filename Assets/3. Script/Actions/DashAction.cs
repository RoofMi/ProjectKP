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
        public float dashDuration = 0.5f;
        
        private void Reset()
        {
            actionName = "Dash";
            description = "Quick dash in movement direction";
            cooldown = 0.5f;
            priority = 30;
            staminaCost = 20f;
            requiredTags = null;
            blockingTags = new string[] { ActionTags.Stunned, ActionTags.Dashing };

            dashDuration = 0.5f;
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

            // 상태 태그 추가
            context.ActionController.AddTag(ActionTags.Dashing);
            context.ActionController.AddTag(ActionTags.ExecutingAction);
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
            else
            {
                // AI 캐릭터는 카메라가 없으므로 월드 방향 그대로 사용
                worldDashDirection = worldDashDirection.normalized;
            }

            // 대시 지속 시간 동안 대기 (루트모션이 실제 이동 처리)
            yield return new WaitForSeconds(dashDuration);

            // 상태 태그 제거
            context.ActionController.RemoveTag(ActionTags.Dashing);
            context.ActionController.RemoveTag(ActionTags.ExecutingAction);
            context.Animator.SetTrigger(AnimationHashes.DashEnd);

            if (isAirDash)
            {
                context.Movement.SetGravityEnabled(true);
                context.Movement.ResetVerticalVelocity();
            }
        }
    }
}