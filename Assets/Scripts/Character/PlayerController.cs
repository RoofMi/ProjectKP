using UnityEngine;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        private CharacterMovement _movement;
        private PlayerInputHandler _inputHandler;
        private CharacterStateMachine _stateMachine;
        private Animator _animator;

        private void Awake()
        {
            _movement = GetComponent<CharacterMovement>();
            _animator = GetComponent<Animator>();


            _stateMachine = new CharacterStateMachine(_movement, _animator);
            
            _inputHandler = GetComponent<PlayerInputHandler>();
            _inputHandler.Init(_stateMachine);
        }

        private void Update()
        {
            _stateMachine.OnUpdate();
        }

        //Test용도. 테스트 캔버스에 모드를 띄우기 위한거기 때문에 사용한 이후에 제거해야함.
        public CharacterStateMachine GetStateMachine()
        {
            return _stateMachine;
        }
    }
}
