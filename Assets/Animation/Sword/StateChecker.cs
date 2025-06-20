using Character;
using TMPro;
using UnityEngine;

public class StateChecker : MonoBehaviour
{
    private TextMeshProUGUI _currentState;
    private CharacterStateMachine _characterStateMachine;

    private void Start()
    {
        _currentState = GameObject.Find("TestCanv").GetComponentInChildren<TextMeshProUGUI>();
        _characterStateMachine = GameObject.Find("Player").GetComponent<PlayerController>().GetStateMachine();
    }
    // Update is called once per frame
    private void Update()
    {
        if (_characterStateMachine.GetCurrentState() is MoveState)
        {
            _currentState.text = "MoveState";
        }
        else if (_characterStateMachine.GetCurrentState() is JumpState)
        {
            _currentState.text = "JumpState";
        }
        else if (_characterStateMachine.GetCurrentState() is DashState)
        {
            _currentState.text = "DashState";
        }
        else
        {
            _currentState.text = "NULL";
        }
    }
}
