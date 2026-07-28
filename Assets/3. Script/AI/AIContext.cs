using System.Collections.Generic;
using Character;
using Combat;
using UnityEngine;

namespace AI
{
    public class AIContext
    {
        private readonly AIBrain _brain;
        private readonly AINavigation _navigation;
        private readonly ActionController _actionController;
        private readonly ComboManager _comboManager;
        private readonly Animator _animator;
        private readonly HealthComponent _health;
        private readonly StaminaComponent _stamina;
        private readonly CharacterMovement _movement;

        private readonly Transform _currentTarget;
        private readonly HealthComponent _targetHealth;
        private readonly StaminaComponent _targetStamina;
        private readonly ActionController _targetActionController;
        private readonly CharacterMovement _targetMovement;

        private readonly Dictionary<string, object> _data = new();

        public AIContext(AIBrain brain, Transform playerCharacter)
        {
            _brain = brain;
            _currentTarget = playerCharacter;

            if (brain == null)
            {
                return;
            }

            _navigation = brain.GetComponent<AINavigation>();
            _actionController = brain.GetComponent<ActionController>();
            _comboManager = brain.GetComponent<ComboManager>();
            _animator = brain.GetComponent<Animator>();
            _health = brain.GetComponent<HealthComponent>();
            _stamina = brain.GetComponent<StaminaComponent>();
            _movement = brain.GetComponent<CharacterMovement>();

            if (playerCharacter == null)
            {
                return;
            }

            _targetHealth = playerCharacter.GetComponent<HealthComponent>();
            _targetStamina = playerCharacter.GetComponent<StaminaComponent>();
            _targetActionController = playerCharacter.GetComponent<ActionController>();
            _targetMovement = playerCharacter.GetComponent<CharacterMovement>();
        }

        public bool IsValid =>
            _brain != null &&
            _currentTarget != null &&
            _navigation != null &&
            _actionController != null &&
            _comboManager != null &&
            _animator != null &&
            _health != null &&
            _stamina != null &&
            _movement != null;

        public AIBrain Brain => _brain;
        public AINavigation Navigation => _navigation;
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

        public bool TryGetData<T>(string key, out T value)
        {
            if (_data.TryGetValue(key, out object storedValue) && storedValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        public T GetData<T>(string key, T fallback = default)
        {
            return TryGetData(key, out T value) ? value : fallback;
        }

        public float GetFloat(string key, float fallback = 0f)
        {
            if (!_data.TryGetValue(key, out object value))
            {
                return fallback;
            }

            return value switch
            {
                byte number => number,
                short number => number,
                int number => number,
                long number => number,
                float number => number,
                double number => (float)number,
                decimal number => (float)number,
                _ => fallback
            };
        }

        public void SetData(string key, object value)
        {
            _data[key] = value;
        }





        public bool IsActionOnCooldown(Actions.Core.ActionBase action)
        {
            return _actionController != null && _actionController.IsActionOnCooldown(action);
        }

        public float GetActionCooldownRemaining(Actions.Core.ActionBase action)
        {
            return _actionController != null ? _actionController.GetCooldownRemaining(action) : 0f;
        }
    }
}
