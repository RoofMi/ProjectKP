using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeInformation : MonoBehaviour
{
    [SerializeField]
    private GameObject _description;

    [SerializeField]
    private Texture2D _image;
    [SerializeField]
    private string _information;

    [SerializeField]
    private Image _modeImage;
    [SerializeField]
    private TextMeshProUGUI _modeInformation;

    void Start()
    {
        _description.SetActive(false);
    }

    public void EnterButton()
    {
        Rect rect = new Rect(0, 0, _image.width, _image.height);
        Sprite sp = Sprite.Create(_image, rect, new Vector2(0.5f, 0.5f));

        _modeImage.sprite = sp;
        _modeInformation.text = _information;

        _description.SetActive(true); 
    }

    public void ExitButton()
    {
        _description.SetActive(false);
    }
}
