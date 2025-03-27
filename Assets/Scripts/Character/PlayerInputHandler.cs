using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private CharacterStateMachine _stateMachine;
        
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;

        public void Init(CharacterStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        private void OnEnable()
        {
            var playerInput = GetComponent<PlayerInput>();
            var actionMap = playerInput.currentActionMap;

            _moveAction = actionMap.FindAction("Move");
            _jumpAction = actionMap.FindAction("Jump");
            _dashAction = actionMap.FindAction("Dash");
            
            _moveAction.Enable();
            _jumpAction.Enable();
            _dashAction.Enable();

            _jumpAction.performed += OnJumpPerformed;
            _dashAction.performed += OnDashPerformed;
        }

        private void Update()
        {
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            _stateMachine.OnMoveInput(moveInput);
        }

        private void OnDisable()
        {
            _jumpAction.performed -= OnJumpPerformed;
            _dashAction.performed -= OnDashPerformed;

            _moveAction.Disable();
            _jumpAction.Disable();
            _dashAction.Disable();
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TryJump();
        }
        
        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TryDash();
        }
    }
}
