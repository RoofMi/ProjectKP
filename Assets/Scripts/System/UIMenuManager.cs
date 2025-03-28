using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMenuManager : MonoBehaviour
{
    private Animator CameraObject;

    public GameObject Canvas_Main;
    public GameObject Canvas_Setting;
    public GameObject Canvas_Mode;

    public GameObject MainMenu;
    
    void Start()
    {
        CameraObject = GetComponent<Animator>();

        CameraObject.SetFloat("CameraPos", 0);
        MainMenu.SetActive(true);
        //SettingMenu.SetActive(false);
        //ModeMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MainPos()
    {
        CameraObject.SetFloat("CameraPos", 0);
        MainMenu.SetActive(true);
    }

    public void SettingPos()
    {
        CameraObject.SetFloat("CameraPos", 1);
        //SettingMenu.SetActive(true);
    }

    public void ModePos()
    {
        CameraObject.SetFloat("CameraPos", 2);
        //ModeMenu.SetActive(true);
    }
    public void LoadVersusScene()
    {
        LoadingUIManager.Instance.LoadScene("VersusScene");
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
}
