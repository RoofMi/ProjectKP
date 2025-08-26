using System;
using UnityEngine;

namespace AI.ScriptableObject
{
    [CreateAssetMenu(menuName = "UtilityAI/Considerations/CurveConsideration")]
    public class CurveConsideration : Consideration
    {
        [SerializeField] private AnimationCurve _curve;
        [SerializeField] private string _contextKey;

        public override float Evaluate(AIContext context)
        {
            float inputValue = context.GetData<float>(_contextKey);

            float utility = _curve.Evaluate(inputValue);
            return Mathf.Clamp01(utility);
        }

        private void Reset()
        {
            _curve = new AnimationCurve(
                new Keyframe(0f, 1f), // When normalized distance is 0 then utility is 1
                new Keyframe(1f, 0f) // When normalized distance is 1 then utility is 0
            );
        }
    }
}
