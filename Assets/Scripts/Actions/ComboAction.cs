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
        
        public override IEnumerator ExecuteOverTime(GameObject owner, ActiveAction activeAction)
        {
            var comboNode = activeAction.Data as RuntimeComboNode;
            if (comboNode == null || comboNode.StepNode?.AnimClip == null)
            {
                yield break;
            }
            
            var actionController = owner.GetComponent<ActionController>();
            var comboManager = owner.GetComponent<ComboManager>();
            var animator = owner.GetComponent<Animator>();
            
            if (actionController == null || comboManager == null || animator == null)
            {
                yield break;
            }
            
            actionController.AddTag(ActionTags.Attacking);
            _isInComboWindow = false;
            var movement = owner.GetComponent<CharacterMovement>();
            if (movement != null && activeAction.InputDirection.sqrMagnitude > 0.01f)
            {
                Vector3 attackDirection = new Vector3(activeAction.InputDirection.x, 0f, activeAction.InputDirection.y);
                attackDirection = movement.CameraTransform.TransformDirection(attackDirection);
                attackDirection.y = 0f;
                movement.SetRotationToDirection(attackDirection);
            }
            string animationName = comboNode.StepNode.AnimClip.name;
            animator.CrossFade(animationName, crossFadeDuration);
            yield return new WaitForSeconds(crossFadeDuration);
            bool comboWindowActive = false;
            float elapsedTime = 0f;
            
            while (true)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
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
                        break;
                    }
                }
                
                float normalizedTime = stateInfo.normalizedTime;
                bool inWindow = normalizedTime >= comboNode.WindowStart && normalizedTime < comboNode.WindowEnd;
                
                if (inWindow && !comboWindowActive)
                {
                    comboWindowActive = true;
                    _isInComboWindow = true;
                    comboManager.SetComboWindow(true);
                }
                else if (!inWindow && comboWindowActive)
                {
                    comboWindowActive = false;
                    _isInComboWindow = false;
                    comboManager.SetComboWindow(false);
                }
                if (normalizedTime >= animationEndThreshold)
                {
                    break;
                }
                
                yield return null;
            }
            actionController.RemoveTag(ActionTags.Attacking);
            comboManager.SetComboWindow(false);
            animator.CrossFade(AnimationStates.IdleRun, crossFadeDuration);
        }
        
        public override bool CanExecute(GameObject owner)
        {
            return CheckTags(owner);
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