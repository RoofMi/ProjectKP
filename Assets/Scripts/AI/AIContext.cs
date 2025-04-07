using System.Collections.Generic;
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
        public NavMeshAgent Agent;

        private readonly Dictionary<string, object> _data = new();

        public AIContext(AIBrain brain, Transform playerCharacter)
        {
            if (brain is null || playerCharacter is null)
            {
                return;
            }

            this._brain = brain;
            this.Agent = brain.gameObject.GetOrAddComponent<NavMeshAgent>();
            this._playerCharacter = playerCharacter;
            this._currentTarget = playerCharacter;
        }

        public T GetData<T>(string key) => _data.TryGetValue(key, out var value) ? (T)value : default;
        public void SetData(string key, object value) => _data[key] = value;

        public void SetAgentDestinationToTarget() => Agent.SetDestination(_currentTarget.position);
    }
}
