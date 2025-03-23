using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class VersusModeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TextMeshProUGUI PlayerScoreText;
    private TextMeshProUGUI EnemyScoreText;
    private TextMeshProUGUI TimeText;

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
    }

    // Update is called once per frame
    void Update()
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
                TimeTextUpdate(RemainTime);
                TimeText.color = Color.black;

                Debug.Log("WAIT 1 SECOND");
                StartCoroutine(Wait());
            }
            
        }
        else
        {
            PreparationTime -= Time.deltaTime;

            TimeTextUpdate(PreparationTime);
        }
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.0f);

        bWait = true;
    }

    private void TimeTextUpdate(float TimeValue)
    {
        Min = (int)TimeValue / 60;
        Sec = (int)TimeValue % 60;

        TimeText.text = "" + string.Format("{0:D2}:{1:D2}", Min, Sec);
    }
}
