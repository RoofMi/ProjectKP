using UnityEngine;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        private CharacterMovement _movement;
        private PlayerInputHandler _inputHandler;
        private CharacterStateMachine _stateMachine;
        private Animator _animator;

        private void Awake()
        {
            _movement = GetComponent<CharacterMovement>();
            _animator = GetComponent<Animator>();
            
            _stateMachine = new CharacterStateMachine(_movement, _animator);
            
            _inputHandler = GetComponent<PlayerInputHandler>();
            _inputHandler.Init(_stateMachine);
        }

        private void Update()
        {
            _stateMachine.OnUpdate();
        }
    }
}
