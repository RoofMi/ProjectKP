using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private Animator _cameraObjectAnimator;
    [SerializeField]
    private GameObject _canvas_Main;
    [SerializeField]
    private GameObject _canvas_Setting;
    [SerializeField]
    private GameObject _canvas_Mode;
    
    private void Start()
    {
        _cameraObjectAnimator.SetFloat("CameraPos", 0);
    }

    public void SetCameraPosition(int num)
    {
        _cameraObjectAnimator.SetFloat("CameraPos", num);
    }

    public void LoadCombatDemo()
    {
        GameActions.onSceneLoadRequest?.Invoke("CombatDemo");
    }

    public void LoadVersusScene()
    {
        LoadCombatDemo();
    }

    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
