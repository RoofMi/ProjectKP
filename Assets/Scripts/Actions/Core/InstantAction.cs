using Character.Core;
using UnityEngine;

namespace Actions.Core
{
    public abstract class InstantAction : ActionBase
    {
        public override void Execute(ActionContext context)
        {
            OnExecute(context);
        }
        
        protected abstract void OnExecute(ActionContext context);
        
        public override bool CanExecute(ActionContext context)
        {
            return CheckTags(context) && CheckStamina(context);
        }
    }
}