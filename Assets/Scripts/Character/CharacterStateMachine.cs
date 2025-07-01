using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Combat;

namespace Character
{
    public class CharacterStateMachine
    {
        private CharacterState _currentState;
        public Animator Animator { get; private set; }
        public CharacterMovement Movement { get; private set; }
        public ComboManager ComboManager { get; private set; }
        public StaminaComponent Stamina { get; private set; }

        public InputBuffer InputBuffer { get; private set; }
        
        public event Action OnComboEnded;
        
        public CharacterStateMachine(CharacterMovement movement, Animator animator, InputBuffer inputBuffer, ComboManager comboManager, StaminaComponent stamina)
        {
            Movement = movement;
            Animator = animator;
            InputBuffer = inputBuffer;
            ComboManager = comboManager;
            Stamina = stamina;
            
            _currentState = new MoveState(this);
            _currentState.OnEnter();
        }

        public void OnUpdate()
        {
            _currentState?.OnUpdate();
        }
        
        public void SetState(CharacterState newState)
        {
            _currentState?.OnExit();
            _currentState = newState;
            _currentState?.OnEnter();
        }

        public void OnMoveInput(Vector2 inputValue)
        {
            _currentState.HandleMoveInput(inputValue);
        }

        public void OnComboInput(string inputKey)
        {
            if (_currentState is null || ComboManager == null)
            {
                return;
            }
            
            // AttackState로 진입하려는 경우
            if (_currentState.CanAttack())
            {
                // 입력된 키로 첫 콤보 노드 찾기
                var firstNode = ComboManager.GetFirstComboNode(inputKey);
                if (firstNode != null)
                {
                    // 스태미나 체크
                    if (Stamina.TryUseStamina(firstNode.StaminaCost))
                    {
                        SetState(new AttackState(this, ComboManager, firstNode));
                    }
                    // TryUseStamina가 false를 반환하면 OnStaminaInsufficient 이벤트가 자동 발생
                }
            }
            // 이미 AttackState인 경우 InputBuffer가 처리함
        }

        public void TryJump()
        {
            if (_currentState == null || !_currentState.CanJump())
            {
                return;
            }
            
            SetState(new JumpState(this));
        }

        public void TryDash()
        {
            if (_currentState == null || !_currentState.CanDash())
            {
                return;
            }
            
            SetState(new DashState(this));
        }

        public void HandleComboEnd()
        {
            ComboManager?.ResetCombo();
            SetState(new MoveState(this));
            SetAnimatorTrigger("goToDefaultTrigger");
            OnComboEnded?.Invoke();
        }

        public void SetAnimatorBool(string paramName, bool paramValue)
        {
            Animator.SetBool(paramName, paramValue);
        }

        public void SetAnimatorFloat(string paramName, float paramValue)
        {
            Animator.SetFloat(paramName, paramValue);
        }

        public void SetAnimatorTrigger(string paramName)
        {
            Animator.SetTrigger(paramName);
        }

        public void PlayAnimation(string stateName, float transitionDuration = 0.1f)
        {
            Animator.CrossFade(stateName, transitionDuration);
        }


        //Test용도. 테스트 캔버스에 모드를 띄우기 위한거기 때문에 사용한 이후에 제거해야함.
        public CharacterState GetCurrentState()
        {
            return _currentState;
        }
    }
}
