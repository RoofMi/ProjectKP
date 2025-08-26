using UnityEngine;

namespace AI.ScriptableObject
{
    [CreateAssetMenu(menuName = "UtilityAI/Considerations/ConstantConsideration")]
    public class ConstantConsideration : Consideration
    {
        [SerializeField] private float _value;

        public override float Evaluate(AIContext context) => _value;
    }
}
