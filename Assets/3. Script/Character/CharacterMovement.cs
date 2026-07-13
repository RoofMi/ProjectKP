using UnityEngine;
using UnityEngine.AI;
using Actions;
using Character.Core;

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
        private NavMeshAgent _navMeshAgent;  // AI 지원
        private Animator _animator;
        
        private Vector2 _inputVector;
        private bool IsAIControlled => _navMeshAgent != null;
        
        public Vector2 GetInputDirection() => _inputVector;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _actionController = GetComponent<ActionController>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            
            // NavMeshAgent 설정 (AI 캐릭터인 경우)
            if (_navMeshAgent != null)
            {
                // 이미 AIContext에서 설정했지만 안전을 위해 재확인
                _navMeshAgent.updatePosition = true;
                _navMeshAgent.updateRotation = true;
            }
        }
        
        private void Update()
        {
            if (IsAIControlled && _navMeshAgent.enabled)
            {
                UpdateAgentLocomotion();
                return;
            }

            UpdateMovement(_inputVector, Time.deltaTime);
        }

        private void UpdateAgentLocomotion()
        {
            if (_animator == null)
            {
                return;
            }

            float speed = _navMeshAgent.speed > 0f
                ? Mathf.Clamp01(_navMeshAgent.velocity.magnitude / _navMeshAgent.speed)
                : 0f;

            _animator.applyRootMotion = false;
            _animator.SetFloat(AnimationHashes.Speed, speed);
            _animator.SetBool(AnimationHashes.IsGrounded, true);
        }
        
        public void SetInput(Vector2 input)
        {
            _inputVector = input;
        }
        
        public void UpdateMovement(Vector2 inputValue, float deltaTime)
        {
            bool isAttacking = _actionController != null && _actionController.HasTag(ActionTags.Attacking);
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
            // AI 캐릭터는 카메라가 없으므로 체크
            if (CameraTransform == null)
            {
                // AI는 NavMeshAgent로 이동하므로 이 메소드가 호출되면 안 됨
                // 혹시 호출되면 Zero 반환
                return Vector3.zero;
            }
            
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
            if (IsAIControlled && _navMeshAgent.enabled)
            {
                return _navMeshAgent.velocity.magnitude;
            }

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
        
    }
}
