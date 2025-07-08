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
        private InputAction _eAction;
        private InputAction _rAction;
        private InputAction _lightAttackAction;
        private InputAction _heavyAttackAction;

        public void Init(CharacterStateMachine stateMachine, InputBuffer inputBuffer)
        {
            _stateMachine = stateMachine;
            _inputBuffer = inputBuffer;
        }
        
        private void OnEnable()
        {
            var playerInput = GetComponent<PlayerInput>();
            var actionMap = playerInput.currentActionMap;

            _moveAction = actionMap.FindAction("Move");
            _jumpAction = actionMap.FindAction("Jump");
            _dashAction = actionMap.FindAction("Dash");
            _qAction = actionMap.FindAction("Q");
            _eAction = actionMap.FindAction("E");
            _rAction = actionMap.FindAction("R");
            _lightAttackAction = actionMap.FindAction("LightAttack");
            _heavyAttackAction = actionMap.FindAction("HeavyAttack");
            
            _moveAction.Enable();
            _jumpAction.Enable();
            _dashAction.Enable();
            _qAction.Enable();
            _eAction.Enable();
            _rAction.Enable();
            _lightAttackAction.Enable();
            _heavyAttackAction.Enable();

            _jumpAction.performed += OnJumpPerformed;
            _dashAction.performed += OnDashPerformed;
            _qAction.performed += OnComboPerformed;
            _eAction.performed += OnComboPerformed;
            _rAction.performed += OnComboPerformed;
            _lightAttackAction.performed += OnComboPerformed;
            _heavyAttackAction.performed += OnComboPerformed;
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
            _eAction.performed -= OnComboPerformed;
            _rAction.performed -= OnComboPerformed;
            _lightAttackAction.performed -= OnComboPerformed;
            _heavyAttackAction.performed -= OnComboPerformed;

            _moveAction.Disable();
            _jumpAction.Disable();
            _dashAction.Disable();
            _qAction.Disable();
            _eAction.Disable();
            _rAction.Disable();
            _lightAttackAction.Disable();
            _heavyAttackAction.Disable();
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
        }

        private void OnComboEnded()
        {
            _inputBuffer.ClearAllInputs();
        }
    }
}
