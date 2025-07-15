using UnityEngine;
using System.Collections;

namespace ProjectKP.Actions
{
    public abstract class DurationAction : Action
    {
        public override void Execute(GameObject owner)
        {
            // ActionController will handle coroutine execution
        }
        
        public abstract IEnumerator ExecuteOverTime(GameObject owner, object data = null);
        
        public override bool CanExecute(GameObject owner)
        {
            return CheckTags(owner) && CheckStamina(owner);
        }
    }
}