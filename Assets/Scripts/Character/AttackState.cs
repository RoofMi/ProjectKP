using UnityEngine;
using Combat;

namespace Character
{
    public class AttackState : CharacterState
    {
        private readonly ComboManager _comboManager;
        private RuntimeComboNode _currentComboNode;
        private bool _hasCheckedComboWindow;
        
        // 캐싱된 값들
        private bool _isInPreAttack = true;
        private float _windowStart;
        private float _windowEnd;
        
        // 히트박스 관련
        private Hitbox[] weaponHitboxes;
        
        public AttackState(CharacterStateMachine stateMachine, ComboManager comboManager, RuntimeComboNode comboNode)
            : base(stateMachine)
        {
            _comboManager = comboManager;
            _currentComboNode = comboNode;
            CacheWindowValues();
        }
        
        private void CacheWindowValues()
        {
            if (_currentComboNode != null)
            {
                _windowStart = _currentComboNode.WindowStart > 0 ? _currentComboNode.WindowStart : 0.5f;
                _windowEnd = _currentComboNode.WindowEnd > 0 ? _currentComboNode.WindowEnd : 0.9f;
            }
        }
        
        private void UpdateToNextCombo(RuntimeComboNode nextNode)
        {
            _currentComboNode = nextNode;
            _hasCheckedComboWindow = false;
            CacheWindowValues();
            
            // 애니메이션 전환
            if (nextNode?.StepNode?.AnimClip != null)
            {
                string stateName = nextNode.StepNode.AnimClip.name;
                StateMachine.PlayAnimation(stateName, 0.1f);
            }
        }

        public override void OnEnter()
        {
            _hasCheckedComboWindow = false;
            
            // 무기 히트박스 찾기 (캐릭터 하위에서 Hitbox 컴포넌트 검색)
            weaponHitboxes = StateMachine.Movement.GetComponentsInChildren<Hitbox>(true);
            
            // 콤보 노드에 애니메이션이 있으면 재생
            if (_currentComboNode?.StepNode?.AnimClip != null)
            {
                string stateName = _currentComboNode.StepNode.AnimClip.name;
                StateMachine.PlayAnimation(stateName, 0.1f);
            }
            else
            {
                // 기본 공격 애니메이션 트리거
                StateMachine.SetAnimatorTrigger("qSkillStartTrigger");
            }
            
            // 히트박스 활성화 (애니메이션 이벤트로 제어하는 것이 더 정확하지만, 일단 즉시 활성화)
            EnableHitboxes();
        }

        public override void OnUpdate()
        {
            var stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            
            // PreAttack 상태 체크 (처음 한 번만)
            if (_isInPreAttack)
            {
                if (stateInfo.IsTag("PreAttack"))
                {
                    return;
                }
                _isInPreAttack = false; // PreAttack이 끝나면 다시 체크하지 않음
            }
            
            float normalizedTime = stateInfo.normalizedTime;
            
            // 콤보 추가입력 가능 구간 체크
            if (_currentComboNode != null)
            {
                if (normalizedTime >= _windowStart && normalizedTime <= _windowEnd)
                {
                    // 매 프레임 InputBuffer 체크 (연타 입력 대응)
                    string nextInput = StateMachine.InputBuffer.GetNextInput();
                    
                    if (!string.IsNullOrEmpty(nextInput))
                    {
                        var nextNode = _comboManager.TryAdvanceCombo(nextInput);
                        if (nextNode != null)
                        {
                            // 스태미나 체크 후 다음 콤보로
                            if (StateMachine.Stamina.TryUseStamina(nextNode.StaminaCost))
                            {
                                UpdateToNextCombo(nextNode);
                                return;
                            }
                            // 스태미나 부족 시 콤보 중단
                        }
                        // 잘못된 입력은 무시하고 다음 입력 확인
                    }
                }
                else if (normalizedTime > _windowEnd && !_hasCheckedComboWindow)
                {
                    // 윈도우를 놓쳤을 때 버퍼 확인
                    _hasCheckedComboWindow = true;
                    StateMachine.InputBuffer.ClearAllInputs();
                }
            }
            
            if (normalizedTime >= 1f)
            {
                StateMachine.HandleComboEnd();
            }
        }

        public override void OnExit()
        {
            DisableHitboxes();
        }

        public override void HandleMoveInput(Vector2 inputValue)
        {  
            
        }
        
        private void EnableHitboxes()
        {
            if (weaponHitboxes == null || _currentComboNode == null) return;
            
            float damage = _currentComboNode.Damage;
            GameObject attacker = StateMachine.Movement.gameObject;
            
            foreach (var hitbox in weaponHitboxes)
            {
                hitbox.EnableHitbox(damage, attacker);
            }
        }
        
        private void DisableHitboxes()
        {
            if (weaponHitboxes == null) return;
            
            foreach (var hitbox in weaponHitboxes)
            {
                hitbox.DisableHitbox();
            }
        }
    }
}
