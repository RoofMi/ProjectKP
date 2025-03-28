using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [field: SerializeField] public float MoveSpeed      { get; private set; } = 5f;
        [field: SerializeField] public float DashSpeed      { get; private set; } = 10f;
        [field: SerializeField] public float DashDuration   { get; private set; } = 0.2f;
        [field: SerializeField] public float JumpPower      { get; private set; } = 5f;
        [field: SerializeField] public float Gravity        { get; private set; } = 9.81f;
        [field: SerializeField] public float RotationSpeed  { get; private set; } = 10f;

        [Header("Camera")] 
        [field: SerializeField] public Transform CameraTransform { get; private set; }
        
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        
        private CharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public void UpdateMovement(Vector2 inputValue, float deltaTime, bool bIsDashing = false)
        {
            Vector3 moveDirection = GetCameraAlignedDirection(inputValue);
            _horizontalVelocity = bIsDashing ? moveDirection * DashSpeed : moveDirection * MoveSpeed;

            if (IsGrounded())
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                _verticalVelocity -= Gravity * deltaTime;
            }

            Vector3 moveVelocity = _horizontalVelocity;
            moveVelocity.y = _verticalVelocity;
            
            _characterController.Move(moveVelocity * Time.deltaTime);
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

            return (camForward * inputValue.y + camRight * inputValue.x).normalized;
        }

        public bool IsGrounded()
        {
            return _characterController.isGrounded;
        }

        public float GetSpeed()
        {
            return _horizontalVelocity.magnitude;
        }
    }
}
