using System;
using System.Collections.Generic;
using Character;
using Character.Core;
using Combat;
using Combat.Events;
using Test;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    [RequireComponent(typeof(NavMeshAgent))]
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
        
        // Time tracking for context updates
        private float _combatStartTime;
        private float _lastHitTime;
        private float _lastActionTime;
        
        // Hit confirmation tracking
        private bool _hitConfirmed;
        private const float HIT_CONFIRM_DURATION = 0.3f; // 히트 확인 유지 시간

        private bool _wasTargetAttacking;
        private float _timeInMeleeRange;
        private float _lastTargetAttackEndTime = float.NegativeInfinity;
        private float _lastDashTime = float.NegativeInfinity;
        private float _nextActionChangeTime;

        private void Awake()
        {
            _context = new AIContext(this, _playerCharacter);
            _combatStartTime = Time.time;
            _lastHitTime = float.MinValue; // 초기값 설정

            foreach (var action in _actions)
            {
                action.Init(_context);
            }
            
            // Combat event 구독
            if (_combatEventChannel != null)
            {
                _combatEventChannel.Subscribe(OnHitDetected);
            }
        }
        
        private void OnDestroy()
        {
            // Combat event 구독 해제
            if (_combatEventChannel != null)
            {
                _combatEventChannel.Unsubscribe(OnHitDetected);
            }
        }
        
        // 히트 이벤트 처리
        private void OnHitDetected(HitInfo hitInfo)
        {
            // AI가 공격자인 경우 히트 확인
            if (hitInfo.attacker == gameObject)
            {
                _lastHitTime = Time.time;
                _hitConfirmed = true;
                UnityEngine.Debug.Log($"[AI] Hit confirmed on {hitInfo.target.name}");
            }
        }
        
        // 액션 실행 시 호출 (외부에서 호출 가능)
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
            
            // Check if currently executing a blocking action (like dash)
            bool isExecutingAction = _context.ActionController != null && 
                                   _context.ActionController.HasTag(ActionTags.ExecutingAction);
            
            // If executing a blocking action (e.g., dash), skip decision making
            if (isExecutingAction)
            {
                // Don't execute any actions during blocking actions like dash
                // The action will complete on its own
                return; // Skip decision making and execution entirely
            }
            
            // Normal execution for non-blocking actions
            
            // Evaluate new action periodically
            if (Time.time >= _nextDecisionTime)
            {
                AIAction bestAction = SelectBestAction();
                
                if (bestAction != _currentAction &&
                    (_currentAction == null || Time.time >= _nextActionChangeTime))
                {
                    RecordDecisionChange(bestAction); // 디버깅용 히스토리 기록
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

            foreach (var action in _actions)
            {
                float utility = action.CalculateUtility(_context);
                if (utility > highestUtility)
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
        
        // 디버그용 필드들
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
        
        // 의사결정 변경 기록 (디버깅용)
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
        
        // 모든 액션과 Utility 점수 반환
        public List<ActionUtilityInfo> GetAllActionsWithUtility()
        {
            var result = new List<ActionUtilityInfo>();
            foreach (var action in _actions)
            {
                if (action != null)
                {
                    float utility = action.CalculateUtility(_context);
                    bool isCurrent = action == _currentAction;
                    result.Add(new ActionUtilityInfo(action.name, utility, isCurrent));
                }
            }
            return result;
        }
        
        // 현재 실행 중인 액션
        public string GetCurrentActionName()
        {
            return _currentAction != null ? _currentAction.name : "None";
        }
        
        // Context 주요 값들
        public Dictionary<string, object> GetContextDebugInfo()
        {
            var debugInfo = new Dictionary<string, object>();
            
            // Basic Status
            debugInfo["Health"] = _context.GetData<float>(ContextKeys.Health);
            debugInfo["Stamina"] = _context.GetData<float>(ContextKeys.Stamina);
            debugInfo["IsAttacking"] = _context.GetData<float>(ContextKeys.IsAttacking) > 0.5f;
            debugInfo["IsAirborne"] = _context.GetData<float>(ContextKeys.IsAirborne) > 0.5f;
            debugInfo["IsStunned"] = _context.GetData<float>(ContextKeys.IsStunned) > 0.5f;
            
            // Distance & Position
            debugInfo["DistanceToTarget"] = _context.GetData<float>(ContextKeys.DistanceToTarget);
            debugInfo["InMeleeRange"] = _context.GetData<float>(ContextKeys.InMeleeRange) > 0.5f;
            debugInfo["TimeInMeleeRange"] = _context.GetData<float>(ContextKeys.TimeInMeleeRange);
            debugInfo["TimeSinceLastDash"] = _context.GetData<float>(ContextKeys.TimeSinceLastDash);
            
            // Combo Info
            debugInfo["InCombo"] = _context.GetData<float>(ContextKeys.InCombo) > 0.5f;
            debugInfo["ComboDepth"] = _context.GetData<int>(ContextKeys.ComboDepth);
            debugInfo["ComboWindowActive"] = _context.GetData<float>(ContextKeys.ComboWindowActive) > 0.5f;
            debugInfo["HitConfirm"] = _context.GetData<float>(ContextKeys.HitConfirm) > 0.5f;
            
            // Target Info
            debugInfo["TargetHealth"] = _context.GetData<float>(ContextKeys.TargetHealth);
            debugInfo["TargetAttacking"] = _context.GetData<float>(ContextKeys.TargetAttacking) > 0.5f;
            debugInfo["TargetRecovery"] = _context.GetData<float>(ContextKeys.TargetRecovery) > 0.5f;
            debugInfo["TargetOpportunity"] = _context.GetData<float>(ContextKeys.TargetOpportunity) > 0.5f;
            
            return debugInfo;
        }
        
        // 의사결정 히스토리
        public List<DecisionRecord> GetDecisionHistory()
        {
            return new List<DecisionRecord>(_decisionHistory);
        }
        
        #endregion
    }
}
