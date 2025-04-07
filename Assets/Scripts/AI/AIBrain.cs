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
        private CharacterStateMachine _stateMachine;
            
        // for test
        private Health _health;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            
            _context = new AIContext(this, _playerCharacter);
            _stateMachine = new CharacterStateMachine(_context, _animator, true);
            
            // for test
            _health = GetComponent<Health>();

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
            
            _stateMachine.OnUpdate();
        }

        private void UpdateContext()
        {
            _context.SetData("health", _health.NormalizedHealth);
        }
    }
}
