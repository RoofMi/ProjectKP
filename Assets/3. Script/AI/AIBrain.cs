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
        [SerializeField] private Transform _playerCharacter;
        [SerializeField] private List<AIAction> _actions;
        private AIContext _context;
        private Animator _animator;
        private HealthComponent _health;

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

            if (bestAction != null)
            {
                bestAction.Execute(_context);
            }
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
