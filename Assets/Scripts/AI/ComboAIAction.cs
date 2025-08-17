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
        
        private float _lastInputTime;
        
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
            
            if (Time.time - _lastInputTime < reactionTime)
                return;
            
            var comboManager = context.ComboManager;
            bool canStartNewCombo = !context.ActionController.HasTag(Character.Core.ActionTags.Attacking);
            bool canContinueCombo = context.ActionController.HasTag(Character.Core.ActionTags.Attacking) && 
                                   comboManager.IsInComboWindow;
            
            // Start new combo
            if (canStartNewCombo)
            {
                string[] startKeys = { "LightAttack", "HeavyAttack" };
                string key = startKeys[Random.Range(0, startKeys.Length)];
                
                if (comboManager.TryExecuteCombo(key))
                {
                    _lastInputTime = Time.time;
                    Debug.Log($"[AI] Started combo with {key}");
                }
            }
            // Continue combo
            else if (canContinueCombo)
            {
                var currentNode = comboManager.CurrentNode;
                if (currentNode != null && currentNode.Children.Count > 0)
                {
                    // Get available keys for next combo step
                    var keys = new List<string>(currentNode.Children.Keys);
                    string key = keys[Random.Range(0, keys.Count)];
                    
                    if (comboManager.TryExecuteCombo(key))
                    {
                        _lastInputTime = Time.time;
                        Debug.Log($"[AI] Continued combo with {key}");
                    }
                }
            }
        }
    }
}