using UnityEngine;
using Combat;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        private CharacterMovement _movement;
        private PlayerInputHandler _inputHandler;
        private Animator _animator;
        private ComboManager _comboManager;
        private HealthComponent _health;
        private StaminaComponent _stamina;

        public HealthComponent Health => _health;
        public StaminaComponent Stamina => _stamina;

        private void Awake()
        {
            _movement = GetComponent<CharacterMovement>();
            _animator = GetComponent<Animator>();
            _comboManager = GetComponent<ComboManager>();
            _health = GetComponent<HealthComponent>();
            _stamina = GetComponent<StaminaComponent>();
            
            _inputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            // Action 시스템이 모든 것을 처리
        }

    }
}
