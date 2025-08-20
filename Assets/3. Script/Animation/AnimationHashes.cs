using UnityEngine;

namespace Character
{
    public static class AnimationHashes
    {
        // Movement
        public static readonly int Speed = Animator.StringToHash("speed");
        public static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        
        // Actions
        public static readonly int Jump = Animator.StringToHash("jump");
        public static readonly int DashStart = Animator.StringToHash("dashStart");
        public static readonly int DashEnd = Animator.StringToHash("dashEnd");
        
        // Combat
        public static readonly int Attack = Animator.StringToHash("attack");
        public static readonly int ComboCount = Animator.StringToHash("comboCount");
    }
}