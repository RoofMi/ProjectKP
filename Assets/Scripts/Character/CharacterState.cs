using UnityEngine;

namespace Character
{
    public abstract class CharacterState
    {
        protected CharacterStateMachine StateMachine;
        protected CharacterMovement Movement;

        public CharacterState(CharacterStateMachine stateMachine)
        {
            StateMachine = stateMachine;
            Movement = stateMachine.Movement;
        }

        public abstract void OnEnter();

        public abstract void OnUpdate();
        public abstract void OnExit();

        public abstract void HandleMoveInput(Vector2 inputValue);

        public virtual bool CanJump() => false;
        public virtual bool CanDash() => false;
        public virtual bool CanAttackAction() => false;
    }
}
