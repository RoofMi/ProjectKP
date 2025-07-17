using UnityEngine;

namespace Actions.Core
{
    public abstract class InstantAction : ActionBase
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