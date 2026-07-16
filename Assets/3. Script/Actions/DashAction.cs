using System.Collections;
using Actions.Core;
using Character;
using Character.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    [CreateAssetMenu(fileName = "DashAction", menuName = "Actions/Movement/Dash")]
    public class DashAction : DurationAction
    {
        [Header("Dash Settings")]
        public float dashSpeed = 15f;
        public float dashDuration = 0.5f;

        private const float NavMeshSampleRadius = 0.5f;
        
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
            var agent = context.Owner.GetComponent<NavMeshAgent>();
            bool isAiDash = agent != null;
            Vector3 destination = context.Owner.transform.position;

            if (isAiDash && !TryGetAiDashDestination(context.Owner.transform.position, worldDashDirection, agent.areaMask, out destination))
            {
                context.ActionController.RemoveTag(ActionTags.Dashing);
                context.ActionController.RemoveTag(ActionTags.ExecutingAction);
                context.Animator.SetTrigger(AnimationHashes.DashEnd);
                yield break;
            }

            if (isAiDash)
            {
                bool previousRootMotion = context.Animator.applyRootMotion;
                context.Animator.applyRootMotion = false;
                context.Movement.SetRotationToDirection(worldDashDirection);

                float elapsed = 0f;
                float speed = Vector3.Distance(context.Owner.transform.position, destination) / dashDuration;

                while (elapsed < dashDuration)
                {
                    Vector3 currentPosition = context.Owner.transform.position;
                    Vector3 nextPosition = Vector3.MoveTowards(currentPosition, destination, speed * Time.deltaTime);

                    if (!NavMesh.SamplePosition(nextPosition, out NavMeshHit hit, NavMeshSampleRadius, agent.areaMask))
                    {
                        break;
                    }

                    Vector3 displacement = hit.position - currentPosition;
                    displacement.y = 0f;

                    context.Movement.ApplyDisplacement(displacement);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                context.Animator.applyRootMotion = previousRootMotion;
            }
            else
            {
                yield return new WaitForSeconds(dashDuration);
            }

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

        private bool TryGetAiDashDestination(Vector3 origin, Vector3 direction, int areaMask, out Vector3 destination)
        {
            destination = origin;

            if (!NavMesh.SamplePosition(origin, out NavMeshHit startHit, NavMeshSampleRadius, areaMask))
            {
                return false;
            }

            Vector3 requestedDestination = origin + direction * dashSpeed * dashDuration;
            if (!NavMesh.SamplePosition(requestedDestination, out NavMeshHit endHit, NavMeshSampleRadius, areaMask))
            {
                return false;
            }

            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(startHit.position, endHit.position, areaMask, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            destination = endHit.position;
            return true;
        }
    }
}
