using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public class RuntimeComboNode
    {
        // ComboStepReference를 직접 참조하여 메모리 중복 제거
        public ComboStepReference StepReference { get; private set; }
        
        // 빠른 접근을 위한 프로퍼티
        public ComboNode StepNode => StepReference?.ComboNode;
        public float DamageMultiplier => StepReference?.DamageMultiplier ?? 1f;
        public bool ApplyKnockback => StepReference?.ApplyKnockback ?? false;
        public float KnockbackHorizontalForce => StepReference?.KnockbackHorizontalForce ?? 0f;
        public float KnockbackVerticalForce => StepReference?.KnockbackVerticalForce ?? 0f;
        public float WindowStart => StepReference?.WindowStart ?? 0.5f;
        public float WindowEnd => StepReference?.WindowEnd ?? 0.9f;
        public float StaminaCost => StepReference?.StaminaCost ?? 0f;
        public string InputKey => StepReference?.InputKey ?? "";
        
        // Lazy initialization으로 메모리 절약 (leaf 노드는 children이 없음)
        private Dictionary<string, List<RuntimeComboNode>> _children;
        public Dictionary<string, List<RuntimeComboNode>> Children
        {
            get
            {
                if (_children == null)
                    _children = new Dictionary<string, List<RuntimeComboNode>>();
                return _children;
            }
        }
        
        public bool IsLeaf => _children == null || _children.Count == 0;
        
        public RuntimeComboNode(ComboStepReference stepRef)
        {
            StepReference = stepRef;
        }
        
        // 파라미터 없는 생성자 (루트 노드용)
        public RuntimeComboNode()
        {
            StepReference = null;
        }
    }
}
