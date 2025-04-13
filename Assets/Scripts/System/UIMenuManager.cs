using TMPro;
using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
    // Camera
    private Animator CameraObject;

    readonly int m_HashPlayPos = Animator.StringToHash("playPos");
    readonly int m_HashSettingPos = Animator.StringToHash("settingPos");
    readonly int m_HashEmptyPos = Animator.StringToHash("emptyPos");

    // Bar
    public GameObject Canvas_Bar;
    public TextMeshProUGUI Text_Play;
    public TextMeshProUGUI Text_Empty;
    public TextMeshProUGUI Text_Setting;

    // Play
    public GameObject Canvas_Play;

    // Setting
    public GameObject Canvas_Setting;

    private TextMeshProUGUI CurrentPos;

    void Start()
    {
        CameraObject = GetComponent<Animator>();

        Canvas_Bar.SetActive(true);
        Canvas_Play.SetActive(true);
        Canvas_Setting.SetActive(false);

        CurrentPos = Text_Play;
        CurrentPos.color = Color.cyan;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayPos()
    {
        CameraObject.SetTrigger(m_HashPlayPos);

        SetCurrentPos(Text_Play);

        Canvas_Play.SetActive(true);
        Canvas_Setting.SetActive(false);
    }

    public void SettingPos()
    {
        CameraObject.SetTrigger(m_HashSettingPos);

        SetCurrentPos(Text_Setting);

        Canvas_Play.SetActive(false);
        Canvas_Setting.SetActive(true);
    }

    public void EmptyPos()
    {
        CameraObject.SetTrigger(m_HashEmptyPos);

        SetCurrentPos(Text_Empty);

        Canvas_Play.SetActive(false);
        Canvas_Setting.SetActive(false);
    }

    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_EDITOR
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#else
        Application.Quit();
#endif
    }

    private void SetCurrentPos(TextMeshProUGUI current)
    {
        CurrentPos.color = Color.white;
        CurrentPos = current;
        CurrentPos.color = Color.cyan;
    }
}
