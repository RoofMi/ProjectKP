using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [field: SerializeField] public float MoveSpeed      { get; private set; } = 5f;
        [field: SerializeField] public float DashSpeed      { get; private set; } = 10f;
        [field: SerializeField] public float DashDuration   { get; private set; } = 0.2f;
        [field: SerializeField] public float JumpPower      { get; private set; } = 5f;
        [field: SerializeField] public float Gravity        { get; private set; } = 9.81f;
        [field: SerializeField] public float RotationSpeed  { get; private set; } = 10f;
        
        [Header("Input Actions")]
        [field: SerializeField] public InputActionReference MoveAction  { get; private set; }
        [field: SerializeField] public InputActionReference JumpAction  { get; private set; }
        [field: SerializeField] public InputActionReference DashAction  { get; private set; }

        [Header("Camera")] 
        [field: SerializeField] public Transform CameraTransform { get; private set; }

        [Header("Runtime")] 
        [SerializeField] private Vector3 _horizontalVelocity;
        [SerializeField] private float _verticalVelocity;

        private CharacterController _controller;
        private Animator _animator;

        private PlayerStateMachine _stateMachine;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator   = GetComponent<Animator>();

            _stateMachine = new PlayerStateMachine(this);
        }

        private void OnEnable()
        {
            MoveAction.action.Enable();
            JumpAction.action.Enable();
            DashAction.action.Enable();
        }

        private void OnDisable()
        {
            MoveAction.action.Disable();
            JumpAction.action.Disable();
            DashAction.action.Disable();
        }

        private void Update()
        {
            _stateMachine.OnUpdate();
            
            // 중력 처리
            if (!_controller.isGrounded)
            {
                _verticalVelocity -= Gravity * Time.deltaTime;
            }
            else if (_verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            
            // 최종 이동 및 회전
            MoveCharacter();
            RotateCharacter();

            float horizontalSpeed = _horizontalVelocity.magnitude;
            _animator.SetFloat("speed", horizontalSpeed);
            _animator.SetBool("isGrounded", IsGrounded());
        }
        
        public Vector2 GetMoveInput()
        {
            return MoveAction.action.ReadValue<Vector2>();
        }

        public Vector3 GetCameraAlignedDirection(Vector2 input2D)
        {
            Vector3 camForward = CameraTransform.forward;
            Vector3 camRight   = CameraTransform.right;

            camForward.y = 0f;
            camRight.y   = 0f;
            camForward.Normalize();
            camRight.Normalize();

            return (camForward * input2D.y + camRight * input2D.x).normalized;
        }
        
        public bool IsDashTriggered()
        {
            return DashAction.action.triggered;
        }
        
        public bool IsJumpTriggered()
        {
            return JumpAction.action.triggered;
        }

        public bool IsGrounded()
        {
            return _controller.isGrounded;
        }
        
        public void MoveCharacter()
        {
            Vector3 finalVelocity = _horizontalVelocity;
            finalVelocity.y = _verticalVelocity;
            _controller.Move(finalVelocity * Time.deltaTime);
        }
        
        public void RotateCharacter()
        {
            Vector3 horizontalVelocity = _horizontalVelocity;
            horizontalVelocity.y = 0f;

            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontalVelocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    RotationSpeed * Time.deltaTime
                );
            }
        }
        
        public void SetHorizontalVelocity(Vector3 newHorizontalVelocity)
        {
            _horizontalVelocity = newHorizontalVelocity;
        }

        public void SetVerticalVelocity(float newVerticalVelocity)
        {
            _verticalVelocity = newVerticalVelocity;
        }
        
        public void SetAnimatorTrigger(string triggerName)
        {
            _animator.SetTrigger(triggerName);
        }

        public void SetAnimatorBool(string boolName, bool value)
        {
            _animator.SetBool(boolName, value);
        }

        public void SetAnimatorFloat(string floatName, float value)
        {
            _animator.SetFloat(floatName, value);
        }
    }
}
