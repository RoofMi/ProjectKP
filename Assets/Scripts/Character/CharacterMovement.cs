using UnityEngine;
using Actions;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpPower = 5f;
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float rotationSpeed = 10f;
        
        public float MoveSpeed => moveSpeed;
        public float JumpPower => jumpPower;
        public float Gravity => gravity;
        public float RotationSpeed => rotationSpeed;

        [Header("Camera")] 
        [field: SerializeField] public Transform CameraTransform { get; private set; }
        
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        
        [Header("Debug")]
        [SerializeField] private bool _gravityEnabled = true;
        
        private CharacterController _characterController;
        private ActionController _actionController;
        
        private Vector3 _dashVelocity;
        private Vector2 _inputVector;
        
        public Vector2 GetInputDirection() => _inputVector;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _actionController = GetComponent<ActionController>();
        }
        
        private void Update()
        {
            UpdateMovement(_inputVector, Time.deltaTime);
        }
        
        public void SetInput(Vector2 input)
        {
            _inputVector = input;
        }
        
        public void UpdateMovement(Vector2 inputValue, float deltaTime)
        {
            bool isAttacking = _actionController != null && _actionController.HasTag("Attacking");
            
            if (_dashVelocity.magnitude > 0)
            {
                if (IsGrounded())
                {
                    if (_verticalVelocity < 0f)
                    {
                        _verticalVelocity = -2f;
                    }
                }
                else
                {
                    if (_gravityEnabled)
                    {
                        _verticalVelocity -= Gravity * deltaTime;
                    }
                    else
                    {
                        _verticalVelocity = 0f;
                    }
                }
                
                Vector3 totalVelocity = _dashVelocity;
                totalVelocity.y = _verticalVelocity;
                _characterController.Move(totalVelocity * deltaTime);
            }
            else
            {
                Vector3 moveDirection = GetCameraAlignedDirection(inputValue);
                
                if (!isAttacking)
                {
                    _horizontalVelocity = moveDirection * MoveSpeed;
                }
                else
                {
                    _horizontalVelocity = Vector3.zero;
                }

                if (IsGrounded())
                {
                    if (_verticalVelocity < 0f)
                    {
                        _verticalVelocity = -2f;
                    }
                }
                else
                {
                    if (_gravityEnabled)
                    {
                        _verticalVelocity -= Gravity * deltaTime;
                    }
                    else
                    {
                        _verticalVelocity = 0f;
                    }
                }

                Vector3 moveVelocity = _horizontalVelocity;
                moveVelocity.y = _verticalVelocity;
                _characterController.Move(moveVelocity * deltaTime);
            }
        }
        
        public void UpdateRotation(float deltaTime, bool bInstantRotation = false)
        {
            Vector3 horizontalVelocity = _horizontalVelocity;
            horizontalVelocity.y = 0f;

            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontalVelocity, Vector3.up);

                if (bInstantRotation)
                {
                    transform.rotation = targetRot;
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        RotationSpeed * deltaTime
                    );
                }
            }
        }
        
        public void SetRotationToDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        public void StartJump()
        {
            _verticalVelocity = JumpPower;
        }
        
        private Vector3 GetCameraAlignedDirection(Vector2 inputValue)
        {
            Vector3 camForward = CameraTransform.forward;
            Vector3 camRight   = CameraTransform.right;

            camForward.y = 0f;
            camRight.y   = 0f;
            
            camForward.Normalize();
            camRight.Normalize();

            Vector3 direction = camForward * inputValue.y + camRight * inputValue.x;
            
            return direction.normalized;
        }

        public bool IsGrounded()
        {
            return _characterController.isGrounded;
        }

        public float GetSpeed()
        {
            return _horizontalVelocity.magnitude;
        }
        
        public void SetGravityEnabled(bool enabled)
        {
            _gravityEnabled = enabled;
        }
        
        public void ResetVerticalVelocity()
        {
            _verticalVelocity = 0f;
        }
        
        public void SetDashVelocity(Vector3 velocity)
        {
            _dashVelocity = velocity;
            _dashVelocity.y = 0f;
        }
        
        public void ResetDashVelocity()
        {
            _dashVelocity = Vector3.zero;
        }
        
        public void ApplyDashDrag(float drag, float deltaTime)
        {
            _dashVelocity.x /= 1 + drag * deltaTime;
            _dashVelocity.z /= 1 + drag * deltaTime;
        }
        
    }
}
