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
        private InputAction _qSkillAction;
        private InputAction _comboAction;

        public void Init(CharacterStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnComboEnded += OnComboEnded;
        }
        
        private void OnEnable()
        {
            var playerInput = GetComponent<PlayerInput>();
            var actionMap = playerInput.currentActionMap;

            _moveAction = actionMap.FindAction("Move");
            _jumpAction = actionMap.FindAction("Jump");
            _dashAction = actionMap.FindAction("Dash");
            _qSkillAction = actionMap.FindAction("QSkill");
            _comboAction = actionMap.FindAction("Combo");
            
            _moveAction.Enable();
            _jumpAction.Enable();
            _dashAction.Enable();
            _qSkillAction.Enable();
            _comboAction.Enable();

            _jumpAction.performed += OnJumpPerformed;
            _dashAction.performed += OnDashPerformed;
            _qSkillAction.performed += OnQSkillPerformed;
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
            _qSkillAction.performed -= OnQSkillPerformed;

            _moveAction.Disable();
            _jumpAction.Disable();
            _dashAction.Disable();
            _qSkillAction.Disable();
            _comboAction.Disable();
        }
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TryJump();
        }
        
        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TryDash();
        }

        private void OnQSkillPerformed(InputAction.CallbackContext context)
        {
            _comboAction.performed += OnComboPerformed;
            _stateMachine.TryAttack();
        }

        private void OnComboPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.OnComboInput();
        }

        private void OnComboEnded()
        {
            _comboAction.performed -= OnComboPerformed;
        }
    }
}
