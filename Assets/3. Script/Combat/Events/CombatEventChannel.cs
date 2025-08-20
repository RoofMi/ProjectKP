using System;
using UnityEngine;

namespace Combat.Events
{
    [CreateAssetMenu(fileName = "CombatEventChannel", menuName = "Events/Combat Event Channel")]
    public class CombatEventChannel : ScriptableObject
    {
        private event Action<HitInfo> OnHitDetected;
        
        public void RaiseHit(HitInfo hitInfo)
        {
            OnHitDetected?.Invoke(hitInfo);
        }
        
        public void Subscribe(Action<HitInfo> handler)
        {
            OnHitDetected += handler;
        }
        
        public void Unsubscribe(Action<HitInfo> handler)
        {
            OnHitDetected -= handler;
        }
        
        private void OnDisable()
        {
            OnHitDetected = null;
        }
    }
}