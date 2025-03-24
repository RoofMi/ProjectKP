using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class VersusModeManager : MonoBehaviour
{
    private TextMeshProUGUI PlayerScoreText;
    private TextMeshProUGUI EnemyScoreText;
    private TextMeshProUGUI TimeText;

    /**
     * 정보 저장 변수들
     * @param ***Score 스코어 점수
     * @param ***Time 대기 시간 및 게임 시간
     * @param bWait, Min, Sec 시간 관리에 필요한 변수
     */
    private float PlayerScore = 0;
    private float EnemyScore = 0;
    private float PreparationTime = 5.0f;
    private float RemainTime = 80.0f;
    private bool bWait = false;
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

        PlayerScoreText.text = "" + (int)PlayerScore;
        EnemyScoreText.text = "" + (int)EnemyScore;
        TimeText.text = "" + (int)PreparationTime;

        // Time.deltaTime에 의해 1초가 빠르게 지나가므로 원하는 시간 설정을 위한 1초 추가
        PreparationTime += 1.0f;
        RemainTime += 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        TimeSystem();

        //ScoreSystem();
    }
    
    private void TimeSystem()
    {
        // 준비시간 이후에 메인 시간이 흐르도록 설정
        if ((int)PreparationTime == 0)
        {
            if (bWait)
            {
                if ((int)RemainTime == 0)
                {
                    Debug.Log("GAME END");
                }
                else
                {
                    RemainTime -= Time.deltaTime;
                    TimeTextUpdate(RemainTime);
                }
            }
            else
            {
                //RemainTime++;
                RemainTime -= Time.deltaTime;
                TimeTextUpdate(RemainTime);
                TimeText.color = Color.black;
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

    private void ScoreSystem()
    {

    }
}