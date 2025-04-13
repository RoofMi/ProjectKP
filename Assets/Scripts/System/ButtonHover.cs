using UnityEngine;
using UnityEngine.UI;

public class ButtonHover : MonoBehaviour
{
    private Image ButtonImage;

    public Texture2D ButtonImgDefault;
    public Texture2D ButtonImgHover;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Start()
    {
        ButtonImage = GetComponent<Image>();
    }
    public void EnterButton()
    {
        Rect rect = new Rect(0, 0, ButtonImgHover.width, ButtonImgHover.height);
        Sprite sp = Sprite.Create(ButtonImgHover, rect, new Vector2(0.5f, 0.5f));

        ButtonImage.sprite = sp;
    }

    public void ExitButton()
    {
        Rect rect = new Rect(0, 0, ButtonImgDefault.width, ButtonImgDefault.height);
        Sprite sp = Sprite.Create(ButtonImgDefault, rect, new Vector2(0.5f, 0.5f));

        ButtonImage.sprite = sp;
    }
}
