using Character;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraLockOn : CinemachineExtension
{
    [Header("Lock-On Settings")]
    [Tooltip("플레이어 또는 카메라의 기준이 될 Transform")]
    [SerializeField] private Transform _playerTransform;
    [Tooltip("락온이 가능한 최대 거리")]
    [SerializeField] private float _maxLockOnDistance = 300f;
    [SerializeField] private float _cameraPositionY = 3f;

    private Transform _lockOnTarget;
    private bool _isLockOn = false;
    private CinemachineInputAxisController _inputAxisController;
    private CinemachineOrbitalFollow _orbitalFollow;
    private CinemachineRotationComposer _rotationComposer;

    protected override void Awake()
    {
        base.Awake();
        
        _inputAxisController = GetComponent<CinemachineInputAxisController>();
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _rotationComposer = GetComponent<CinemachineRotationComposer>();
    }

    // 매 프레임 입력을 감지하기 위해 Update 함수를 사용합니다.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleLockOn();
        }

        if (_isLockOn && _lockOnTarget != null)
        {
            if ((_playerTransform.position - _lockOnTarget.position).sqrMagnitude > _maxLockOnDistance)
            {
                SetLockOn(false);
            }
        }
    }

    private void ToggleLockOn()
    {
        if (!_isLockOn)
        {
            _lockOnTarget = FindNearestEnemy();

            if (_lockOnTarget != null)
            {
                SetLockOn(true);
            }
        }
        else
        {
            _lockOnTarget = null;

            SetLockOn(false);
        }
    }

    private void SetLockOn(bool setBool)
    {
        if (setBool)
        {
            _inputAxisController.enabled = false;
            _orbitalFollow.enabled = false;
            _rotationComposer.enabled = false;

            _isLockOn = true;

            Debug.Log("LockOn Enabled: " + _lockOnTarget.name);
        }
        else
        {
            _inputAxisController.enabled = true;
            _orbitalFollow.enabled = true;
            _rotationComposer.enabled = true;

            _lockOnTarget = null;

            _isLockOn = false;

            Debug.Log("LockOn Disabled");
        }
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        if (_playerTransform == null)
            return null;

        foreach (var enemy in enemies)
        {
            float distance = (_playerTransform.position - enemy.transform.position).sqrMagnitude;
            if (distance < minDistance && distance <= _maxLockOnDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        return nearestEnemy;
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (_lockOnTarget != null && stage == CinemachineCore.Stage.Aim)
        {
            Vector3 targetPosition = _playerTransform.position + (((_playerTransform.position - _lockOnTarget.position).normalized) * 5f);
            targetPosition.y = _cameraPositionY;

            state.RawPosition = Vector3.Lerp(state.RawPosition, targetPosition, 10f * deltaTime);


            Vector3 dir = (_lockOnTarget.position + (Vector3.up * 1.2f)) - state.RawPosition;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            state.RawOrientation = Quaternion.Slerp(state.RawOrientation, targetRotation, 50f * deltaTime);
        }
    }
}