using UnityEngine;

public class UIMenuManager : MonoBehaviour
{
    private Animator CameraObject;

    public GameObject MainMenu;
    public GameObject SettingMenu;
    public GameObject ModeMenu;
    
    void Start()
    {
        CameraObject = GetComponent<Animator>();

        CameraObject.SetFloat("CameraPos", 0);
        MainMenu.SetActive(true);
        SettingMenu.SetActive(false);
        ModeMenu.SetActive(false);
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
        SettingMenu.SetActive(true);
    }

    public void ModePos()
    {
        CameraObject.SetFloat("CameraPos", 2);
        ModeMenu.SetActive(true);
    }
}
