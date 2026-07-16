using System.Collections;
using System.Collections.Generic;
using Actions;
using Actions.Core;
using Character.Core;
using Combat;
using UnityEngine;
using UnityEngine.AI;

namespace Character
{
    public class ActionController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterMovement _movement;
        [SerializeField] private StaminaComponent _stamina;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        [Header("Reactions")]
        [SerializeField] private HitReactionAction _hitReactionAction;
        
        [Header("Debug")]
        [SerializeField] private List<ActiveAction> _activeActions = new();
        [SerializeField] private List<string> _currentTags = new() { ActionTags.Grounded };
        
        private HashSet<string> _tags;
        private ActionContext _context;
        private HealthComponent _health;
        private ComboManager _comboManager;
        private PlayerInputHandler _playerInputHandler;
        private Dictionary<ActionBase, float> _cooldownEndTimes = new();
        
        private void Awake()
        {
            _tags = new HashSet<string>(_currentTags);
            
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_movement == null) _movement = GetComponent<CharacterMovement>();
            if (_stamina == null) _stamina = GetComponent<StaminaComponent>();
            if (_navMeshAgent == null) _navMeshAgent = GetComponent<NavMeshAgent>();
            _health = GetComponent<HealthComponent>();
            _comboManager = GetComponent<ComboManager>();
            _playerInputHandler = GetComponent<PlayerInputHandler>();
            
            _context = new ActionContext(gameObject);
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDeath += HandleOwnerDeath;
                _health.OnHitReceived += HandleOwnerHitReceived;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDeath -= HandleOwnerDeath;
                _health.OnHitReceived -= HandleOwnerHitReceived;
            }

            CancelAllActions();
            _movement?.CancelGroundPush();
            _comboManager?.ResetCombo();
        }
        
        public ActionContext GetContext() => _context;

        public bool TryExecuteInterrupt(ActionBase action, object data = null)
        {
            if (action == null)
            {
                Debug.LogWarning("[ActionController] Interrupt action is null!");
                return false;
            }

            if (!action.CanExecute(_context) || IsActionOnCooldown(action))
            {
                return false;
            }

            CancelAllActions();
            return TryExecuteAction(action, data);
        }
        
        public bool TryExecuteAction(ActionBase action, object data = null)
        {
            
            if (action == null)
            {
                Debug.LogWarning("[ActionController] Action is null!");
                return false;
            }
            
            if (!action.CanExecute(_context))
            {
                Debug.LogWarning($"[ActionController] CanExecute returned false for {action.name}");
                return false;
            }
            
            if (IsActionOnCooldown(action))
            {
                Debug.LogWarning($"[ActionController] Action {action.name} is on cooldown!");
                return false;
            }
            
            var sameType = _activeActions.Find(a => 
                a.Action.GetType() == action.GetType());
            
            if (sameType != null)
            {
                if (!sameType.Action.CanBeCancelledBy(action))
                    return false;
                    
                StopAction(sameType);
            }
            
            if (action.ShouldUseStamina() && action.staminaCost > 0)
            {
                _stamina.UseStamina(action.staminaCost);
            }
            Vector2 inputDirection = _movement != null ? _movement.GetInputDirection() : Vector2.zero;

            var active = new ActiveAction(action, data, inputDirection);
            _activeActions.Add(active);

            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.enabled = false;

                if (_animator != null && action is Actions.ComboAction)
                {
                    _animator.applyRootMotion = true;
                }
            }

            action.Execute(_context);
            
            if (action.cooldown > 0f)
            {
                _cooldownEndTimes[action] = Time.time + action.cooldown;
            }
            
            if (action is DurationAction durationAction)
            {
                active.Coroutine = StartCoroutine(RunDurationActionInternal(active, durationAction));
            }
            else
            {
                _activeActions.Remove(active);
                RestoreNavMeshAgent();
            }
            
            return true;
        }
        
        private IEnumerator RunDurationActionInternal(ActiveAction active, DurationAction action)
        {
            yield return action.ExecuteOverTime(_context, active);
            action.OnCompleted(_context, active);
            _activeActions.Remove(active);

            RestoreNavMeshAgent();
        }
        
        private void StopAction(ActiveAction activeAction)
        {
            if (activeAction.Coroutine != null)
            {
                StopCoroutine(activeAction.Coroutine);
            }

            activeAction.Action.OnCancelled(_context, activeAction);
            _activeActions.Remove(activeAction);

            RestoreNavMeshAgent();
        }

        private void HandleOwnerDeath()
        {
            CancelAllActions();
            _movement?.CancelGroundPush();
            _comboManager?.ResetCombo();
        }

        private void HandleOwnerHitReceived(HitInfo hitInfo)
        {
            if (_hitReactionAction == null || !IsGroundedForHitReaction())
            {
                return;
            }

            if (TryExecuteInterrupt(_hitReactionAction, hitInfo))
            {
                _comboManager?.ResetCombo();
                _playerInputHandler?.ClearInputBuffer();
            }
        }

        private bool IsGroundedForHitReaction()
        {
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                return true;
            }

            return _movement != null && _movement.IsGrounded();
        }

        private void CancelAllActions()
        {
            for (int i = _activeActions.Count - 1; i >= 0; i--)
            {
                StopAction(_activeActions[i]);
            }
        }

        private void RestoreNavMeshAgent()
        {
            if (_activeActions.Count != 0 || _navMeshAgent == null || _navMeshAgent.enabled)
            {
                return;
            }

            if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                return;
            }

            _navMeshAgent.enabled = true;
            _navMeshAgent.Warp(hit.position);
            _navMeshAgent.isStopped = false;

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
            }
        }
        
        public bool HasTag(string tag) => _tags.Contains(tag);
        
        public void AddTag(string tag)
        {
            _tags.Add(tag);
            _currentTags = new List<string>(_tags);
        }
        
        public void RemoveTag(string tag)
        {
            _tags.Remove(tag);
            _currentTags = new List<string>(_tags);
        }
        
        public bool IsActionActive(System.Type actionType)
        {
            return _activeActions.Exists(a => a.Action.GetType() == actionType);
        }
        
        public bool IsActionOnCooldown(ActionBase action)
        {
            return _cooldownEndTimes.TryGetValue(action, out float endTime) && Time.time < endTime;
        }
        
        public float GetCooldownRemaining(ActionBase action)
        {
            if (_cooldownEndTimes.TryGetValue(action, out float endTime))
            {
                return Mathf.Max(0f, endTime - Time.time);
            }
            return 0f;
        }
        
        public float GetCooldownProgress(ActionBase action)
        {
            if (action.cooldown <= 0f) return 1f;
            
            float remaining = GetCooldownRemaining(action);
            return 1f - (remaining / action.cooldown);
        }
        
    }
}
