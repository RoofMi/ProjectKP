namespace AI
{
    public static class ContextKeys
    {
        // === Basic Status (기본 상태) - 5개 ===
        public const string Health = "health";                    // float: 0.0 ~ 1.0
        public const string Stamina = "stamina";                  // float: 0.0 ~ 1.0
        public const string IsAttacking = "isAttacking";          // bool
        public const string IsAirborne = "isAirborne";            // bool
        public const string IsStunned = "isStunned";              // bool
        
        // === Distance & Position (거리 및 위치) - 3개 ===
        public const string DistanceToTarget = "distanceToTarget";        // float: meters
        public const string DistanceNormalized = "distanceNormalized";    // float: 0.0 ~ 1.0 (based on max range)
        public const string InMeleeRange = "inMeleeRange";                // bool
        
        // === Combo Information (콤보 정보) - 5개 ===
        public const string InCombo = "inCombo";                  // bool
        public const string ComboDepth = "comboDepth";            // int: 0, 1, 2, 3...
        public const string ComboWindowActive = "comboWindowActive"; // bool
        public const string HitConfirm = "hitConfirm";            // bool: last attack hit
        public const string LastHitTime = "lastHitTime";          // float: time since last hit
        
        // === Target Information (타겟 정보) - 5개 ===
        public const string TargetHealth = "targetHealth";        // float: 0.0 ~ 1.0
        public const string TargetStamina = "targetStamina";      // float: 0.0 ~ 1.0
        public const string TargetAttacking = "targetAttacking";  // bool
        public const string TargetAirborne = "targetAirborne";    // bool
        public const string TargetInHitstun = "targetInHitstun";  // bool: target is stunned/hit
        
        // === Time Information (시간 정보) - 2개 ===
        public const string CombatDuration = "combatDuration";    // float: seconds since combat start
        public const string TimeSinceLastAction = "timeSinceLastAction"; // float: seconds since last action
    }
}