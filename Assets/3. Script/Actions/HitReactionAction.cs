using System.Collections;
using Actions.Core;
using Character;
using Character.Core;
using Combat;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "HitReactionAction", menuName = "Actions/Combat/Hit Reaction")]
    public class HitReactionAction : DurationAction
    {
        [SerializeField, Min(0f)] private float hitStunDuration = 0.9f;
        [SerializeField, Min(0f)] private float crossFadeDuration = 0.05f;

        public override bool CanExecute(ActionContext context)
        {
            return context != null && context.IsValid();
        }

        public override bool ShouldUseStamina()
        {
            return false;
        }

        public override void Execute(ActionContext context)
        {
            SetHitStunTags(context, true);
            PlayHitReaction(context);
        }

        public override IEnumerator ExecuteOverTime(ActionContext context, ActiveAction activeAction)
        {
            float duration = hitStunDuration;

            if (activeAction.Data is HitInfo hitInfo && hitInfo.hitReactionProfile != null)
            {
                HitReactionProfile profile = hitInfo.hitReactionProfile;
                duration = profile.HitStunDuration;

                context.Movement?.ApplyGroundPush(
                    hitInfo.attackDirection,
                    profile.PushDistance,
                    profile.PushDuration);
            }

            yield return new WaitForSeconds(duration);
        }

        public override void OnCompleted(ActionContext context, ActiveAction activeAction)
        {
            context.Movement?.CancelGroundPush();
            SetHitStunTags(context, false);
            RestoreLocomotion(context);
        }

        public override void OnCancelled(ActionContext context, ActiveAction activeAction)
        {
            context.Movement?.CancelGroundPush();
            SetHitStunTags(context, false);
            RestoreLocomotion(context);
        }

        private void PlayHitReaction(ActionContext context)
        {
            if (context?.Animator == null)
            {
                return;
            }

            context.Animator.CrossFadeInFixedTime(AnimationStates.HitReact, crossFadeDuration);
        }

        private void RestoreLocomotion(ActionContext context)
        {
            if (context?.Animator == null)
            {
                return;
            }

            if (context.Animator.isActiveAndEnabled)
            {
                context.Animator.CrossFadeInFixedTime(AnimationStates.IdleRun, crossFadeDuration);
            }
        }

        private static void SetHitStunTags(ActionContext context, bool enabled)
        {
            if (context?.ActionController == null)
            {
                return;
            }

            if (enabled)
            {
                context.ActionController.AddTag(ActionTags.Stunned);
                context.ActionController.AddTag(ActionTags.ExecutingAction);
                return;
            }

            context.ActionController.RemoveTag(ActionTags.Stunned);
            context.ActionController.RemoveTag(ActionTags.ExecutingAction);
        }
    }
}
