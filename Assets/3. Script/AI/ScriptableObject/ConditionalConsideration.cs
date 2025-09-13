using UnityEngine;

namespace AI.ScriptableObject
{
    [CreateAssetMenu(menuName = "UtilityAI/Considerations/ConditionalConsideration", fileName = "ConditionalConsideration")]
    public class ConditionalConsideration : Consideration
    {
        public enum ComparisonType
        {
            Greater,        // >
            GreaterOrEqual, // >=
            Less,           // <
            LessOrEqual,    // <=
            Equal,          // ==
            NotEqual        // !=
        }

        [Header("Condition Settings")]
        [Tooltip("The context key to check for the condition")]
        [SerializeField] private string _contextKey = "health";
        
        [SerializeField] private ComparisonType _comparison = ComparisonType.Less;
        
        [Tooltip("The threshold value to compare against")]
        [SerializeField] private float _threshold = 0.3f;
        
        [Header("Value Mode")]
        [Tooltip("Use direct float values instead of Consideration branches")]
        [SerializeField] private bool _useDirectValues = false;
        
        [Header("Direct Values")]
        [Tooltip("Value to return when condition is TRUE (only used if Use Direct Values is checked)")]
        [SerializeField] private float _trueValue = 1.0f;
        
        [Tooltip("Value to return when condition is FALSE (only used if Use Direct Values is checked)")]
        [SerializeField] private float _falseValue = 0.0f;
        
        [Header("Consideration Branches")]
        [Tooltip("Consideration to evaluate when condition is TRUE (only used if Use Direct Values is unchecked)")]
        [SerializeField] private Consideration _trueBranch;
        
        [Tooltip("Consideration to evaluate when condition is FALSE (only used if Use Direct Values is unchecked)")]
        [SerializeField] private Consideration _falseBranch;
        
        [Header("Fallback")]
        [Tooltip("Default value when branches are null")]
        [SerializeField] private float _defaultValue = 0f;

        public override float Evaluate(AIContext context)
        {
            // Get context value
            float contextValue = 0f;
            if (!string.IsNullOrEmpty(_contextKey))
            {
                contextValue = context.GetData<float>(_contextKey);
            }

            // Evaluate condition
            bool condition = false;
            
            switch (_comparison)
            {
                case ComparisonType.Greater:
                {
                    condition = contextValue > _threshold;
                    break;
                }

                case ComparisonType.GreaterOrEqual:
                {
                    condition = contextValue >= _threshold;
                    break;
                }

                case ComparisonType.Less:
                {
                    condition = contextValue < _threshold;
                    break;
                }

                case ComparisonType.LessOrEqual:
                {
                    condition = contextValue <= _threshold;
                    break;
                }

                case ComparisonType.Equal:
                {
                    condition = Mathf.Approximately(contextValue, _threshold);
                    break;
                }

                case ComparisonType.NotEqual:
                {
                    condition = !Mathf.Approximately(contextValue, _threshold);
                    break;
                }

                default:
                    break;
            }
            
            // Return based on mode
            if (_useDirectValues)
            {
                return condition ? _trueValue : _falseValue;
            }
            else
            {
                // Use consideration branches
                if (_trueBranch == null && _falseBranch == null)
                {
                    Debug.LogError("[ConditionalConsideration] No branches set, using default");
                    return _defaultValue;
                }
                
                if (condition && _trueBranch != null)
                    return _trueBranch.Evaluate(context);
                else if (!condition && _falseBranch != null)
                    return _falseBranch.Evaluate(context);
                else
                    return _defaultValue;
            }
        }

#if UNITY_EDITOR
        public string GetDebugInfo(AIContext context)
        {
            float contextValue = context.GetData<float>(_contextKey);
            bool conditionMet = CheckCondition(contextValue);
            
            string info = $"ConditionalConsideration:\n";
            info += $"  Condition: {_contextKey} {_comparison} {_threshold}\n";
            info += $"  Current Value: {contextValue:F3}\n";
            info += $"  Condition Met: {conditionMet}\n";
            
            if (conditionMet && _trueBranch != null)
                info += $"  Using True Branch: {_trueBranch.name}\n";
            else if (!conditionMet && _falseBranch != null)
                info += $"  Using False Branch: {_falseBranch.name}\n";
            else
                info += $"  Using Default: {_defaultValue:F3}\n";
                
            info += $"  Result: {Evaluate(context):F3}";
            return info;
        }
        
        private bool CheckCondition(float value)
        {
            // Helper method for debug info - implement same logic as in Evaluate
            return false; // Placeholder
        }
#endif
    }
}