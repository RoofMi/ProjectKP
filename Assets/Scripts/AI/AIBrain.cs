using System;
using System.Collections.Generic;
using Character;
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
        
        private AIContext _context;
        private Animator _animator;
        private HealthComponent _health;
        private AIAction _currentAction;
        private float _nextDecisionTime;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _health = GetComponent<HealthComponent>();
            _context = new AIContext(this, _playerCharacter);

            foreach (var action in _actions)
            {
                action.Init(_context);
            }
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
            // health
            _context.SetData("health", _health.HealthPercentage);
            
            // distance
            float normalizedDistance = Mathf.Clamp01(Vector3.Distance(transform.position, _playerCharacter.position) / 20f);
            _context.SetData("distanceToTarget", normalizedDistance);
        }
    }
}
