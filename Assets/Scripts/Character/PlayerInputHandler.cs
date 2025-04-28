using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private CharacterStateMachine _stateMachine;
        private InputBuffer _inputBuffer;
        
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _qAction;
        private InputAction _clickAction;

        public void Init(CharacterStateMachine stateMachine, InputBuffer inputBuffer)
        {
            _stateMachine = stateMachine;
            _inputBuffer = inputBuffer;
            
            _stateMachine.OnComboEnded += OnComboEnded;
        }
        
        private void OnEnable()
        {
            var playerInput = GetComponent<PlayerInput>();
            var actionMap = playerInput.currentActionMap;

            _moveAction = actionMap.FindAction("Move");
            _jumpAction = actionMap.FindAction("Jump");
            _dashAction = actionMap.FindAction("Dash");
            _qAction = actionMap.FindAction("Q");
            _clickAction = actionMap.FindAction("Click");
            
            _moveAction.Enable();
            _jumpAction.Enable();
            _dashAction.Enable();
            _qAction.Enable();
            _clickAction.Enable();

            _jumpAction.performed += OnJumpPerformed;
            _dashAction.performed += OnDashPerformed;
            _qAction.performed += OnComboPerformed;
            _clickAction.performed += OnComboPerformed;
        }

        private void Update()
        {
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            _stateMachine.OnMoveInput(moveInput);
            
            _inputBuffer.UpdateBuffer();
        }

        private void OnDisable()
        {
            _jumpAction.performed -= OnJumpPerformed;
            _dashAction.performed -= OnDashPerformed;
            _qAction.performed -= OnComboPerformed;
            _clickAction.performed -= OnComboPerformed;

            _moveAction.Disable();
            _jumpAction.Disable();
            _dashAction.Disable();
            _qAction.Disable();
            _clickAction.Disable();
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TryJump();
        }
        
        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TryDash();
        }

        private void OnComboPerformed(InputAction.CallbackContext context)
        {
            string actionName = context.action.name;
            _inputBuffer.AddInput(actionName);
            
            _stateMachine.OnComboInput();
        }

        private void OnComboEnded()
        {
            _inputBuffer.ClearAllInputs();
        }
    }
}
