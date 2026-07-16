using Actions;
using Character.Core;
using Combat;
using UnityEngine;
using UnityEngine.AI;

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

        [Header("Camera")]
        [field: SerializeField] public Transform CameraTransform { get; private set; }

        [Header("Combat Movement")]
        [SerializeField, Min(0f)] private float comboContactPadding = 0.05f;

        [Header("Debug")]
        [SerializeField] private bool _gravityEnabled = true;

        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private Vector3 _groundPushDirection;
        private float _groundPushDistance;
        private float _groundPushDuration;
        private float _groundPushElapsed;
        private float _groundPushAppliedDistance;
        private Vector3 _movementDisplacement;
        private Vector3 _rootMotionDisplacement;
        private CharacterController _characterController;
        private ActionController _actionController;
        private ComboManager _comboManager;
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private Vector2 _inputVector;

        public float MoveSpeed => moveSpeed;
        public float JumpPower => jumpPower;
        public float Gravity => gravity;
        public float RotationSpeed => rotationSpeed;

        private bool IsAIControlled => _navMeshAgent != null;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _actionController = GetComponent<ActionController>();
            _comboManager = GetComponent<ComboManager>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            if (_navMeshAgent != null)
            {
                _navMeshAgent.updatePosition = true;
                _navMeshAgent.updateRotation = true;
            }
        }

        private void Update()
        {
            if (IsAIControlled && _navMeshAgent.enabled)
            {
                _movementDisplacement = Vector3.zero;
                _rootMotionDisplacement = Vector3.zero;
                UpdateAgentLocomotion();
                return;
            }

            UpdateMovement(_inputVector, Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (IsAIControlled && _navMeshAgent.enabled)
            {
                return;
            }

            ApplyDisplacement(_movementDisplacement + _rootMotionDisplacement);
            _movementDisplacement = Vector3.zero;
            _rootMotionDisplacement = Vector3.zero;
        }

        private void OnAnimatorMove()
        {
            if (_animator == null || !_animator.applyRootMotion)
            {
                _rootMotionDisplacement = Vector3.zero;
                return;
            }

            _rootMotionDisplacement = ClampComboRootMotion(_animator.deltaPosition);
            transform.rotation *= _animator.deltaRotation;
        }

        private void OnDisable()
        {
            CancelGroundPush();
            _movementDisplacement = Vector3.zero;
            _rootMotionDisplacement = Vector3.zero;
        }

        public Vector2 GetInputDirection()
        {
            return _inputVector;
        }

        public void SetInput(Vector2 input)
        {
            _inputVector = input;
        }

        public void UpdateMovement(Vector2 inputValue, float deltaTime)
        {
            bool isMovementLocked = _actionController != null &&
                (_actionController.HasTag(ActionTags.Attacking) || _actionController.HasTag(ActionTags.Stunned));
            Vector3 moveDirection = GetCameraAlignedDirection(inputValue);
            _horizontalVelocity = isMovementLocked ? Vector3.zero : moveDirection * MoveSpeed;

            if (IsGrounded())
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else if (_gravityEnabled)
            {
                _verticalVelocity -= Gravity * deltaTime;
            }
            else
            {
                _verticalVelocity = 0f;
            }

            Vector3 moveVelocity = _horizontalVelocity;
            moveVelocity.y = _verticalVelocity;
            _movementDisplacement = moveVelocity * deltaTime + GetGroundPushDelta(deltaTime);
        }

        public void ApplyGroundPush(Vector3 direction, float distance, float duration)
        {
            direction.y = 0f;
            distance = Mathf.Max(0f, distance);

            if (direction.sqrMagnitude <= Mathf.Epsilon || distance <= Mathf.Epsilon)
            {
                CancelGroundPush();
                return;
            }

            _groundPushDirection = direction.normalized;
            _groundPushDistance = distance;
            _groundPushDuration = Mathf.Max(0f, duration);
            _groundPushElapsed = 0f;
            _groundPushAppliedDistance = 0f;
        }

        public void CancelGroundPush()
        {
            _groundPushDirection = Vector3.zero;
            _groundPushDistance = 0f;
            _groundPushDuration = 0f;
            _groundPushElapsed = 0f;
            _groundPushAppliedDistance = 0f;
        }

        public void ApplyDisplacement(Vector3 displacement)
        {
            _characterController.Move(displacement);
        }

        public void UpdateRotation(float deltaTime, bool bInstantRotation = false)
        {
            Vector3 horizontalVelocity = _horizontalVelocity;
            horizontalVelocity.y = 0f;

            if (horizontalVelocity.sqrMagnitude <= 0.01f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity, Vector3.up);
            transform.rotation = bInstantRotation
                ? targetRotation
                : Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * deltaTime);
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

        private Vector3 ClampComboRootMotion(Vector3 displacement)
        {
            if (_actionController == null || !_actionController.HasTag(ActionTags.Attacking) || _comboManager == null)
            {
                return displacement;
            }

            Transform target = _comboManager.ComboTarget;
            if (target == null)
            {
                return displacement;
            }

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);

            if (toTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                displacement.x = 0f;
                displacement.z = 0f;
                return displacement;
            }

            float stopDistance = GetWorldRadius(_characterController) +
                GetWorldRadius(target.GetComponent<CharacterController>()) + comboContactPadding;
            float allowedDistance = Mathf.Max(0f, toTarget.magnitude - stopDistance);

            if (Vector3.Dot(horizontalDisplacement, toTarget) > 0f &&
                horizontalDisplacement.sqrMagnitude > allowedDistance * allowedDistance)
            {
                horizontalDisplacement = horizontalDisplacement.normalized * allowedDistance;
            }

            Debug.Assert(horizontalDisplacement.sqrMagnitude <= allowedDistance * allowedDistance + 0.0001f ||
                Vector3.Dot(horizontalDisplacement, toTarget) <= 0f);

            displacement.x = horizontalDisplacement.x;
            displacement.z = horizontalDisplacement.z;
            return displacement;
        }

        private static float GetWorldRadius(CharacterController characterController)
        {
            if (characterController == null)
            {
                return 0f;
            }

            Vector3 scale = characterController.transform.lossyScale;
            return characterController.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        }

        private Vector3 GetGroundPushDelta(float deltaTime)
        {
            if (_groundPushDistance <= Mathf.Epsilon)
            {
                return Vector3.zero;
            }

            _groundPushElapsed = Mathf.Min(_groundPushElapsed + deltaTime, _groundPushDuration);
            float progress = _groundPushDuration <= Mathf.Epsilon
                ? 1f
                : _groundPushElapsed / _groundPushDuration;
            float targetDistance = _groundPushDistance * Mathf.SmoothStep(0f, 1f, progress);
            float frameDistance = targetDistance - _groundPushAppliedDistance;
            _groundPushAppliedDistance = targetDistance;
            Vector3 displacement = _groundPushDirection * frameDistance;

            if (progress >= 1f)
            {
                CancelGroundPush();
            }

            return displacement;
        }

        private Vector3 GetCameraAlignedDirection(Vector2 inputValue)
        {
            if (CameraTransform == null)
            {
                return Vector3.zero;
            }

            Vector3 cameraForward = CameraTransform.forward;
            Vector3 cameraRight = CameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            return (cameraForward * inputValue.y + cameraRight * inputValue.x).normalized;
        }
    }
}
