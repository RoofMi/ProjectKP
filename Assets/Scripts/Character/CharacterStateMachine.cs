using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Combat;

namespace Character
{
    public class CharacterStateMachine
    {
        private IState _currentState;
        private CharacterContext _context;
        private Vector2 _currentMoveInput;
        
        public Animator Animator { get; private set; }
        public CharacterMovement Movement { get; private set; }
        public ComboManager ComboManager { get; private set; }
        public StaminaComponent Stamina { get; private set; }
        public InputBuffer InputBuffer { get; private set; }
        
        public struct CharacterContext
        {
            public Animator Animator { get; set; }
            public CharacterMovement Movement { get; set; }
            public ComboManager ComboManager { get; set; }
            public StaminaComponent Stamina { get; set; }
            public InputBuffer InputBuffer { get; set; }
        }
        
        public event Action OnComboEnded;
        
        public CharacterStateMachine(CharacterMovement movement, Animator animator, InputBuffer inputBuffer, ComboManager comboManager, StaminaComponent stamina)
        {
            Movement = movement;
            Animator = animator;
            InputBuffer = inputBuffer;
            ComboManager = comboManager;
            Stamina = stamina;
            
            _context = new CharacterContext
            {
                Animator = animator,
                Movement = movement,
                ComboManager = comboManager,
                Stamina = stamina,
                InputBuffer = inputBuffer
            };
            
            _currentState = new MoveState(_context);
            _currentState.OnEnter();
        }

        public void OnUpdate()
        {
            var moveData = new InputData(_currentMoveInput);
            var moveTransition = _currentState?.HandleInput(moveData);
            if (moveTransition != null)
            {
                ChangeState(moveTransition);
                return;
            }
            
            while (InputBuffer.HasInput())
            {
                var inputString = InputBuffer.GetNextInput();
                var inputData = CreateInputData(inputString);
                
                var transition = _currentState?.HandleInput(inputData);
                if (transition != null)
                {
                    ChangeState(transition);
                    return;
                }
            }
            
            var stateTransition = _currentState?.Update();
            if (stateTransition != null)
            {
                ChangeState(stateTransition);
            }
        }
        
        private InputData CreateInputData(string inputString)
        {
            InputType type = InputType.Attack;
            
            if (inputString == "Jump")
                type = InputType.Jump;
            else if (inputString == "Dash")
                type = InputType.Dash;
            else if (inputString == "Q" || inputString == "W" || inputString == "E" || inputString == "R")
                type = InputType.Attack;
            else if (inputString == "LightAttack" || inputString == "HeavyAttack")
                type = InputType.Attack;
                
            return new InputData(inputString, type);
        }
        
        private void ChangeState(StateTransition transition)
        {
            _currentState?.OnExit();
            
            switch (transition.NextState)
            {
                case StateType.Move:
                    _currentState = new MoveState(_context);
                    break;
                    
                case StateType.Jump:
                    _currentState = new JumpState(_context);
                    break;
                    
                case StateType.Dash:
                    _currentState = new DashState(_context, _currentMoveInput);
                    break;
                    
                case StateType.Attack:
                    var comboNode = transition.TransitionData as RuntimeComboNode;
                    _currentState = new AttackState(_context, comboNode);
                    break;
            }
            
            _currentState?.OnEnter();
        }

        public void OnMoveInput(Vector2 inputValue)
        {
            _currentMoveInput = inputValue;
        }

        public void TryJump()
        {
            InputBuffer.AddInput("Jump");
        }

        public void TryDash()
        {
            InputBuffer.AddInput("Dash");
        }

    }
}