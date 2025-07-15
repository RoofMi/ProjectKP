using UnityEngine;

namespace ProjectKP.Actions
{
    public abstract class InstantAction : Action
    {
        public override void Execute(GameObject owner)
        {
            OnExecute(owner);
        }
        
        protected abstract void OnExecute(GameObject owner);
        
        public override bool CanExecute(GameObject owner)
        {
            return CheckTags(owner) && CheckStamina(owner);
        }
    }
}