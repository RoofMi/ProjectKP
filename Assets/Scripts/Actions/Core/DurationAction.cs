using System.Collections;
using UnityEngine;

namespace Actions.Core
{
    public abstract class DurationAction : ActionBase
    {
        public override void Execute(GameObject owner)
        {
            // ActionController will handle coroutine execution
        }
        
        public abstract IEnumerator ExecuteOverTime(GameObject owner, ActiveAction activeAction);
        
        public override bool CanExecute(GameObject owner)
        {
            return CheckTags(owner) && CheckStamina(owner);
        }
    }
}