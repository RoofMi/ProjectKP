using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaySelector : MonoBehaviour
{
    public Button LeftButton;
    public Button PlayButton;
    public Button RightButton;
    public TextMeshProUGUI PlayText;
    private string[] _modes = { "1 vs 1", "2 vs 2", "훈련장" };

    private int currentIndex = 0;

    void Start()
    {
        PlayText = PlayButton.GetComponentInChildren<TextMeshProUGUI>();

        UpdatePlay();

        LeftButton.onClick.AddListener(PreviousPlay);
        PlayButton.onClick.AddListener(OnClickedPlay);
        RightButton.onClick.AddListener(NextPlay);
    }

    void PreviousPlay()
    {
        currentIndex = (currentIndex - 1 + _modes.Length) % _modes.Length;
        UpdatePlay();
    }

    void OnClickedPlay()
    {
        switch(currentIndex)
        {
            case 0:
                LoadingUIManager.Instance.LoadScene("VersusScene");
                break;
        }
    }

    void NextPlay()
    {
        currentIndex = (currentIndex + 1) % _modes.Length;
        UpdatePlay();
    }

    void UpdatePlay()
    {
        PlayText.text = _modes[currentIndex];
    }

    void PlayButtonEnter()
    {

    }
}
