using System.Collections;
using Character.Core;
using UnityEngine;

namespace Actions.Core
{
    public abstract class DurationAction : ActionBase
    {
        public override void Execute(ActionContext context)
        {
            // ActionController will handle coroutine execution
        }
        
        public abstract IEnumerator ExecuteOverTime(ActionContext context, ActiveAction activeAction);
        
        public override bool CanExecute(ActionContext context)
        {
            return CheckTags(context) && CheckStamina(context);
        }
    }
}