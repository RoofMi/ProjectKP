using System;
using TMPro;
using UnityEngine;

public class VersusModeManager : MonoBehaviour
{
    private TextMeshProUGUI PlayerScoreText;
    private TextMeshProUGUI EnemyScoreText;
    private TextMeshProUGUI TimeText;

    /**
     * 정보 저장 변수들
     * @param Score 스코어 점수. 0번이 Player, 1번이 Enemy
     * @param ***Time 대기 시간 및 게임 시간
     * @param Min, Sec 시간 관리에 필요한 변수
     */
    private float[] Score = new float[2];
    private float PreparationTime = 5.0f;
    public float RemainTime = 5.0f;
    private int Min;
    private int Sec;

    void Start()
    {
        PlayerScoreText = GameObject.Find("PlayerScore").GetComponent<TextMeshProUGUI>();
        EnemyScoreText = GameObject.Find("EnemyScore").GetComponent<TextMeshProUGUI>();
        TimeText = GameObject.Find("Time").GetComponent<TextMeshProUGUI>();

        if (PlayerScoreText == null)
            Debug.LogWarning("PlayerScoreText is not assigned!");
        if (EnemyScoreText == null)
            Debug.LogWarning("EnemyScoreText is not assigned!");

        PlayerScoreText.text = "" + (int)Score[0];
        EnemyScoreText.text = "" + (int)Score[1];
        TimeText.text = "" + (int)PreparationTime;

        // Time.deltaTime에 의해 1초가 빠르게 지나가므로 원하는 시간 설정을 위한 1초 추가
        PreparationTime += 1.0f;
        RemainTime += 1.0f;

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
        if ((int)PreparationTime == 0)
        {
            if ((int)RemainTime == 0)
            {
                GameEnd();
            }
            else
            {
                RemainTime -= Time.deltaTime;
                TimeText.color = Color.black;

                TimeTextUpdate(RemainTime);
            }
        }
        else
        {
            PreparationTime -= Time.deltaTime;
            TimeTextUpdate(PreparationTime);
        }
    }

    private void TimeTextUpdate(float TimeValue)
    {
        Min = (int)TimeValue / 60;
        Sec = (int)TimeValue % 60;

        TimeText.text = "" + string.Format("{0:D2}:{1:D2}", Min, Sec);
    }

    private void ScoreTextUpdate(byte idx)
    {
        switch(idx)
        {
            case 0:
                PlayerScoreText.text = "" + (int)Score[0];
                break;
            case 1:
                EnemyScoreText.text = "" + (int)Score[1];
                break;
        }
    }

    private void IncreasePlayerScore(object sender, EventArgs eventArgs)
    {
        Score[0] += 1.0f;
        ScoreTextUpdate(0);
    }

    private void IncreaseEnemyScore(object sender, EventArgs eventArgs)
    {
        Score[1] += 1.0f;
        ScoreTextUpdate(1);
    }

    private void GameEnd()
    {
        if (Score[0] > Score[1])
            Debug.Log("Player Win");
        else if (Score[0] < Score[1])
            Debug.Log("Enemy Win");
        else
            Debug.Log("DRAW");
    }
}