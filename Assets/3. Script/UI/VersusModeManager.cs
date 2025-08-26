using System;
using TMPro;
using UnityEngine;

public class VersusModeManager : MonoBehaviour
{
    private TextMeshProUGUI _playerScoreText;
    private TextMeshProUGUI _enemyScoreText;
    private TextMeshProUGUI _timeText;

    /**
     * 정보 저장 변수들
     * @param Score 스코어 점수. 0번이 Player, 1번이 Enemy
     * @param ***Time 대기 시간 및 게임 시간
     * @param Min, Sec 시간 관리에 필요한 변수
     */
    private float[] _score = new float[2];
    private int _timeMin;
    private int _timeSec;

    [SerializeField] private float _preparationTime = 5.0f;
    [SerializeField] private float _remainTime = 150.0f;

    void Start()
    {
        _playerScoreText = GameObject.Find("PlayerScore").GetComponent<TextMeshProUGUI>();
        _enemyScoreText = GameObject.Find("EnemyScore").GetComponent<TextMeshProUGUI>();
        _timeText = GameObject.Find("Time").GetComponent<TextMeshProUGUI>();

        if (_playerScoreText == null)
            Debug.LogWarning("PlayerScoreText is not assigned!");
        if (_enemyScoreText == null)
            Debug.LogWarning("EnemyScoreText is not assigned!");

        _playerScoreText.text = "" + (int)_score[0];
        _enemyScoreText.text = "" + (int)_score[1];
        _timeText.text = "" + (int)_preparationTime;

        // Time.deltaTime에 의해 1초가 빠르게 지나가므로 원하는 시간 설정을 위한 1초 추가
        _preparationTime += 1.0f;
        _remainTime += 1.0f;

        // 캐릭터의 상태에 따라 점수값을 추가하는 단계 (Enemy는 아직 구현되어 있지 않음)
        //PlayerState ps = FindFirstObjectByType<PlayerState>();
        //ps.PlayerDie += new EventHandler(IncreasePlayerScore);
    }

    // Update is called once per frame
    void Update()
    {
        TimeSystem();
    }
    
    private void TimeSystem()
    {
        // 준비시간 이후에 메인 시간이 흐르도록 설정
        if ((int)_preparationTime == 0)
        {
            if ((int)_remainTime == 0)
            {
                GameEnd();
            }
            else
            {
                _remainTime -= Time.deltaTime;
                _timeText.color = Color.black;

                TimeTextUpdate(_remainTime);
            }
        }
        else
        {
            _preparationTime -= Time.deltaTime;
            TimeTextUpdate(_preparationTime);
        }
    }

    private void TimeTextUpdate(float TimeValue)
    {
        _timeMin = (int)TimeValue / 60;
        _timeSec = (int)TimeValue % 60;

        _timeText.text = "" + string.Format("{0:D2}:{1:D2}", _timeMin, _timeSec);
    }

    private void ScoreTextUpdate(byte idx)
    {
        switch(idx)
        {
            case 0:
                _playerScoreText.text = "" + (int)_score[0];
                break;
            case 1:
                _enemyScoreText.text = "" + (int)_score[1];
                break;
        }
    }

    private void IncreasePlayerScore(object sender, EventArgs eventArgs)
    {
        _score[0] += 1.0f;
        ScoreTextUpdate(0);
    }

    private void IncreaseEnemyScore(object sender, EventArgs eventArgs)
    {
        _score[1] += 1.0f;
        ScoreTextUpdate(1);
    }

    private void GameEnd()
    {
        //if (_score[0] > _score[1])
        //    Debug.Log("Player Win");
        //else if (_score[0] < _score[1])
        //    Debug.Log("Enemy Win");
        //else
        //    Debug.Log("DRAW");
    }
}