using System.Collections.Generic;
using Character;
using Combat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    public class AIContext
    {
        private Transform _playerCharacter;
        private Transform _currentTarget;
        private AIBrain _brain;
        private NavMeshAgent _agent;
        private ActionController _actionController;
        private ComboManager _comboManager;

        private readonly Dictionary<string, object> _data = new();

        public AIContext(AIBrain brain, Transform playerCharacter)
        {
            if (brain is null || playerCharacter is null)
            {
                return;
            }

            _brain = brain;
            _agent = brain.GetOrAddComponent<NavMeshAgent>();
            _playerCharacter = playerCharacter;
            _currentTarget = playerCharacter;
            _actionController = brain.GetOrAddComponent<ActionController>();
            _comboManager = brain.GetOrAddComponent<ComboManager>();
        }
        
        public AIBrain Brain => _brain;
        public NavMeshAgent Agent => _agent;
        public ActionController ActionController => _actionController;
        public ComboManager ComboManager => _comboManager;
        public Transform CurrentTarget => _currentTarget;

        public T GetData<T>(string key) => _data.TryGetValue(key, out var value) ? (T)value : default;
        public void SetData(string key, object value) => _data[key] = value;

        public void SetAgentDestinationToTarget() => Agent.SetDestination(_currentTarget.position);
    }
}
