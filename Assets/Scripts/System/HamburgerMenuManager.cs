using UnityEngine;

public class HamburgerMenuManager : MonoBehaviour
{
    public GameObject Canvas_Hamburger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Canvas_Hamburger.SetActive(false);
    }

    public void VisibleHamburgerMenu(bool bOffOn)
    {
        Canvas_Hamburger.SetActive(bOffOn);
    }

    public void GameExit()
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
