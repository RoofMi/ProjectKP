using Combat;
using UnityEngine;

namespace Character.Core
{
    public class ActionContext
    {
        public readonly GameObject Owner;
        public readonly ActionController ActionController;
        public readonly CharacterMovement Movement;
        public readonly Animator Animator;
        public readonly StaminaComponent Stamina;
        public readonly ComboManager ComboManager;
        
        public ActionContext(GameObject owner)
        {
            Owner = owner;
            ActionController = owner.GetComponent<ActionController>();
            Movement = owner.GetComponent<CharacterMovement>();
            Animator = owner.GetComponent<Animator>();
            Stamina = owner.GetComponent<StaminaComponent>();
            ComboManager = owner.GetComponent<ComboManager>();
        }
        
        public bool IsValid()
        {
            return Owner != null && ActionController != null;
        }
        
        public bool HasAllComponents()
        {
            return IsValid() && 
                   Movement != null && 
                   Animator != null && 
                   Stamina != null && 
                   ComboManager != null;
        }
    }
}