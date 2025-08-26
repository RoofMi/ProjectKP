using System.Collections.Generic;
using UnityEngine;

namespace AI.ScriptableObject
{
    [CreateAssetMenu(menuName = "UtilityAI/Considerations/MultiConsideration", fileName = "MultiConsideration")]
    public class MultiConsideration : Consideration
    {
        public enum AggregationType
        {
            Multiply,
            Average,
            Max,
            Min,
            WeightedSum
        }

        [Header("Multi-Factor Settings")]
        [SerializeField] private AggregationType _aggregationType = AggregationType.Multiply;
        [SerializeField] private List<Consideration> _considerations = new List<Consideration>();
        
        [Header("Weighted Sum Settings")]
        [Tooltip("Only used when aggregationType is WeightedSum")]
        [SerializeField] private List<float> _weights = new List<float>();

        private float[] _scoreBuffer;
        private int _validCount;

        public override float Evaluate(AIContext context)
        {
            if (_considerations == null || _considerations.Count == 0)
            {
                Debug.LogError("[MultiConsideration] no Considerations found");
                return 0f;
            }

            if (_scoreBuffer == null || _scoreBuffer.Length < _considerations.Count)
            {
                _scoreBuffer = new float[_considerations.Count];
            }

            for (int i = 0; i < _considerations.Count; i++)
            {
                if (_considerations[i] == null)
                {
                    Debug.LogError("[MultiConsideration] invalid Consideration found");
                    return 0f;
                }
                
                _scoreBuffer[i] = _considerations[i].Evaluate(context);
            }

            // TODO(human): Implement the aggregation logic here
            // Use _scoreBuffer array (contains _validCount valid scores)
            // Handle each AggregationType case and return the combined score
            // Remember to clamp the final result between 0 and 1
            // For WeightedSum, use the _weights list (handle mismatched lengths gracefully)

            float evaluation;
            switch (_aggregationType)
            {
                case AggregationType.Multiply:
                {
                    evaluation = 1f;
                    
                    for (int i = 0; i < _considerations.Count; i++)
                    {
                        evaluation *= _scoreBuffer[i];
                    }
                    
                    break;
                }

                case AggregationType.Average:
                {
                    evaluation = 0f;
                    
                    for (int i = 0; i < _considerations.Count; i++)
                    {
                        evaluation += _scoreBuffer[i];
                    }
                    
                    evaluation = evaluation / _considerations.Count;
                    break;
                }

                case AggregationType.Max:
                {
                    evaluation = 0f;

                    for (int i = 0; i < _considerations.Count; i++)
                    {
                        if (_scoreBuffer[i] > evaluation)
                            evaluation = _scoreBuffer[i];
                    }
                    
                    break;
                }

                case AggregationType.Min:
                {
                    evaluation = 2f; // Consideration : 0~1

                    for (int i = 0; i < _considerations.Count; i++)
                    {
                        if (_scoreBuffer[i] < evaluation)
                            evaluation = _scoreBuffer[i];
                    }

                    break;
                }

                case AggregationType.WeightedSum:
                {
                    evaluation = 0f;

                    for (int i = 0; i < _considerations.Count; i++)
                    {
                        float weight = (i < _weights.Count) ? _weights[i] : 1f;
                        evaluation += _scoreBuffer[i] * weight;
                    }

                    break;
                }
                
                default:
                    evaluation = 0f;
                    break;
            }

            evaluation = Mathf.Clamp(evaluation, 0f, 1f);
            return evaluation;
        }

        private void OnValidate()
        {
            // Ensure weights list matches considerations count for WeightedSum
            if (_aggregationType == AggregationType.WeightedSum)
            {
                while (_weights.Count < _considerations.Count)
                    _weights.Add(1f);
                while (_weights.Count > _considerations.Count)
                    _weights.RemoveAt(_weights.Count - 1);
            }
        }

#if UNITY_EDITOR
        // Helper method for editor debugging
        public string GetDebugInfo(AIContext context)
        {
            if (_considerations == null || _considerations.Count == 0)
                return "No considerations";

            var debugInfo = $"MultiConsideration ({_aggregationType}):\n";
            for (int i = 0; i < _considerations.Count; i++)
            {
                if (_considerations[i] != null)
                {
                    float score = _considerations[i].Evaluate(context);
                    string weight = _aggregationType == AggregationType.WeightedSum ? $" (w:{_weights[i]:F2})" : "";
                    debugInfo += $"  [{i}] {_considerations[i].name}: {score:F3}{weight}\n";
                }
            }
            debugInfo += $"  Final: {Evaluate(context):F3}";
            return debugInfo;
        }
#endif
    }
}