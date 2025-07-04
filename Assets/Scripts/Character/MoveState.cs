using UnityEngine;
using Combat;

namespace Character
{
    public class MoveState : IState
    {
        private readonly CharacterStateMachine.CharacterContext _context;
        private Vector2 _moveInput;
        
        public MoveState(CharacterStateMachine.CharacterContext context)
        {
            _context = context;
        }
        
        public void OnEnter()
        {
            _context.Animator.SetBool("isGrounded", true);
            _context.Animator.CrossFade("Idle/Run", 0.1f);
        }
        
        public StateTransition Update()
        {
            _context.Movement.UpdateMovement(_moveInput, Time.deltaTime);
            _context.Movement.UpdateRotation(Time.deltaTime);
            
            float normalizedSpeed = _moveInput.magnitude;
            _context.Animator.SetFloat("speed", normalizedSpeed);
            
            return null;
        }
        
        public StateTransition HandleInput(InputData input)
        {
            StateTransition transition = null;
            
            switch (input.Type)
            {
                case InputType.Movement:
                    _moveInput = input.Direction;
                    break;
                    
                case InputType.Attack:
                    var firstNode = _context.ComboManager.GetFirstComboNode(input.Key);
                    if (firstNode != null && _context.Stamina.TryUseStamina(firstNode.StaminaCost))
                    {
                        transition = new StateTransition(StateType.Attack, firstNode);
                    }
                    break;
                    
                case InputType.Jump:
                    transition = new StateTransition(StateType.Jump);
                    break;
                    
                case InputType.Dash:
                    if (_moveInput.magnitude >= 0.1f)
                    {
                        transition = new StateTransition(StateType.Dash);
                    }
                    break;
            }
            
            return transition;
        }
        
        public void OnExit()
        {
            // 현재는 비어있지만 인터페이스 구현을 위해 필요
        }
    }
}