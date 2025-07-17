using Character;
using UnityEngine;

namespace Actions.Core
{
    public abstract class ActionBase : ScriptableObject
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
        
        public abstract bool CanExecute(GameObject owner);
        public abstract void Execute(GameObject owner);
        
        public virtual bool CanBeCancelledBy(ActionBase other)
        {
            return other.priority > this.priority;
        }
        
        protected bool CheckTags(GameObject owner)
        {
            var controller = owner.GetComponent<ActionController>();
            if (controller == null) return false;
            
            if (requiredTags != null)
            {
                foreach (var tag in requiredTags)
                {
                    if (!controller.HasTag(tag))
                        return false;
                }
            }
            
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
        
        protected bool CheckStamina(GameObject owner)
        {
            if (staminaCost <= 0) return true;
            
            var stamina = owner.GetComponent<StaminaComponent>();
            return stamina != null && stamina.CurrentStamina >= staminaCost;
        }
    }
}