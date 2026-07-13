using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [CreateAssetMenu(menuName = "UtilityAI/Actions/ComboAction", fileName = "ComboAIAction")]
    public class ComboAIAction : AIAction
    {
        [Header("Combo Settings")]
        [Tooltip("Delay before AI decides on next combo move (simulates reaction time)")]
        [SerializeField] private float reactionTime = 0.2f;

        [Range(-1f, 1f)]
        [SerializeField] private float minFacingDot = 0.65f;

        [SerializeField] private bool requiresTargetOpportunity = true;
        
        private void Reset()
        {
            priority = 30;
        }

        public override void Init(AIContext context)
        {
            if (context.ComboManager == null)
            {
                Debug.LogError("ComboAIAction: ComboManager not found!");
            }
        }

        public override void Execute(AIContext context)
        {
            if (context.ComboManager == null)
            {
                Debug.LogWarning("ComboAIAction: ComboManager is null!");
                return;
            }

            float lastComboTime = context.GetData<float>("lastComboInputTime");
            if (Time.time - lastComboTime < reactionTime)
                return;

            var comboManager = context.ComboManager;
            bool canStartNewCombo = !context.ActionController.HasTag(Character.Core.ActionTags.Attacking);
            bool canContinueCombo = context.ActionController.HasTag(Character.Core.ActionTags.Attacking) &&
                                   comboManager.IsInComboWindow;

            // Start new combo
            if (canStartNewCombo)
            {
                if (!CanStartCombo(context))
                {
                    return;
                }

                var rootNode = comboManager.RootNode;
                if (rootNode == null || rootNode.Children.Count == 0)
                {
                    return;
                }

                var startKeys = new List<string>(rootNode.Children.Keys);
                string key = startKeys[Random.Range(0, startKeys.Count)];

                if (comboManager.TryExecuteCombo(key))
                {
                    context.SetData("lastComboInputTime", Time.time);
                    Debug.Log($"[AI] Started combo with {key}");
                }
                else
                {
                    Debug.Log($"[ComboDebug] Failed to start combo with {key}");
                }
            }
            // Continue combo
            else if (canContinueCombo)
            {
                Debug.Log($"[ComboDebug] Attempting to continue combo - window: {comboManager.IsInComboWindow}");
                var currentNode = comboManager.CurrentNode;
                if (currentNode != null && currentNode.Children.Count > 0)
                {
                    var keys = new List<string>(currentNode.Children.Keys);
                    string key = keys[Random.Range(0, keys.Count)];

                    if (comboManager.TryExecuteCombo(key))
                    {
                        context.SetData("lastComboInputTime", Time.time);
                        Debug.Log($"[AI] Continued combo with {key}");
                    }
                    else
                    {
                        Debug.Log($"[ComboDebug] Failed to continue combo with {key}");
                    }
                }
                else
                {
                    Debug.Log($"[ComboDebug] No available combo continuations");
                }
            }
        }

        private bool CanStartCombo(AIContext context)
        {
            if (context.GetData<float>(ContextKeys.InMeleeRange) < 0.5f)
            {
                return false;
            }

            if (requiresTargetOpportunity && context.GetData<float>(ContextKeys.TargetOpportunity) < 0.5f)
            {
                return false;
            }

            if (context.CurrentTarget == null)
            {
                return false;
            }

            Vector3 toTarget = context.CurrentTarget.position - context.Brain.transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            Vector3 forward = context.Brain.transform.forward;
            forward.y = 0f;
            if (Vector3.Dot(forward.normalized, toTarget.normalized) < minFacingDot)
            {
                context.Movement?.SetRotationToDirection(toTarget);
            }

            return true;
        }
    }
}
