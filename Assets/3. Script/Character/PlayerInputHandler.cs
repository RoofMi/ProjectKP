using Actions;
using Combat;
using Character.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private ActionController _actionController;
        [SerializeField] private CharacterMovement _movement;
        [SerializeField] private Animator _animator;
        [SerializeField] private ComboManager _comboManager;
        
        [Header("Actions")]
        [SerializeField] private JumpAction _jumpAction;
        [SerializeField] private DashAction _dashAction;
        
        private PlayerInput _playerInput;
        private InputAction _moveInput;
        private InputAction _jumpInput;
        private InputAction _dashInput;
        private InputAction _lightAttackInput;
        private InputAction _heavyAttackInput;
        
        private Vector2 _currentMoveInput;
        private InputBuffer _inputBuffer;
        
        private void Awake()
        {
            if (_actionController == null)
                _actionController = GetComponent<ActionController>();
            if (_movement == null)
                _movement = GetComponent<CharacterMovement>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
            if (_comboManager == null)
                _comboManager = GetComponent<ComboManager>();
            
            _playerInput = GetComponent<PlayerInput>();
            _inputBuffer = new InputBuffer();
        }
        
        private void OnEnable()
        {
            var actionMap = _playerInput.currentActionMap;
            
            _moveInput = actionMap.FindAction("Move");
            _jumpInput = actionMap.FindAction("Jump");
            _dashInput = actionMap.FindAction("Dash");
            _lightAttackInput = actionMap.FindAction("LightAttack");
            _heavyAttackInput = actionMap.FindAction("HeavyAttack");
            
            _moveInput.Enable();
            _jumpInput.Enable();
            _dashInput.Enable();
            _lightAttackInput.Enable();
            _heavyAttackInput.Enable();
            
            _moveInput.performed += OnMove;
            _moveInput.canceled += OnMove;
            _jumpInput.performed += OnJump;
            _dashInput.performed += OnDash;
            _lightAttackInput.performed += OnLightAttack;
            _heavyAttackInput.performed += OnHeavyAttack;
        }
        
        private void OnDisable()
        {
            _moveInput.performed -= OnMove;
            _moveInput.canceled -= OnMove;
            _jumpInput.performed -= OnJump;
            _dashInput.performed -= OnDash;
            _lightAttackInput.performed -= OnLightAttack;
            _heavyAttackInput.performed -= OnHeavyAttack;
            
            _moveInput.Disable();
            _jumpInput.Disable();
            _dashInput.Disable();
            _lightAttackInput.Disable();
            _heavyAttackInput.Disable();
        }
        
        private void Update()
        {
            if (!_actionController.HasTag(ActionTags.Dashing) && !_actionController.HasTag(ActionTags.Stunned))
            {
                _movement.UpdateRotation(Time.deltaTime);
            }
            
            UpdateGroundedState();
            UpdateAnimationParameters();
            ProcessBufferedInputs();
        }
        
        private void UpdateGroundedState()
        {
            if (_movement.IsGrounded())
            {
                if (!_actionController.HasTag(ActionTags.Grounded))
                {
                    _actionController.AddTag(ActionTags.Grounded);
                    _actionController.RemoveTag(ActionTags.Airborne);
                    _actionController.RemoveTag("AirDashUsed");
                }
            }
            else
            {
                if (!_actionController.HasTag(ActionTags.Airborne))
                {
                    _actionController.RemoveTag(ActionTags.Grounded);
                    _actionController.AddTag(ActionTags.Airborne);
                }
            }
        }
        
        private void UpdateAnimationParameters()
        {
            if (_animator == null) return;
            
            float inputMagnitude = _currentMoveInput.magnitude;
            _animator.SetFloat(AnimationHashes.Speed, inputMagnitude);
            _animator.SetBool(AnimationHashes.IsGrounded, _movement.IsGrounded());
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            _currentMoveInput = context.ReadValue<Vector2>();
            _movement.SetInput(_currentMoveInput);
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            if (_jumpAction != null)
                _actionController.TryExecuteAction(_jumpAction);
        }
        
        private void OnDash(InputAction.CallbackContext context)
        {
            if (_dashAction != null)
                _actionController.TryExecuteAction(_dashAction, _currentMoveInput);
        }
        
        private void OnLightAttack(InputAction.CallbackContext context)
        {
            if (_inputBuffer != null && !_actionController.HasTag(ActionTags.Stunned))
            {
                _inputBuffer.AddInput("LightAttack");
            }
        }
        
        private void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (_inputBuffer != null && !_actionController.HasTag(ActionTags.Stunned))
            {
                _inputBuffer.AddInput("HeavyAttack");
            }
        }
        
        private void ProcessBufferedInputs()
        {
            if (_inputBuffer == null || _comboManager == null)
                return;

            if (_actionController.HasTag(ActionTags.Stunned))
            {
                ClearInputBuffer();
                return;
            }

            _inputBuffer.UpdateBuffer();
            if (_inputBuffer.HasInput())
            {
                bool canStartNewCombo = !_actionController.HasTag(ActionTags.Attacking);
                bool canContinueCombo = _actionController.HasTag(ActionTags.Attacking) && _comboManager.IsInComboWindow;
                
                if (canStartNewCombo || canContinueCombo)
                {
                    string nextInput = _inputBuffer.GetNextInput();
                    if (nextInput != null)
                    {
                        bool success = _comboManager.TryExecuteCombo(nextInput);
                    }
                }
            }
        }
        
        public void ClearInputBuffer()
        {
            _inputBuffer?.ClearAllInputs();
        }
    }
}
