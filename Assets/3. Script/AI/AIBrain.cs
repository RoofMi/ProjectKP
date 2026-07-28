using System;
using System.Collections.Generic;
using Character;
using Character.Core;
using Combat;
using Combat.Events;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(AINavigation))]
    public class AIBrain : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _playerCharacter;
        
        [Header("Actions")]
        [SerializeField] private List<AIAction> _actions;
        
        [Header("Decision Making")]
        [Tooltip("How often AI evaluates which action to take (in seconds)")]
        [SerializeField] private float decisionInterval = 0.2f;
        
        [Header("Combat Events")]
        [SerializeField] private CombatEventChannel _combatEventChannel;

        [Header("Tactical Decision")]
        [SerializeField, Min(0f)] private float actionCommitmentDuration = 0.25f;
        [SerializeField, Min(0f)] private float targetRecoveryDuration = 0.65f;
        [SerializeField, Min(0f)] private float closeRangePatience = 0.35f;
        
        private AIContext _context;
        private AIAction _currentAction;
        private float _nextDecisionTime;
        
        private float _combatStartTime;
        private float _lastHitTime;
        private float _lastActionTime;
        
        private bool _hitConfirmed;
        private const float HIT_CONFIRM_DURATION = 0.3f;

        private bool _wasTargetAttacking;
        private float _timeInMeleeRange;
        private float _lastTargetAttackEndTime = float.NegativeInfinity;
        private float _lastDashTime = float.NegativeInfinity;
        private float _nextActionChangeTime;

        private void Awake()
        {
            _context = new AIContext(this, _playerCharacter);
            if (!_context.IsValid)
            {
                Debug.LogError(
                    "[AIBrain] Target and required AI combat components must be configured.",
                    this);
                enabled = false;
                return;
            }

            _combatStartTime = Time.time;
            _lastHitTime = float.MinValue;

            if (_actions != null)
            {
                foreach (AIAction action in _actions)
                {
                    if (action == null)
                    {
                        Debug.LogWarning("[AIBrain] Null action entry ignored.", this);
                        continue;
                    }

                    action.Init(_context);
                }
            }

            if (_combatEventChannel != null)
            {
                _combatEventChannel.Subscribe(OnHitDetected);
            }
        }
        
        private void OnDestroy()
        {
            if (_combatEventChannel != null)
            {
                _combatEventChannel.Unsubscribe(OnHitDetected);
            }
        }
        
        private void OnHitDetected(HitInfo hitInfo)
        {
            if (hitInfo.attacker != gameObject)
            {
                return;
            }

            _lastHitTime = Time.time;
            _hitConfirmed = true;
        }
        
        public void OnActionExecuted()
        {
            _lastActionTime = Time.time;
        }

        public void NotifyDashExecuted()
        {
            _lastDashTime = Time.time;
        }

        private void Update()
        {
            UpdateContext();
            
            bool isExecutingAction = _context.ActionController != null && 
                                   _context.ActionController.HasTag(ActionTags.ExecutingAction);
            
            if (isExecutingAction)
            {
                return;
            }
            
            
            if (Time.time >= _nextDecisionTime)
            {
                AIAction bestAction = SelectBestAction();
                
                if (bestAction != _currentAction &&
                    (_currentAction == null || Time.time >= _nextActionChangeTime))
                {
                    RecordDecisionChange(bestAction);
                    _currentAction = bestAction;
                    _nextActionChangeTime = Time.time + actionCommitmentDuration;
                }
                
                _currentAction?.Execute(_context);
                _nextDecisionTime = Time.time + decisionInterval;
            }
        }
        
        private AIAction SelectBestAction()
        {
            AIAction bestAction = null;
            float highestUtility = float.MinValue;

            if (_actions == null)
            {
                return null;
            }

            foreach (AIAction action in _actions)
            {
                if (action == null)
                {
                    continue;
                }

                float utility = action.CalculateUtility(_context);
                if (bestAction == null ||
                    utility > highestUtility ||
                    (Mathf.Approximately(utility, highestUtility) && action.Priority > bestAction.Priority))
                {
                    highestUtility = utility;
                    bestAction = action;
                }
            }

            return bestAction;
        }

        private void UpdateContext()
        {
            if (_playerCharacter == null)
            {
                _timeInMeleeRange = 0f;
            }

            if (_context.Health != null)
            {
                _context.SetData(ContextKeys.Health, _context.Health.HealthPercentage);
            }
            if (_context.Stamina != null)
            {
                _context.SetData(ContextKeys.Stamina, _context.Stamina.StaminaPercentage);
            }
            
            if (_context.ActionController != null)
            {
                _context.SetData(ContextKeys.IsAttacking, 
                    _context.ActionController.HasTag(ActionTags.Attacking) ? 1.0f : 0.0f);
                _context.SetData(ContextKeys.IsAirborne, 
                    _context.ActionController.HasTag(ActionTags.Airborne) ? 1.0f : 0.0f);
                _context.SetData(ContextKeys.IsStunned, 
                    _context.ActionController.HasTag(ActionTags.Stunned) ? 1.0f : 0.0f);
            }
            
            if (_playerCharacter != null)
            {
                Vector3 toTarget = _playerCharacter.position - transform.position;
                float distance = toTarget.magnitude;
                
                _context.SetData(ContextKeys.DistanceToTarget, distance);
                _context.SetData(ContextKeys.DistanceNormalized, Mathf.Clamp01(distance / 20f));
                bool isInMeleeRange = distance <= 2f;
                _context.SetData(ContextKeys.InMeleeRange, isInMeleeRange ? 1.0f : 0.0f);
                if (isInMeleeRange)
                {
                    _timeInMeleeRange += Time.deltaTime;
                }
                else
                {
                    _timeInMeleeRange = 0f;
                }
            }
            
            if (_context.ComboManager != null)
            {
                bool isInCombo = _context.ComboManager.IsInCombo();
                _context.SetData(ContextKeys.InCombo, isInCombo ? 1.0f : 0.0f);
                
                int comboDepth = _context.ComboManager.GetComboDepth();
                _context.SetData(ContextKeys.ComboDepth, comboDepth);
                
                _context.SetData(ContextKeys.ComboWindowActive, _context.ComboManager.IsInComboWindow ? 1.0f : 0.0f);
                
                if (_hitConfirmed && Time.time - _lastHitTime > HIT_CONFIRM_DURATION)
                {
                    _hitConfirmed = false;
                }
                _context.SetData(ContextKeys.HitConfirm, _hitConfirmed ? 1.0f : 0.0f);
                
                float timeSinceLastHit = _lastHitTime > 0 ? Time.time - _lastHitTime : float.MaxValue;
                _context.SetData(ContextKeys.LastHitTime, timeSinceLastHit);
            }
            else
            {
                _context.SetData(ContextKeys.InCombo, 0.0f);
                _context.SetData(ContextKeys.ComboDepth, 0);
                _context.SetData(ContextKeys.ComboWindowActive, 0.0f);
                _context.SetData(ContextKeys.HitConfirm, 0.0f);
                _context.SetData(ContextKeys.LastHitTime, float.MaxValue);
            }
            
            if (_context.TargetHealth != null)
            {
                _context.SetData(ContextKeys.TargetHealth, _context.TargetHealth.HealthPercentage);
            }
            else
            {
                _context.SetData(ContextKeys.TargetHealth, 0f);
            }
            
            bool hasTargetStamina = _context.TargetStamina != null;
            float targetStamina = hasTargetStamina ? _context.TargetStamina.StaminaPercentage : 0f;
            _context.SetData(ContextKeys.TargetStamina, targetStamina);

            bool targetAttacking = false;
            if (_context.TargetActionController != null)
            {
                targetAttacking = _context.TargetActionController.HasTag(ActionTags.Attacking);
                _context.SetData(ContextKeys.TargetAttacking, targetAttacking ? 1.0f : 0.0f);
                _context.SetData(ContextKeys.TargetAirborne,
                    _context.TargetActionController.HasTag(ActionTags.Airborne) ? 1.0f : 0.0f);
                _context.SetData(ContextKeys.TargetInHitstun,
                    _context.TargetActionController.HasTag(ActionTags.Stunned) ? 1.0f : 0.0f);
            }
            else
            {
                _context.SetData(ContextKeys.TargetAttacking, 0.0f);
                _context.SetData(ContextKeys.TargetAirborne, 0.0f);
                _context.SetData(ContextKeys.TargetInHitstun, 0.0f);
            }

            if (_wasTargetAttacking && !targetAttacking)
            {
                _lastTargetAttackEndTime = Time.time;
            }

            _wasTargetAttacking = targetAttacking;
            bool targetRecovering = Time.time - _lastTargetAttackEndTime <= targetRecoveryDuration;
            bool targetExhausted = hasTargetStamina && targetStamina <= 0.25f;
            bool targetOpportunity = targetRecovering || targetExhausted || _timeInMeleeRange >= closeRangePatience;

            _context.SetData(ContextKeys.TargetRecovery, targetRecovering ? 1.0f : 0.0f);
            _context.SetData(ContextKeys.TimeInMeleeRange, _timeInMeleeRange);
            _context.SetData(ContextKeys.TargetOpportunity, targetOpportunity ? 1.0f : 0.0f);
            
            float combatDuration = Time.time - _combatStartTime;
            _context.SetData(ContextKeys.CombatDuration, combatDuration);
            
            float timeSinceLastAction = Time.time - _lastActionTime;
            _context.SetData(ContextKeys.TimeSinceLastAction, timeSinceLastAction);

            float timeSinceLastDash = Time.time - _lastDashTime;
            _context.SetData(ContextKeys.TimeSinceLastDash, timeSinceLastDash);
        }
        
        #region Debug Support
        
        private List<DecisionRecord> _decisionHistory = new List<DecisionRecord>();
        private const int MAX_HISTORY = 10;
        
        [Serializable]
        public class ActionUtilityInfo
        {
            public string actionName;
            public float utility;
            public bool isCurrentAction;
            
            public ActionUtilityInfo(string name, float util, bool current)
            {
                actionName = name;
                utility = util;
                isCurrentAction = current;
            }
        }
        
        [Serializable]
        public class DecisionRecord
        {
            public float timestamp;
            public string actionName;
            public float utility;
            
            public DecisionRecord(string action, float util)
            {
                timestamp = Time.time;
                actionName = action;
                utility = util;
            }
        }
        
        private void RecordDecisionChange(AIAction newAction)
        {
            if (newAction == null) return;
            
            float utility = newAction.CalculateUtility(_context);
            _decisionHistory.Add(new DecisionRecord(newAction.name, utility));
            
            if (_decisionHistory.Count > MAX_HISTORY)
            {
                _decisionHistory.RemoveAt(0);
            }
        }
        
        public List<ActionUtilityInfo> GetAllActionsWithUtility()
        {
            var result = new List<ActionUtilityInfo>();
            if (_actions == null)
            {
                return result;
            }

            foreach (AIAction action in _actions)
            {
                if (action == null)
                {
                    continue;
                }

                result.Add(new ActionUtilityInfo(
                    action.name,
                    action.CalculateUtility(_context),
                    action == _currentAction));
            }

            return result;
        }
        
        public string GetCurrentActionName()
        {
            return _currentAction != null ? _currentAction.name : "None";
        }
        
        public Dictionary<string, object> GetContextDebugInfo()
        {
            return new Dictionary<string, object>
            {
                ["Health"] = _context.GetFloat(ContextKeys.Health),
                ["Stamina"] = _context.GetFloat(ContextKeys.Stamina),
                ["IsAttacking"] = _context.GetFloat(ContextKeys.IsAttacking) > 0.5f,
                ["IsAirborne"] = _context.GetFloat(ContextKeys.IsAirborne) > 0.5f,
                ["IsStunned"] = _context.GetFloat(ContextKeys.IsStunned) > 0.5f,
                ["DistanceToTarget"] = _context.GetFloat(ContextKeys.DistanceToTarget),
                ["InMeleeRange"] = _context.GetFloat(ContextKeys.InMeleeRange) > 0.5f,
                ["TimeInMeleeRange"] = _context.GetFloat(ContextKeys.TimeInMeleeRange),
                ["TimeSinceLastDash"] = _context.GetFloat(ContextKeys.TimeSinceLastDash),
                ["InCombo"] = _context.GetFloat(ContextKeys.InCombo) > 0.5f,
                ["ComboDepth"] = _context.GetData<int>(ContextKeys.ComboDepth),
                ["ComboWindowActive"] = _context.GetFloat(ContextKeys.ComboWindowActive) > 0.5f,
                ["HitConfirm"] = _context.GetFloat(ContextKeys.HitConfirm) > 0.5f,
                ["TargetHealth"] = _context.GetFloat(ContextKeys.TargetHealth),
                ["TargetAttacking"] = _context.GetFloat(ContextKeys.TargetAttacking) > 0.5f,
                ["TargetRecovery"] = _context.GetFloat(ContextKeys.TargetRecovery) > 0.5f,
                ["TargetOpportunity"] = _context.GetFloat(ContextKeys.TargetOpportunity) > 0.5f
            };
        }
        
        public List<DecisionRecord> GetDecisionHistory()
        {
            return new List<DecisionRecord>(_decisionHistory);
        }
        
        #endregion
    }
}
