using System.Collections.Generic;
using Character;
using Combat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    public class AIContext
    {
        private Transform _playerCharacter;
        private Transform _currentTarget;
        private AIBrain _brain;
        
        // AI's own components
        private NavMeshAgent _agent;
        private ActionController _actionController;
        private ComboManager _comboManager;
        private Animator _animator;
        private HealthComponent _health;
        private StaminaComponent _stamina;
        private CharacterMovement _movement;
        
        // Target's components
        private HealthComponent _targetHealth;
        private StaminaComponent _targetStamina;
        private ActionController _targetActionController;
        private CharacterMovement _targetMovement;

        private readonly Dictionary<string, object> _data = new();

        public AIContext(AIBrain brain, Transform playerCharacter)
        {
            if (brain is null || playerCharacter is null)
            {
                return;
            }

            _brain = brain;
            _playerCharacter = playerCharacter;
            _currentTarget = playerCharacter;
            
            _agent = brain.GetOrAddComponent<NavMeshAgent>();
            _actionController = brain.GetOrAddComponent<ActionController>();
            _comboManager = brain.GetOrAddComponent<ComboManager>();
            _animator = brain.GetComponent<Animator>();
            _health = brain.GetComponent<HealthComponent>();
            _stamina = brain.GetComponent<StaminaComponent>();
            _movement = brain.GetComponent<CharacterMovement>();
            
            if (_playerCharacter != null)
            {
                _targetHealth = _playerCharacter.GetComponent<HealthComponent>();
                _targetStamina = _playerCharacter.GetComponent<StaminaComponent>();
                _targetActionController = _playerCharacter.GetComponent<ActionController>();
                _targetMovement = _playerCharacter.GetComponent<CharacterMovement>();
            }
        }
        
        public AIBrain Brain => _brain;
        public NavMeshAgent Agent => _agent;
        public ActionController ActionController => _actionController;
        public ComboManager ComboManager => _comboManager;
        public Animator Animator => _animator;
        public HealthComponent Health => _health;
        public StaminaComponent Stamina => _stamina;
        public CharacterMovement Movement => _movement;
        
        public Transform CurrentTarget => _currentTarget;
        public HealthComponent TargetHealth => _targetHealth;
        public StaminaComponent TargetStamina => _targetStamina;
        public ActionController TargetActionController => _targetActionController;
        public CharacterMovement TargetMovement => _targetMovement;

        public T GetData<T>(string key) => _data.TryGetValue(key, out var value) ? (T)value : default;
        public void SetData(string key, object value) => _data[key] = value;

        public void SetAgentDestinationToTarget() => Agent.SetDestination(_currentTarget.position);
    }
}
