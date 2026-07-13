using Character;
using Character.Core;
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
        
        public abstract bool CanExecute(ActionContext context);
        public abstract void Execute(ActionContext context);
        
        public virtual bool CanBeCancelledBy(ActionBase other)
        {
            return other.priority > this.priority;
        }
        
        public virtual bool ShouldUseStamina()
        {
            return true;
        }

        public virtual void OnCompleted(ActionContext context, ActiveAction activeAction)
        {
        }

        public virtual void OnCancelled(ActionContext context, ActiveAction activeAction)
        {
        }
        
        protected bool CheckTags(ActionContext context)
        {
            if (context == null || !context.IsValid()) return false;
            
            if (requiredTags != null)
            {
                foreach (var tag in requiredTags)
                {
                    if (!context.ActionController.HasTag(tag))
                        return false;
                }
            }
            
            if (blockingTags != null)
            {
                foreach (var tag in blockingTags)
                {
                    if (context.ActionController.HasTag(tag))
                        return false;
                }
            }
            
            return true;
        }
        
        protected bool CheckStamina(ActionContext context)
        {
            if (staminaCost <= 0) return true;
            
            return context.Stamina != null && context.Stamina.CurrentStamina >= staminaCost;
        }
    }
}
