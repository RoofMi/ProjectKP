using UnityEngine;
using Character;

namespace ProjectKP.Actions
{
    public abstract class Action : ScriptableObject
    {
        [Header("Basic Info")]
        public string actionName;
        public Sprite icon;
        [TextArea(3, 5)]
        public string description;
        
        [Header("Execution")]
        public float cooldown = 0f;
        [Range(0, 100)]
        public int priority = 10;
        
        [Header("Requirements")]
        public float staminaCost = 0f;
        public string[] requiredTags;
        public string[] blockingTags;
        
        // Core methods
        public abstract bool CanExecute(GameObject owner);
        public abstract void Execute(GameObject owner);
        
        // Optional override
        public virtual bool CanBeCancelledBy(Action other)
        {
            return other.priority > this.priority;
        }
        
        // Helper method for tag checking
        protected bool CheckTags(GameObject owner)
        {
            var controller = owner.GetComponent<ActionController>();
            if (controller == null) return false;
            
            // Check required tags
            if (requiredTags != null)
            {
                foreach (var tag in requiredTags)
                {
                    if (!controller.HasTag(tag))
                        return false;
                }
            }
            
            // Check blocking tags
            if (blockingTags != null)
            {
                foreach (var tag in blockingTags)
                {
                    if (controller.HasTag(tag))
                        return false;
                }
            }
            
            return true;
        }
        
        // Helper method for stamina check
        protected bool CheckStamina(GameObject owner)
        {
            if (staminaCost <= 0) return true;
            
            var stamina = owner.GetComponent<StaminaComponent>();
            return stamina != null && stamina.CurrentStamina >= staminaCost;
        }
    }
}