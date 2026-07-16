using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "HitReactionProfile", menuName = "Combat/Hit Reaction Profile")]
    public class HitReactionProfile : ScriptableObject
    {
        [SerializeField, Min(0f)] private float hitStunDuration = 0.9f;
        [SerializeField, Min(0f)] private float pushDistance;
        [SerializeField, Min(0f)] private float pushDuration;

        public float HitStunDuration => hitStunDuration;
        public float PushDistance => pushDistance;
        public float PushDuration => pushDuration;
    }
}
