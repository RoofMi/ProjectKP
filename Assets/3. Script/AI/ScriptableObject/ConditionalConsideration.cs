using UnityEngine;

namespace AI.ScriptableObject
{
    [CreateAssetMenu(
        menuName = "UtilityAI/Considerations/ConditionalConsideration",
        fileName = "ConditionalConsideration")]
    public class ConditionalConsideration : Consideration
    {
        public enum ComparisonType
        {
            Greater,
            GreaterOrEqual,
            Less,
            LessOrEqual,
            Equal,
            NotEqual
        }

        [Header("Condition")]
        [SerializeField] private string _contextKey = "health";
        [SerializeField] private ComparisonType _comparison = ComparisonType.Less;
        [SerializeField] private float _threshold = 0.3f;

        [Header("Result")]
        [SerializeField] private bool _useDirectValues;
        [SerializeField] private float _trueValue = 1f;
        [SerializeField] private float _falseValue;
        [SerializeField] private Consideration _trueBranch;
        [SerializeField] private Consideration _falseBranch;
        [SerializeField] private float _defaultValue;

        public override float Evaluate(AIContext context)
        {
            float contextValue = string.IsNullOrEmpty(_contextKey)
                ? 0f
                : context.GetFloat(_contextKey);
            bool condition = Matches(contextValue);

            if (_useDirectValues)
            {
                return condition ? _trueValue : _falseValue;
            }

            Consideration branch = condition ? _trueBranch : _falseBranch;
            return branch != null ? branch.Evaluate(context) : _defaultValue;
        }

        private bool Matches(float value)
        {
            return _comparison switch
            {
                ComparisonType.Greater => value > _threshold,
                ComparisonType.GreaterOrEqual => value >= _threshold,
                ComparisonType.Less => value < _threshold,
                ComparisonType.LessOrEqual => value <= _threshold,
                ComparisonType.Equal => Mathf.Approximately(value, _threshold),
                ComparisonType.NotEqual => !Mathf.Approximately(value, _threshold),
                _ => false
            };
        }
    }
}
