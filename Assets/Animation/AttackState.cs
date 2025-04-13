using Character;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class AttackState : CharacterState
{
    private Sword _weapon;

    private Animator _animator;
    private uint _currentSkillCombo = 0;
    private bool bIsEnemyHit = false;
    private bool bIsAttackInput = false;
    private bool bIsQInput = false;

    // parameters
    readonly int m_HashStateTime = Animator.StringToHash("stateTime");
    readonly int m_HashSword = Animator.StringToHash("sword");
    readonly int m_HashBaseAttack = Animator.StringToHash("attack");
    readonly int m_HashSkillQ = Animator.StringToHash("skillQ");

    private float stateTime;

    public AttackState(CharacterStateMachine stateMachine, Animator animator, string input) 
        : base(stateMachine)
    {
        _animator = animator;

        if (input.Equals("Attack"))
        {
            bIsAttackInput = true;
        }
        else if (input.Equals("Q"))
        {
            bIsQInput = true;
        }
    }

    public override void OnEnter()
    {
        _weapon = GameObject.Find("PlayerSword").GetComponent<Sword>();

        if (bIsAttackInput)
        {
            _animator.SetTrigger(m_HashSword);
            _animator.SetTrigger(m_HashBaseAttack);

            _weapon.SetAttackInf(true);

            bIsAttackInput = false;
        }
        else if (bIsQInput)
        {
            _animator.SetTrigger(m_HashSword);
            _animator.SetTrigger(m_HashSkillQ);

            _weapon.SetSkillQCombo(1);
            _currentSkillCombo = 1;

            _weapon.SetAttackInf(true);

            bIsQInput = false;
        }
    }

    public override void OnUpdate()
    {
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        _animator.SetFloat(m_HashStateTime, Mathf.Repeat(stateInfo.normalizedTime, 1f));
        stateTime = stateInfo.normalizedTime;

        if (bIsAttackInput)         // 기본 공격
        {
            _animator.SetTrigger(m_HashBaseAttack);

            _weapon.SetAttackInf(true);

            bIsAttackInput = false;
        }
        else if (bIsQInput)         // Q 스킬
        {
            if (stateInfo.normalizedTime >= 0.5f)
            {
                _animator.SetTrigger(m_HashSkillQ);

                _weapon.SetSkillQCombo(_currentSkillCombo);
                _weapon.Skill_Q(++_currentSkillCombo);

                bIsQInput = false;
            }
        }

        if (stateInfo.normalizedTime >= 0.95f)
        {
            _weapon.SetAttackInf(false);

            _animator.ResetTrigger(m_HashSword);
            _animator.ResetTrigger(m_HashBaseAttack);
            _animator.SetFloat(m_HashStateTime, 0.0f);

            StateMachine.SetState(new MoveState(StateMachine));
        }
    }

    public override void OnExit()
    {
        bIsEnemyHit = false;
        bIsAttackInput = false;
        bIsQInput = false;
        _currentSkillCombo = 0;

        _weapon.SetEnemyGravity(true);
    }

    public void SetEnemyHit(bool bHit)
    {
        bIsEnemyHit = bHit;
    }
    public void TryAttackInput()
    {
        if (!bIsAttackInput && stateTime >= 0.5)
        {
            Debug.LogWarning("A INPUT");
            bIsAttackInput = true;
        }
    }

    public void TryQInput()
    {
        if (!bIsQInput && stateTime >= 0.5)
        {
            Debug.LogWarning("Q INPUT");
            bIsQInput = true;
        }
    }

    public override void HandleMoveInput(Vector2 inputValue) { }
}
