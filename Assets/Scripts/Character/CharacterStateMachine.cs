using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class CharacterStateMachine
    {
        private CharacterState _currentState;
        private Animator _animator;
        public CharacterMovement Movement { get; private set; }

        private ComboEndBehaviour _comboEndBehaviour;

        public event Action OnComboEnded;
        
        public CharacterStateMachine(CharacterMovement movement, Animator animator)
        {
            Movement = movement;
            _animator = animator;

            _comboEndBehaviour = _animator.GetBehaviour<ComboEndBehaviour>();
            
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
            
            if (_comboEndBehaviour is not null)
            {
                _comboEndBehaviour.OnComboEnded += HandleComboEnd;
            }
            
            SetState(new AttackState(this));
        }

        private void HandleComboEnd()
        {
            if (_comboEndBehaviour is not null)
            {
                _comboEndBehaviour.OnComboEnded -= HandleComboEnd;
            }
            
            SetState(new MoveState(this));
            _animator.Play("Idle/Run");
            OnComboEnded?.Invoke();
        }

        public void SetAnimatorBool(string paramName, bool paramValue)
        {
            _animator.SetBool(paramName, paramValue);
        }

        public void SetAnimatorFloat(string paramName, float paramValue)
        {
            _animator.SetFloat(paramName, paramValue);
        }

        public void SetAnimatorTrigger(string paramName)
        {
            _animator.SetTrigger(paramName);
        }


        //Test용도. 테스트 캔버스에 모드를 띄우기 위한거기 때문에 사용한 이후에 제거해야함.
        public CharacterState GetCurrentState()
        {
            return _currentState;
        }
    }
}
