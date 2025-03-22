using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float dashSpeed = 10f;
        public float dashDuration = 0.2f;
        public float jumpPower = 5f;
        public float gravity = 9.81f;
        public float rotationSpeed = 10f;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference dashAction;

        private CharacterController _controller;
        private Animator _animator;

        private Vector3 _moveDirection;
        private float _verticalVelocity;

        private bool _isDashing;
        private float _dashTimer;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            dashAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();
            dashAction.action.Disable();
        }

        private void Update()
        {
            if (!_isDashing)
            {
                HandleMovementInput();
                HandleJump();
                HandleDashInput();
            }
            else
            {
                UpdateDashTimer();
            }

            ApplyGravityIfNeeded();
            HandleRotation();
            UpdateAnimatorParams();
            MoveCharacter();
        }

        private void HandleMovementInput()
        {
            Vector2 input2D = moveAction.action.ReadValue<Vector2>();
            Vector3 inputDir = new Vector3(input2D.x, 0f, input2D.y).normalized;

            _moveDirection = inputDir * moveSpeed;
        }

        private void HandleJump()
        {
            bool isGrounded = _controller.isGrounded;
            _animator.SetBool("isGrounded", isGrounded);

            if (isGrounded)
            {
                if (_verticalVelocity < 0f)
                    _verticalVelocity = -1f;

                if (jumpAction.action.triggered)
                {
                    _verticalVelocity = jumpPower;
                    _animator.SetTrigger("jumpTrigger");
                }
            }
        }

        private void HandleDashInput()
        {
            Vector2 input2D = moveAction.action.ReadValue<Vector2>();
            Vector3 inputDir = new Vector3(input2D.x, 0f, input2D.y);

            if (dashAction.action.triggered && inputDir.magnitude > 0.1f)
            {
                StartDash(inputDir.normalized);
            }
        }

        private void StartDash(Vector3 dashDir)
        {
            _isDashing     = true;
            _dashTimer     = dashDuration;
            _moveDirection = dashDir * dashSpeed;
            _animator.SetTrigger("dashStartTrigger");
        }

        private void UpdateDashTimer()
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                EndDash();
            }
        }

        private void EndDash()
        {
            _isDashing = false;
            _animator.SetTrigger("dashEndTrigger");
        }

        private void ApplyGravityIfNeeded()
        {
            if (!_controller.isGrounded || _isDashing)
            {
                _verticalVelocity -= gravity * Time.deltaTime;
            }
        }

        private void HandleRotation()
        {
            if (!_isDashing)
            {
                Vector3 horizontalDir = new Vector3(_moveDirection.x, 0f, _moveDirection.z);

                if (horizontalDir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(horizontalDir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }
        }

        private void UpdateAnimatorParams()
        {
            float horizontalSpeed = new Vector2(_controller.velocity.x, _controller.velocity.z).magnitude;
            _animator.SetFloat("speed", horizontalSpeed);
            _animator.SetFloat("velocityY", _verticalVelocity);
        }

        private void MoveCharacter()
        {
            Vector3 finalVelocity = _moveDirection;
            finalVelocity.y = _verticalVelocity;
            _controller.Move(finalVelocity * Time.deltaTime);
        }
    }
}
