using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeInf : MonoBehaviour
{
    public GameObject Desc;

    public Texture2D Image;
    public string Information;

    public Image ModeImage;
    public TextMeshProUGUI ModeInformation;

    void Start()
    {
        Desc.SetActive(false);
    }

    public void EnterButton()
    {
        Rect rect = new Rect(0, 0, Image.width, Image.height);
        Sprite sp = Sprite.Create(Image, rect, new Vector2(0.5f, 0.5f));

        ModeImage.sprite = sp;
        ModeInformation.text = Information;

        Desc.SetActive(true); 
    }

    public void ExitButton()
    {
        Desc.SetActive(false);
    }
}
