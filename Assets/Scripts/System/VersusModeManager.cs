using System.Collections;
using TMPro;
using UnityEngine;

public class VersusModeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TextMeshProUGUI PlayerScoreText;
    private TextMeshProUGUI EnemyScoreText;
    private TextMeshProUGUI PreparationTimeText;
    private TextMeshProUGUI RemainTimeText;

    private float PlayerScore = 0;
    private float EnemyScore = 0;

    private float PreparationTime = 5.0f;
    private float RemainTime = 5.0f;
    private bool bWait = false;

    void Start()
    {
        PlayerScoreText = GameObject.Find("PlayerScore").GetComponent<TextMeshProUGUI>();
        EnemyScoreText = GameObject.Find("EnemyScore").GetComponent<TextMeshProUGUI>();
        PreparationTimeText = GameObject.Find("PreparationTime").GetComponent<TextMeshProUGUI>();
        RemainTimeText = GameObject.Find("RemainTime").GetComponent<TextMeshProUGUI>();

        if (PlayerScoreText == null)
            Debug.LogWarning("PlayerScoreText is not assigned!");
        if (EnemyScoreText == null)
            Debug.LogWarning("EnemyScoreText is not assigned!");
        if (PreparationTimeText == null)
            Debug.LogWarning("PreparationTimeText is not assigned!");
        if (RemainTimeText == null)
            Debug.LogWarning("RemainTimeText is not assigned!");

        PlayerScoreText.text = "" + (int)PlayerScore;
        EnemyScoreText.text = "" + (int)EnemyScore;
        PreparationTimeText.text = "" + (int)PreparationTime;
        RemainTimeText.text = "" + (int)RemainTime;
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
                    RemainTimeText.text = "" + (int)RemainTime;
                }
            }
            else
            {
                Debug.Log("WAIT 1 SECOND");
                StartCoroutine(Wait());
            }
            
        }
        else
        {
            PreparationTime -= Time.deltaTime;
            PreparationTimeText.text = "" + (int)PreparationTime;
        }
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.0f);

        bWait = true;
    }
}
