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
            float inputValue = context.GetFloat(_contextKey);

            float utility = _curve.Evaluate(inputValue);
            return Mathf.Clamp01(utility);
        }

        private void Reset()
        {
            _curve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f));
        }
    }
}
