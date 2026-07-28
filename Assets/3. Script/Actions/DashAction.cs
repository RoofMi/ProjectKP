using System.Collections;
using Actions.Core;
using Character;
using Character.Core;
using UnityEngine;
using AI;

namespace Actions
{
    [CreateAssetMenu(fileName = "DashAction", menuName = "Actions/Movement/Dash")]
    public class DashAction : DurationAction
    {
        [Header("Dash Settings")]
        public float dashSpeed = 15f;
        public float dashDuration = 0.5f;

        private readonly System.Collections.Generic.Dictionary<ActiveAction, DashState> _activeDashStates = new();

        private void Reset()
        {
            actionName = "Dash";
            description = "Quick dash in movement direction";
            cooldown = 0.5f;
            priority = 30;
            staminaCost = 20f;
            requiredTags = null;
            blockingTags = new[] { ActionTags.Stunned, ActionTags.Dashing };
            dashDuration = 0.5f;
        }

        public override bool CanExecute(ActionContext context)
        {
            if (!base.CanExecute(context))
            {
                return false;
            }

            if (context.ActionController.IsActionActive(typeof(DashAction)) ||
                context.ActionController.HasTag(ActionTags.Stunned))
            {
                return false;
            }

            return !context.ActionController.HasTag(ActionTags.Airborne) ||
                !context.ActionController.HasTag(ActionTags.AirDashUsed);
        }

        public override IEnumerator ExecuteOverTime(ActionContext context, ActiveAction activeAction)
        {
            bool isAirDash = context.ActionController.HasTag(ActionTags.Airborne);
            _activeDashStates[activeAction] = new DashState(isAirDash, context.Animator.applyRootMotion);

            if (isAirDash)
            {
                context.ActionController.AddTag(ActionTags.AirDashUsed);
                context.Movement.SetGravityEnabled(false);
            }

            context.ActionController.AddTag(ActionTags.Dashing);
            context.ActionController.AddTag(ActionTags.ExecutingAction);
            context.Animator.SetTrigger(AnimationHashes.DashStart);

            Vector2 dashDirection = activeAction.Data as Vector2? ?? Vector2.zero;
            if (dashDirection.sqrMagnitude < 0.01f)
            {
                Vector3 forward = context.Owner.transform.forward;
                dashDirection = new Vector2(forward.x, forward.z).normalized;
            }

            Vector3 worldDashDirection = new(dashDirection.x, 0f, dashDirection.y);
            if (context.Movement.CameraTransform != null)
            {
                Vector3 cameraForward = context.Movement.CameraTransform.forward;
                Vector3 cameraRight = context.Movement.CameraTransform.right;
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();
                worldDashDirection =
                    (cameraForward * dashDirection.y + cameraRight * dashDirection.x).normalized;
            }
            else
            {
                worldDashDirection.Normalize();
            }

            AINavigation navigation = context.Owner.GetComponent<AINavigation>();
            if (navigation == null)
            {
                yield return new WaitForSeconds(dashDuration);
                yield break;
            }

            if (dashDuration <= Mathf.Epsilon ||
                !navigation.TryGetReachableDestination(
                    context.Owner.transform.position,
                    worldDashDirection,
                    dashSpeed * dashDuration,
                    out Vector3 destination))
            {
                yield break;
            }

            context.Animator.applyRootMotion = false;
            context.Movement.SetRotationToDirection(worldDashDirection);

            float elapsed = 0f;
            float speed = Vector3.Distance(context.Owner.transform.position, destination) / dashDuration;
            while (elapsed < dashDuration)
            {
                Vector3 currentPosition = context.Owner.transform.position;
                Vector3 nextPosition =
                    Vector3.MoveTowards(currentPosition, destination, speed * Time.deltaTime);

                if (!navigation.TryProjectPosition(nextPosition, out Vector3 projectedPosition))
                {
                    break;
                }

                Vector3 displacement = projectedPosition - currentPosition;
                displacement.y = 0f;
                context.Movement.ApplyDisplacement(displacement);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        public override void OnCompleted(ActionContext context, ActiveAction activeAction)
        {
            ResetDashState(context, activeAction);
        }

        public override void OnCancelled(ActionContext context, ActiveAction activeAction)
        {
            ResetDashState(context, activeAction);
        }

        private void ResetDashState(ActionContext context, ActiveAction activeAction)
        {
            context.ActionController.RemoveTag(ActionTags.Dashing);
            context.ActionController.RemoveTag(ActionTags.ExecutingAction);
            context.Animator.SetTrigger(AnimationHashes.DashEnd);

            if (!_activeDashStates.TryGetValue(activeAction, out DashState state))
            {
                return;
            }

            _activeDashStates.Remove(activeAction);
            context.Animator.applyRootMotion = state.PreviousRootMotion;

            if (state.IsAirDash)
            {
                context.Movement.SetGravityEnabled(true);
                context.Movement.ResetVerticalVelocity();
            }
        }

        private readonly struct DashState
        {
            public bool IsAirDash { get; }
            public bool PreviousRootMotion { get; }

            public DashState(bool isAirDash, bool previousRootMotion)
            {
                IsAirDash = isAirDash;
                PreviousRootMotion = previousRootMotion;
            }
        }
    }
}
