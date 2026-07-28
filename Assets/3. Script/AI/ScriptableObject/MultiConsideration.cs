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

        [SerializeField] private AggregationType _aggregationType = AggregationType.Multiply;
        [SerializeField] private List<Consideration> _considerations = new();
        [SerializeField] private List<float> _weights = new();

        public override float Evaluate(AIContext context)
        {
            if (_considerations == null || _considerations.Count == 0)
            {
                return 0f;
            }

            float product = 1f;
            float sum = 0f;
            float maximum = 0f;
            float minimum = 1f;
            float weightedSum = 0f;
            float totalWeight = 0f;
            int validCount = 0;

            for (int i = 0; i < _considerations.Count; i++)
            {
                Consideration consideration = _considerations[i];
                if (consideration == null)
                {
                    continue;
                }

                float score = consideration.Evaluate(context);
                float weight = _weights != null && i < _weights.Count ? _weights[i] : 1f;

                product *= score;
                sum += score;
                maximum = Mathf.Max(maximum, score);
                minimum = Mathf.Min(minimum, score);
                weightedSum += score * weight;
                totalWeight += weight;
                validCount++;
            }

            if (validCount == 0)
            {
                return 0f;
            }

            float result = _aggregationType switch
            {
                AggregationType.Multiply => product,
                AggregationType.Average => sum / validCount,
                AggregationType.Max => maximum,
                AggregationType.Min => minimum,
                AggregationType.WeightedSum =>
                    totalWeight > 0f ? weightedSum / totalWeight : 0f,
                _ => 0f
            };

            return Mathf.Clamp01(result);
        }

        private void OnValidate()
        {
            if (_aggregationType != AggregationType.WeightedSum ||
                _considerations == null)
            {
                return;
            }

            _weights ??= new List<float>();

            while (_weights.Count < _considerations.Count)
            {
                _weights.Add(1f);
            }

            while (_weights.Count > _considerations.Count)
            {
                _weights.RemoveAt(_weights.Count - 1);
            }
        }
    }
}
