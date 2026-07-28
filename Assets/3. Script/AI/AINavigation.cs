using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class AINavigation : MonoBehaviour
    {
        private const float ResumeSampleRadius = 1.5f;
        private const float DashSampleRadius = 0.5f;

        private NavMeshAgent _agent;
        private NavMeshPath _path;
        private bool _hasDestination;
        private Vector3 _lastDestination;
        private float _lastStoppingDistance;

        public bool IsDrivingTransform =>
            _agent != null && _agent.enabled && _agent.isOnNavMesh && !_agent.isStopped;

        public bool IsOnNavMesh =>
            _agent != null && _agent.enabled && _agent.isOnNavMesh;

        public Vector3 Velocity => IsOnNavMesh ? _agent.velocity : Vector3.zero;
        public float Speed => _agent != null ? _agent.speed : 0f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _path = new NavMeshPath();
            _agent.updatePosition = true;
            _agent.updateRotation = true;
        }

        public void SetDestination(Vector3 destination, float stoppingDistance = 0f)
        {
            if (!IsOnNavMesh)
            {
                return;
            }

            bool destinationChanged = !_hasDestination ||
                (_lastDestination - destination).sqrMagnitude > 0.01f;
            bool stoppingDistanceChanged = !Mathf.Approximately(_lastStoppingDistance, stoppingDistance);
            bool requiresPath = !_agent.hasPath && !_agent.pathPending;

            _agent.stoppingDistance = stoppingDistance;
            _agent.isStopped = false;

            if (!destinationChanged && !stoppingDistanceChanged && !requiresPath)
            {
                return;
            }

            if (_agent.SetDestination(destination))
            {
                _hasDestination = true;
                _lastDestination = destination;
                _lastStoppingDistance = stoppingDistance;
            }
        }

        public void Suspend()
        {
            if (_agent == null || !_agent.enabled)
            {
                return;
            }

            if (_agent.isOnNavMesh)
            {
                _agent.isStopped = true;
            }

            _agent.enabled = false;
        }

        public bool Resume(Vector3 currentPosition)
        {
            if (_agent == null)
            {
                return false;
            }

            if (_agent.enabled)
            {
                if (_agent.isOnNavMesh)
                {
                    _agent.isStopped = false;
                    return true;
                }

                _agent.enabled = false;
            }

            if (!NavMesh.SamplePosition(currentPosition, out NavMeshHit hit, ResumeSampleRadius, _agent.areaMask))
            {
                return false;
            }

            transform.position = hit.position;
            _agent.enabled = true;

            if (!_agent.Warp(hit.position))
            {
                _agent.enabled = false;
                return false;
            }

            _agent.isStopped = false;
            return true;
        }

        public bool TryGetReachableDestination(
            Vector3 origin,
            Vector3 direction,
            float distance,
            out Vector3 destination)
        {
            destination = origin;
            direction.y = 0f;

            if (_agent == null || direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            if (!NavMesh.SamplePosition(origin, out NavMeshHit start, DashSampleRadius, _agent.areaMask))
            {
                return false;
            }

            Vector3 requestedDestination = origin + direction.normalized * distance;
            if (!NavMesh.SamplePosition(
                    requestedDestination,
                    out NavMeshHit end,
                    DashSampleRadius,
                    _agent.areaMask))
            {
                return false;
            }

            if (!NavMesh.CalculatePath(start.position, end.position, _agent.areaMask, _path) ||
                _path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            destination = end.position;
            return true;
        }

        public bool TryProjectPosition(Vector3 position, out Vector3 projectedPosition)
        {
            projectedPosition = position;

            if (_agent == null ||
                !NavMesh.SamplePosition(position, out NavMeshHit hit, DashSampleRadius, _agent.areaMask))
            {
                return false;
            }

            projectedPosition = hit.position;
            return true;
        }
    }
}
