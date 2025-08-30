using System.Collections;
using UnityEngine;
using Actions.Core;
using Character;
using Character.Core;
using Combat;

namespace Actions
{
    [CreateAssetMenu(fileName = "ComboAction", menuName = "Actions/Combat/Combo")]
    public class ComboAction : DurationAction
    {
        [Header("Combo Settings")]
        [SerializeField] private float crossFadeDuration = 0.1f;
        [SerializeField] private float animationEndThreshold = 0.95f;
        
        private bool _isInComboWindow;
        
        public override IEnumerator ExecuteOverTime(ActionContext context, ActiveAction activeAction)
        {
            
            var comboNode = activeAction.Data as RuntimeComboNode;
            if (comboNode == null || comboNode.StepNode?.AnimClip == null)
            {
                Debug.LogError($"[ComboAction] Invalid combo node! Node: {comboNode != null}, StepNode: {comboNode?.StepNode != null}, AnimClip: {comboNode?.StepNode?.AnimClip != null}");
                yield break;
            }
            
            if (!context.HasAllComponents())
            {
                Debug.LogError("[ComboAction] Missing required components!");
                yield break;
            }
            
            context.ActionController.AddTag(ActionTags.Attacking);
            _isInComboWindow = false;
            var movement = context.Movement;
            
            if (movement != null && activeAction.InputDirection.sqrMagnitude > 0.01f)
            {
                Vector3 attackDirection = new Vector3(activeAction.InputDirection.x, 0f, activeAction.InputDirection.y);
                
                if (movement.CameraTransform != null)
                {
                    attackDirection = movement.CameraTransform.TransformDirection(attackDirection);
                }
                else
                {
                    Debug.LogWarning("[ComboAction] CameraTransform is null! Using world direction.");
                }
                
                attackDirection.y = 0f;
                movement.SetRotationToDirection(attackDirection);
            }
            else
            {
            }
            string animationName = comboNode.StepNode.AnimClip.name;
            
            // Animator 상태 확인
            
            context.Animator.CrossFadeInFixedTime(animationName, crossFadeDuration);
            
            yield return new WaitForSeconds(crossFadeDuration);
            bool comboWindowActive = false;
            float elapsedTime = 0f;
            
            while (true)
            {
                AnimatorStateInfo stateInfo = context.Animator.GetCurrentAnimatorStateInfo(0);
                
                // 현재 상태 이름을 가져오는 디버그 코드
                int currentStateHash = stateInfo.fullPathHash;
                string currentStateName = "Unknown";
                foreach (var clipInfo in context.Animator.GetCurrentAnimatorClipInfo(0))
                {
                    if (clipInfo.clip != null)
                    {
                        currentStateName = clipInfo.clip.name;
                        break;
                    }
                }
                
                if (!stateInfo.IsName(animationName))
                {
                    if (elapsedTime < crossFadeDuration + 0.1f)
                    {
                        elapsedTime += Time.deltaTime;
                        yield return null;
                        continue;
                    }
                    else
                    {
                        Debug.LogError($"[ComboAction] Animation never started! Expected: {animationName}, Current: {currentStateName}");
                        break;
                    }
                }
                
                float normalizedTime = stateInfo.normalizedTime;
                bool inWindow = normalizedTime >= comboNode.WindowStart && normalizedTime < comboNode.WindowEnd;
                
                if (inWindow && !comboWindowActive)
                {
                    comboWindowActive = true;
                    _isInComboWindow = true;
                    context.ComboManager.SetComboWindow(true);
                }
                else if (!inWindow && comboWindowActive)
                {
                    comboWindowActive = false;
                    _isInComboWindow = false;
                    context.ComboManager.SetComboWindow(false);
                }
                if (normalizedTime >= animationEndThreshold)
                {
                    break;
                }
                
                yield return null;
            }
            context.ActionController.RemoveTag(ActionTags.Attacking);
            context.ComboManager.SetComboWindow(false);
            context.Animator.CrossFadeInFixedTime(AnimationStates.IdleRun, crossFadeDuration);
        }
        
        public override bool CanExecute(ActionContext context)
        {
            return CheckTags(context);
        }
        
        public override bool CanBeCancelledBy(ActionBase other)
        {
            if (other is ComboAction)
            {
                return _isInComboWindow;
            }
            return base.CanBeCancelledBy(other);
        }
        
        public override bool ShouldUseStamina()
        {
            return false;
        }
    }
}