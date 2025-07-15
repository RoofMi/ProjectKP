using UnityEngine;
using UnityEngine.InputSystem;
using ProjectKP.Actions;

namespace Character
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private ActionController _actionController;

        [SerializeField] private CharacterMovement _movement;
        [SerializeField] private Animator _animator;
        
        [Header("Actions")]
        [SerializeField] private JumpAction _jumpAction;
        [SerializeField] private DashAction _dashAction;
        
        private PlayerInput _playerInput;
        private InputAction _moveInput;
        private InputAction _jumpInput;
        private InputAction _dashInput;
        
        private Vector2 _currentMoveInput;
        
        private void Awake()
        {
            if (_actionController == null)
                _actionController = GetComponent<ActionController>();
            if (_movement == null)
                _movement = GetComponent<CharacterMovement>();
            if (_animator == null)
                _animator = GetComponent<Animator>();
            
            _playerInput = GetComponent<PlayerInput>();
        }
        
        private void OnEnable()
        {
            var actionMap = _playerInput.currentActionMap;
            
            _moveInput = actionMap.FindAction("Move");
            _jumpInput = actionMap.FindAction("Jump");
            _dashInput = actionMap.FindAction("Dash");
            
            _moveInput.Enable();
            _jumpInput.Enable();
            _dashInput.Enable();
            
            _moveInput.performed += OnMove;
            _moveInput.canceled += OnMove;
            _jumpInput.performed += OnJump;
            _dashInput.performed += OnDash;
        }
        
        private void OnDisable()
        {
            _moveInput.performed -= OnMove;
            _moveInput.canceled -= OnMove;
            _jumpInput.performed -= OnJump;
            _dashInput.performed -= OnDash;
            
            _moveInput.Disable();
            _jumpInput.Disable();
            _dashInput.Disable();
        }
        
        private void Update()
        {
            if (!_actionController.HasTag("Dashing"))
            {
                _movement.UpdateRotation(Time.deltaTime);
            }
            
            UpdateGroundedState();
            UpdateAnimationParameters();
        }
        
        private void UpdateGroundedState()
        {
            if (_movement.IsGrounded())
            {
                if (!_actionController.HasTag("Grounded"))
                {
                    _actionController.AddTag("Grounded");
                    _actionController.RemoveTag("Airborne");
                    _actionController.RemoveTag("AirDashUsed");
                }
            }
            else
            {
                if (!_actionController.HasTag("Airborne"))
                {
                    _actionController.RemoveTag("Grounded");
                    _actionController.AddTag("Airborne");
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
    }
}