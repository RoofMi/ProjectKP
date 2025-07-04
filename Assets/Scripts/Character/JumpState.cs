using UnityEngine;

namespace Character
{
    public class JumpState : IState
    {
        private readonly CharacterStateMachine.CharacterContext _context;
        private Vector2 _moveInput;
        
        public JumpState(CharacterStateMachine.CharacterContext context)
        {
            _context = context;
        }
        
        public void OnEnter()
        {
            _context.Movement.StartJump();
            _context.Animator.SetTrigger("jumpTrigger");
            _context.Animator.SetBool("isGrounded", false);
        }
        
        public StateTransition Update()
        {
            _context.Movement.UpdateMovement(_moveInput, Time.deltaTime);
            _context.Movement.UpdateRotation(Time.deltaTime);
            
            if (_context.Movement.IsGrounded())
            {
                return new StateTransition(StateType.Move);
            }
            
            return null;
        }
        
        public StateTransition HandleInput(InputData input)
        {
            switch (input.Type)
            {
                case InputType.Movement:
                    _moveInput = input.Direction;
                    break;
            }
            
            return null;
        }
        
        public void OnExit()
        {
            // 현재는 비어있지만 인터페이스 구현을 위해 필요
        }
    }
}