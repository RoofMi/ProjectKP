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
        private InputAction _Attack_Action;
        private InputAction _skill_Q_Action;

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

            _Attack_Action = actionMap.FindAction("Attack");
            _skill_Q_Action = actionMap.FindAction("Skill_Q");


            _moveAction.Enable();
            _jumpAction.Enable();
            _dashAction.Enable();

            _Attack_Action.Enable();
            _skill_Q_Action.Enable();


            _jumpAction.performed += OnJumpPerformed;
            _dashAction.performed += OnDashPerformed;

            _Attack_Action.performed += OnAttackPerformed;
            _skill_Q_Action.performed += OnSkillQPerformed;
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

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TryAttackAction();
        }

        private void OnSkillQPerformed(InputAction.CallbackContext context)
        {
            _stateMachine.TrySkillQAction();
        }
    }
}
