using System.Collections;
using System.Collections.Generic;
using Actions.Core;
using UnityEngine;

namespace Character
{
    public class ActionController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterMovement _movement;
        [SerializeField] private StaminaComponent _stamina;
        
        [Header("Debug")]
        [SerializeField] private List<ActiveAction> _activeActions = new();
        [SerializeField] private List<string> _currentTags = new() { "Grounded" };
        
        private HashSet<string> _tags;
        
        private void Awake()
        {
            _tags = new HashSet<string>(_currentTags);
            
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_movement == null) _movement = GetComponent<CharacterMovement>();
            if (_stamina == null) _stamina = GetComponent<StaminaComponent>();
        }
        
        public bool TryExecuteAction(ActionBase action, object data = null)
        {
            if (action == null || !action.CanExecute(gameObject))
                return false;
            
            var sameType = _activeActions.Find(a => 
                a.Action.GetType() == action.GetType());
            
            if (sameType != null)
            {
                if (!sameType.Action.CanBeCancelledBy(action))
                    return false;
                    
                StopAction(sameType);
            }
            
            if (action.staminaCost > 0 && !(action is Actions.ComboAction))
            {
                _stamina.UseStamina(action.staminaCost);
            }
            Vector2 inputDirection = _movement != null ? _movement.GetInputDirection() : Vector2.zero;
            
            var active = new ActiveAction(action, data, inputDirection);
            _activeActions.Add(active);
            
            action.Execute(gameObject);
            
            if (action is DurationAction durationAction)
            {
                StartCoroutine(RunDurationAction(active, durationAction));
            }
            else
            {
                _activeActions.Remove(active);
            }
            
            return true;
        }
        
        private IEnumerator RunDurationAction(ActiveAction active, DurationAction action)
        {
            active.Coroutine = StartCoroutine(RunDurationActionInternal(active, action));
            yield break;
        }
        
        private IEnumerator RunDurationActionInternal(ActiveAction active, DurationAction action)
        {
            yield return action.ExecuteOverTime(gameObject, active);
            _activeActions.Remove(active);
        }
        
        private void StopAction(ActiveAction activeAction)
        {
            if (activeAction.Coroutine != null)
            {
                StopCoroutine(activeAction.Coroutine);
            }
            _activeActions.Remove(activeAction);
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
        
    }
}