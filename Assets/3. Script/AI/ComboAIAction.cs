using System.Collections.Generic;
using Combat;
using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/ComboAction", fileName = "ComboAIAction")]
    public class ComboAIAction : AIAction
    {
        [Header("Combo")]
        [SerializeField] private float reactionTime = 0.2f;

        [Range(-1f, 1f)]
        [SerializeField] private float minFacingDot = 0.65f;

        [SerializeField] private bool requiresTargetOpportunity = true;

        private void Reset()
        {
            priority = 30;
        }

        public override void Execute(AIContext context)
        {
            ComboManager comboManager = context.ComboManager;
            if (comboManager == null ||
                Time.time - context.GetFloat(ContextKeys.LastComboInputTime) < reactionTime)
            {
                return;
            }

            bool isAttacking =
                context.ActionController.HasTag(Character.Core.ActionTags.Attacking);

            if (!isAttacking)
            {
                StartCombo(context, comboManager);
            }
            else if (comboManager.IsInComboWindow)
            {
                ContinueCombo(context, comboManager);
            }
        }

        private void StartCombo(AIContext context, ComboManager comboManager)
        {
            if (!CanStartCombo(context))
            {
                return;
            }

            RuntimeComboNode rootNode = comboManager.RootNode;
            if (rootNode == null || rootNode.Children.Count == 0)
            {
                return;
            }

            ExecuteRandomChild(context, comboManager, rootNode);
        }

        private static void ContinueCombo(AIContext context, ComboManager comboManager)
        {
            RuntimeComboNode currentNode = comboManager.CurrentNode;
            if (currentNode == null || currentNode.Children.Count == 0)
            {
                return;
            }

            ExecuteRandomChild(context, comboManager, currentNode);
        }

        private static void ExecuteRandomChild(
            AIContext context,
            ComboManager comboManager,
            RuntimeComboNode parent)
        {
            var keys = new List<string>(parent.Children.Keys);
            string key = keys[Random.Range(0, keys.Count)];

            if (comboManager.TryExecuteCombo(key))
            {
                context.SetData(ContextKeys.LastComboInputTime, Time.time);
            }
        }

        private bool CanStartCombo(AIContext context)
        {
            if (context.GetFloat(ContextKeys.InMeleeRange) < 0.5f ||
                (requiresTargetOpportunity &&
                 context.GetFloat(ContextKeys.TargetOpportunity) < 0.5f) ||
                context.CurrentTarget == null)
            {
                return false;
            }

            Vector3 toTarget =
                context.CurrentTarget.position - context.Brain.transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                return true;
            }

            Vector3 forward = context.Brain.transform.forward;
            forward.y = 0f;

            if (Vector3.Dot(forward.normalized, toTarget.normalized) < minFacingDot)
            {
                context.Movement.SetRotationToDirection(toTarget);
            }

            return true;
        }
    }
}
