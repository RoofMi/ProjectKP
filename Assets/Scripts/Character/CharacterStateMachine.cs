using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class CharacterStateMachine
    {
        private CharacterState _currentState;
        public Animator Animator { get; private set; }
        public CharacterMovement Movement { get; private set; }
        
        public event Action OnComboEnded;
        
        public CharacterStateMachine(CharacterMovement movement, Animator animator)
        {
            Movement = movement;
            Animator = animator;
            
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

        public void OnComboInput()
        {
            if (_currentState is not AttackState)
            {
                return;
            }

            _currentState.HandleComboInput();
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

        public void TryAttack()
        {
            if (_currentState is null || !_currentState.CanAttack())
            {
                return;
            }
            
            SetState(new AttackState(this));
        }

        public void HandleComboEnd()
        {
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


        //Test용도. 테스트 캔버스에 모드를 띄우기 위한거기 때문에 사용한 이후에 제거해야함.
        public CharacterState GetCurrentState()
        {
            return _currentState;
        }
    }
}
