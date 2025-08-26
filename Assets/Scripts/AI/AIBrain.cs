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
                Debug.Log($"[AI] Hit confirmed on {hitInfo.target.name}");
            }
        }
        
        // 액션 실행 시 호출 (외부에서 호출 가능)
        public void OnActionExecuted()
        {
            _lastActionTime = Time.time;
        }

        private void Update()
        {
            UpdateContext();
            
            _currentAction?.Execute(_context);
            
            // Evaluate new action periodically
            if (Time.time >= _nextDecisionTime)
            {
                AIAction bestAction = SelectBestAction();
                
                if (bestAction != _currentAction)
                {
                    _currentAction = bestAction;
                    Debug.Log($"[AI] Switched to action: {_currentAction?.name}");
                }
                
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
                    _context.ActionController.HasTag(ActionTags.Attacking));
                _context.SetData(ContextKeys.IsAirborne, 
                    _context.ActionController.HasTag(ActionTags.Airborne));
                _context.SetData(ContextKeys.IsStunned, 
                    _context.ActionController.HasTag(ActionTags.Stunned));
            }
            
            if (_playerCharacter != null)
            {
                Vector3 toTarget = _playerCharacter.position - transform.position;
                float distance = toTarget.magnitude;
                
                _context.SetData(ContextKeys.DistanceToTarget, distance);
                _context.SetData(ContextKeys.DistanceNormalized, Mathf.Clamp01(distance / 20f));
                _context.SetData(ContextKeys.InMeleeRange, distance <= 2f);
            }
            
            if (_context.ComboManager != null)
            {
                bool isInCombo = _context.ComboManager.IsInCombo();
                _context.SetData(ContextKeys.InCombo, isInCombo);
                
                int comboDepth = _context.ComboManager.GetComboDepth();
                _context.SetData(ContextKeys.ComboDepth, comboDepth);
                
                _context.SetData(ContextKeys.ComboWindowActive, _context.ComboManager.IsInComboWindow);
                
                if (_hitConfirmed && Time.time - _lastHitTime > HIT_CONFIRM_DURATION)
                {
                    _hitConfirmed = false;
                }
                _context.SetData(ContextKeys.HitConfirm, _hitConfirmed);
                
                float timeSinceLastHit = _lastHitTime > 0 ? Time.time - _lastHitTime : float.MaxValue;
                _context.SetData(ContextKeys.LastHitTime, timeSinceLastHit);
            }
            else
            {
                _context.SetData(ContextKeys.InCombo, false);
                _context.SetData(ContextKeys.ComboDepth, 0);
                _context.SetData(ContextKeys.ComboWindowActive, false);
                _context.SetData(ContextKeys.HitConfirm, false);
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
            
            if (_context.TargetStamina != null)
            {
                _context.SetData(ContextKeys.TargetStamina, _context.TargetStamina.StaminaPercentage);
            }
            else
            {
                _context.SetData(ContextKeys.TargetStamina, 0f);
            }
            
            if (_context.TargetActionController != null)
            {
                _context.SetData(ContextKeys.TargetAttacking, 
                    _context.TargetActionController.HasTag(ActionTags.Attacking));
                _context.SetData(ContextKeys.TargetAirborne, 
                    _context.TargetActionController.HasTag(ActionTags.Airborne));
                _context.SetData(ContextKeys.TargetInHitstun, 
                    _context.TargetActionController.HasTag(ActionTags.Stunned));
            }
            else
            {
                _context.SetData(ContextKeys.TargetAttacking, false);
                _context.SetData(ContextKeys.TargetAirborne, false);
                _context.SetData(ContextKeys.TargetInHitstun, false);
            }
            
            float combatDuration = Time.time - _combatStartTime;
            _context.SetData(ContextKeys.CombatDuration, combatDuration);
            
            float timeSinceLastAction = Time.time - _lastActionTime;
            _context.SetData(ContextKeys.TimeSinceLastAction, timeSinceLastAction);
        }
    }
}
