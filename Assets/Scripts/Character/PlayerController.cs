using UnityEngine;
using Combat;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        private CharacterMovement _movement;
        private PlayerInputHandler _inputHandler;
        private CharacterStateMachine _stateMachine;
        private Animator _animator;
        private InputBuffer _inputBuffer;
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

            _inputBuffer = new InputBuffer();
            
            _stateMachine = new CharacterStateMachine(_movement, _animator, _inputBuffer, _comboManager, _stamina);
            
            _inputHandler = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            // _stateMachine.OnUpdate();
        }

    }
}
