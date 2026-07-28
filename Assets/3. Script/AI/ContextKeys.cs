namespace AI
{
    public static class ContextKeys
    {
        public const string Health = "health";
        public const string Stamina = "stamina";
        public const string IsAttacking = "isAttacking";
        public const string IsAirborne = "isAirborne";
        public const string IsStunned = "isStunned";

        public const string DistanceToTarget = "distanceToTarget";
        public const string DistanceNormalized = "distanceNormalized";
        public const string InMeleeRange = "inMeleeRange";

        public const string InCombo = "inCombo";
        public const string ComboDepth = "comboDepth";
        public const string ComboWindowActive = "comboWindowActive";
        public const string HitConfirm = "hitConfirm";
        public const string LastHitTime = "lastHitTime";
        public const string LastComboInputTime = "lastComboInputTime";

        public const string TargetHealth = "targetHealth";
        public const string TargetStamina = "targetStamina";
        public const string TargetAttacking = "targetAttacking";
        public const string TargetAirborne = "targetAirborne";
        public const string TargetInHitstun = "targetInHitstun";
        public const string TargetRecovery = "targetRecovery";
        public const string TimeInMeleeRange = "timeInMeleeRange";
        public const string TargetOpportunity = "targetOpportunity";

        public const string CombatDuration = "combatDuration";
        public const string TimeSinceLastAction = "timeSinceLastAction";
        public const string TimeSinceLastDash = "timeSinceLastDash";
    }
}
