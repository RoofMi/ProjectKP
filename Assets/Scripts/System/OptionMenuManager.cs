using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionMenuManager : MonoBehaviour
{
    [Header("CanvasOption")]
    public GameObject Canvas_Option;
    public TextMeshProUGUI GraphicOptionButtonText;
    public TextMeshProUGUI SoundOptionButtonText;

    [Header("OptionGameObject")]
    public GameObject GraphicOption;
    public GameObject SoundOption;
    public Transform ResolutionObject;
    public Transform FullScreenModeObject;
    public Transform FramerateObject;
    public Transform TextureQualityObject;
    public Transform ShadowQualityObject;

    [Header("Resolution")]
    private TextMeshProUGUI ResolutionText;
    private Button ResolutionLeftButton, ResolutionRightButton;
    private List<Image> ResolutionImages = new List<Image>();

    [Header("FullscreenMode")]
    private TextMeshProUGUI FullScreenModeText;
    private Button FullScreenModeLeftButton, FullScreenModeRightButton;
    private List<Image> FullScreenModeImages = new List<Image>();

    [Header("Framerate")]
    private TextMeshProUGUI FramerateText;
    private Button FramerateLeftButton, FramerateRightButton;
    private List<Image> FramerateImages = new List<Image>();

    [Header("Texture Quality")]
    private TextMeshProUGUI TextureQualityText;
    private Button TextureQualityLeftButton, TextureQualityRightButton;
    private List<Image> TextureQualityImages = new List<Image>();

    [Header("Shadow Quality")]
    private TextMeshProUGUI ShadowQualityText;
    private Button ShadowQualityLeftButton, ShadowQualityRightButton;
    private List<Image> ShadowQualityImages = new List<Image>();

    [Header("Volume")]
    public Slider MasterSlider;

    private readonly List<(int w, int h)> _resolutionList = new List<(int w, int h)>() { (960, 540), (1280, 720), (1366, 768), (1600, 900), (1920, 1080), (2560, 1440), (3840, 2160), (7680, 4320) };
    private readonly List<int> _framerateList = new List<int>() { 30, 60, 120, 144, 240 };
    private int _resolutionIdx, _fullScreenModeIdx, _framerateIdx, _textureQualityIdx, _shadowQualityIdx;

    private SaveManager _saveManager;

    void Awake()
    {
        InitSettingOption(ResolutionObject, ResolutionImages, 8, out ResolutionText, out ResolutionLeftButton, out ResolutionRightButton, OnClickResolutionLeft, OnClickResolutionRight);
        InitSettingOption(FullScreenModeObject, FullScreenModeImages, 3, out FullScreenModeText, out FullScreenModeLeftButton, out FullScreenModeRightButton, OnClickFullScreenModeLeft, OnClickFullScreenModeRight);
        InitSettingOption(FramerateObject, FramerateImages, 5, out FramerateText, out FramerateLeftButton, out FramerateRightButton, OnClickFramerateLeft, OnClickFramerateRight);
        InitSettingOption(TextureQualityObject, TextureQualityImages, 3, out TextureQualityText, out TextureQualityLeftButton, out TextureQualityRightButton, OnClickTextureQualityLeft, OnClickTextureQualityRight);
        InitSettingOption(ShadowQualityObject, ShadowQualityImages, 5, out ShadowQualityText, out ShadowQualityLeftButton, out ShadowQualityRightButton, OnClickShadowQualityLeft, OnClickShadowQualityRight);

        _saveManager = GetComponent<SaveManager>();

        if (_saveManager.LoadOptionData())
        {
            var data = _saveManager.Data;
            _resolutionIdx = data.ResolutionIdx;
            _fullScreenModeIdx = data.FullScreenModeIdx;
            _framerateIdx = data.FramerateIdx;
            _textureQualityIdx = data.TextureQualityIdx;
            _shadowQualityIdx = data.ShadowQualityIdx;
            MasterSlider.value = data.MasterVolume;

            ApplyOptions();
        }
        else
        {
            AddDefaultSettings();

            ApplyOptions();
        }

        UpdateAllUI();
    }
    public void VisibleSettingMenu(bool bOffOn)
    {
        Canvas_Option.SetActive(bOffOn);
    }

    public void VisibleGraphicSetting(bool bOffOn)
    {
        GraphicOption.SetActive(bOffOn);

        if (bOffOn)
        {
            GraphicOptionButtonText.color = Color.cyan;
            SoundOptionButtonText.color = Color.white;
        }

        SoundOption.SetActive(false);
    }

    public void VisibleSoundSetting(bool bOffOn)
    {
        SoundOption.SetActive(bOffOn);

        if (bOffOn)
        {
            GraphicOptionButtonText.color = Color.white;
            SoundOptionButtonText.color = Color.cyan;
        }

        GraphicOption.SetActive(false);
    }

    // Graphic - Resolution
    private void OnClickResolutionLeft()
    {
        if (_resolutionIdx - 1 >= 0)
        {
            _resolutionIdx--;

            ResolutionText.text = $"{_resolutionList[_resolutionIdx].w} x {_resolutionList[_resolutionIdx].h}";
        }

        UpdateButtonInteractable(ResolutionLeftButton, ResolutionRightButton, _resolutionIdx, 0, _resolutionList.Count - 1);
        UpdateOptionIndexColor(ResolutionImages, _resolutionIdx);
    }

    private void OnClickResolutionRight()
    {
        if (_resolutionIdx + 1 <= _resolutionList.Count - 1)
        {
            _resolutionIdx++;

            ResolutionText.text = $"{_resolutionList[_resolutionIdx].w} x {_resolutionList[_resolutionIdx].h}";
        }

        UpdateButtonInteractable(ResolutionLeftButton, ResolutionRightButton, _resolutionIdx, 0, _resolutionList.Count - 1);
        UpdateOptionIndexColor(ResolutionImages, _resolutionIdx);
    }

    // Graphic - FullScreen
    private void OnClickFullScreenModeLeft()
    {
        if (_fullScreenModeIdx == 1)
            _fullScreenModeIdx = 0;
        else if (_fullScreenModeIdx == 3)
            _fullScreenModeIdx = 1;

        switch (_fullScreenModeIdx)
        {
            case 0:
                FullScreenModeText.text = "전체 화면";
                break;
            case 1:
                FullScreenModeText.text = "전체 창모드";
                break;
            case 3:
                FullScreenModeText.text = "창모드";
                break;
            default:
                FullScreenModeText.text = "Error";
                break;
        }

        UpdateButtonInteractable(FullScreenModeLeftButton, FullScreenModeRightButton, _fullScreenModeIdx != 3 ? _fullScreenModeIdx : 2, 0, 2);
        UpdateOptionIndexColor(FullScreenModeImages, _fullScreenModeIdx != 3 ? _fullScreenModeIdx : 2);
    }

    private void OnClickFullScreenModeRight()
    {
        if (_fullScreenModeIdx == 0)
            _fullScreenModeIdx = 1;
        else if (_fullScreenModeIdx == 1)
            _fullScreenModeIdx = 3;

        switch (_fullScreenModeIdx)
        {
            case 0:
                FullScreenModeText.text = "전체 화면";
                break;
            case 1:
                FullScreenModeText.text = "전체 창모드";
                break;
            case 3:
                FullScreenModeText.text = "창모드";
                break;
            default:
                FullScreenModeText.text = "Error";
                break;
        }

        UpdateButtonInteractable(FullScreenModeLeftButton, FullScreenModeRightButton, _fullScreenModeIdx != 3 ? _fullScreenModeIdx : 2, 0, 2);
        UpdateOptionIndexColor(FullScreenModeImages, _fullScreenModeIdx != 3 ? _fullScreenModeIdx : 2);
    }

    // Graphic - Framerate
    private void OnClickFramerateLeft()
    {
        if (_framerateIdx - 1 >= 0)
        {
            _framerateIdx--;

            FramerateText.text = $"{_framerateList[_framerateIdx]} Hz";
        }

        UpdateButtonInteractable(FramerateLeftButton, FramerateRightButton, _framerateIdx, 0, _framerateList.Count - 1);
        UpdateOptionIndexColor(FramerateImages, _framerateIdx);
    }

    private void OnClickFramerateRight()
    {
        if (_framerateIdx + 1 <= _framerateList.Count - 1)
        {
            _framerateIdx++;

            FramerateText.text = $"{_framerateList[_framerateIdx]} Hz";
        }

        UpdateButtonInteractable(FramerateLeftButton, FramerateRightButton, _framerateIdx, 0, _framerateList.Count - 1);
        UpdateOptionIndexColor(FramerateImages, _framerateIdx);
    }

    // Graphic - TextureQuality
    private void OnClickTextureQualityLeft()
    {
        if (_textureQualityIdx - 1 >= 0)
        {
            _textureQualityIdx--;

            switch (_textureQualityIdx)
            {
                case 0:
                    TextureQualityText.text = "높음";
                    break;
                case 1:
                    TextureQualityText.text = "중간";
                    break;
                case 2:
                    TextureQualityText.text = "낮음";
                    break;
                default:
                    TextureQualityText.text = "Error";
                    break;
            }
        }

        UpdateButtonInteractable(TextureQualityLeftButton, TextureQualityRightButton, _textureQualityIdx, 0, 2);
        UpdateOptionIndexColor(TextureQualityImages, _textureQualityIdx);
    }

    private void OnClickTextureQualityRight()
    {
        if (_textureQualityIdx + 1 <= 2)
        {
            _textureQualityIdx++;

            switch (_textureQualityIdx)
            {
                case 0:
                    TextureQualityText.text = "높음";
                    break;
                case 1:
                    TextureQualityText.text = "중간";
                    break;
                case 2:
                    TextureQualityText.text = "낮음";
                    break;
                default:
                    TextureQualityText.text = "Error";
                    break;
            }
        }

        UpdateButtonInteractable(TextureQualityLeftButton, TextureQualityRightButton, _textureQualityIdx, 0, 2);
        UpdateOptionIndexColor(TextureQualityImages, _textureQualityIdx);
    }

    // Graphic - ShadowQuality
    private void OnClickShadowQualityLeft()
    {
        if (_shadowQualityIdx - 1 >= -1)
        {
            _shadowQualityIdx--;

            switch (_shadowQualityIdx)
            {
                case -1:
                    ShadowQualityText.text = "끄기";
                    break;
                case 0:
                    ShadowQualityText.text = "낮음";
                    break;
                case 1:
                    ShadowQualityText.text = "중간";
                    break;
                case 2:
                    ShadowQualityText.text = "높음";
                    break;
                case 3:
                    ShadowQualityText.text = "매우 높음";
                    break;
                default:
                    ShadowQualityText.text = "Error";
                    break;
            }
        }

        UpdateButtonInteractable(ShadowQualityLeftButton, ShadowQualityRightButton, _shadowQualityIdx + 1, 0, 4);
        UpdateOptionIndexColor(ShadowQualityImages, _shadowQualityIdx + 1);
    }

    private void OnClickShadowQualityRight()
    {
        if (_shadowQualityIdx + 1 <= 3)
        {
            _shadowQualityIdx++;

            switch (_shadowQualityIdx)
            {
                case -1:
                    ShadowQualityText.text = "끄기";
                    break;
                case 0:
                    ShadowQualityText.text = "낮음";
                    break;
                case 1:
                    ShadowQualityText.text = "중간";
                    break;
                case 2:
                    ShadowQualityText.text = "높음";
                    break;
                case 3:
                    ShadowQualityText.text = "매우 높음";
                    break;
                default:
                    ShadowQualityText.text = "Error";
                    break;
            }
        }

        UpdateButtonInteractable(ShadowQualityLeftButton, ShadowQualityRightButton, _shadowQualityIdx + 1, 0, 4);
        UpdateOptionIndexColor(ShadowQualityImages, _shadowQualityIdx + 1);
    }


    private void InitSettingOption(Transform option, List<Image> optionImageList, int optionCount, out TextMeshProUGUI valueText, out Button LeftBtn, out Button RightBtn, UnityAction OnClickLeftListener, UnityAction OnClickRightListener)
    {
        for (int i = 0; i < optionCount; i++)
        {
            var img = option.Find("Prt_Value/Lst_Img/Img_Idx" + i).GetComponent<Image>();

            optionImageList.Add(img);
        }

        valueText = option.Find("Prt_Value/Tmp_Value").GetComponent<TextMeshProUGUI>();
        LeftBtn = option.Find("Btn_Left").GetComponent<Button>();
        RightBtn = option.Find("Btn_Right").GetComponent<Button>();

        LeftBtn.onClick.AddListener(OnClickLeftListener);
        RightBtn.onClick.AddListener(OnClickRightListener);
    }

    private void AddDefaultSettings()
    {
        foreach (var img in ResolutionImages) 
            img.color = Color.white;
        foreach (var img in FullScreenModeImages) 
            img.color = Color.white;
        foreach (var img in FramerateImages) 
            img.color = Color.white;
        foreach (var img in TextureQualityImages) 
            img.color = Color.white;
        foreach (var img in ShadowQualityImages)
            img.color = Color.white;

        // 해상도
        var currentResolution = Screen.currentResolution;
        _resolutionIdx = _resolutionList.FindIndex(r => r.w == currentResolution.width && r.h == currentResolution.height);
        if (_resolutionIdx == -1)
            _resolutionIdx = 0;

        Screen.SetResolution(_resolutionList[_resolutionIdx].w, _resolutionList[_resolutionIdx].h, 0);

        // 전체화면
        _fullScreenModeIdx = 0;

        // 프레임레이트
        var refreshRate = Screen.currentResolution.refreshRateRatio;
        float frameRate = refreshRate.numerator / (float)refreshRate.denominator;
        float minDelta = float.MaxValue;
        _framerateIdx = 2;
        for (int i = 0; i < _framerateList.Count; i++)
        {
            float delta = Mathf.Abs(_framerateList[i] - frameRate);
            if (delta < minDelta)
            {
                minDelta = delta;
                _framerateIdx = i;
            }
        }

        // 텍스처 품질
        if (SystemInfo.graphicsMemorySize <= 2048)
            _textureQualityIdx = 2;
        else if (SystemInfo.graphicsMemorySize <= 4096)
            _textureQualityIdx = 1;
        else
            _textureQualityIdx = 0;

        // 그림자 품질
        if (SystemInfo.graphicsMemorySize <= 2048)
            _shadowQualityIdx = -1;
        else if (SystemInfo.graphicsMemorySize <= 4096)
            _shadowQualityIdx = 0;
        else if (SystemInfo.graphicsMemorySize <= 6144)
            _shadowQualityIdx = 1;
        else if (SystemInfo.graphicsMemorySize <= 8192)
            _shadowQualityIdx = 2;
        else
            _shadowQualityIdx = 3;

        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        // 텍스트 셋팅
        ResolutionText.text = $"{_resolutionList[_resolutionIdx].w} x {_resolutionList[_resolutionIdx].h}";
        switch (_fullScreenModeIdx)
        {
            case 0:
                FullScreenModeText.text = "전체 화면";
                break;
            case 1:
                FullScreenModeText.text = "전체 창모드";
                break;
            case 3:
                FullScreenModeText.text = "창모드";
                break;
            default:
                FullScreenModeText.text = "Error";
                break;
        }
        FramerateText.text = $"{_framerateList[_framerateIdx]} Hz";
        switch (_textureQualityIdx)
        {
            case 0:
                TextureQualityText.text = "높음";
                break;
            case 1:
                TextureQualityText.text = "중간";
                break;
            case 2:
                TextureQualityText.text = "낮음";
                break;
            default:
                TextureQualityText.text = "Error";
                break;
        }
        switch (_shadowQualityIdx)
        {
            case -1:
                ShadowQualityText.text = "끄기";
                break;
            case 0:
                ShadowQualityText.text = "낮음";
                break;
            case 1:
                ShadowQualityText.text = "중간";
                break;
            case 2:
                ShadowQualityText.text = "높음";
                break;
            case 3:
                ShadowQualityText.text = "매우 높음";
                break;
            default:
                ShadowQualityText.text = "Error";
                break;
        }

        // 선택 값 색 셋팅
        UpdateOptionIndexColor(ResolutionImages, _resolutionIdx);
        UpdateOptionIndexColor(FullScreenModeImages, _fullScreenModeIdx);
        UpdateOptionIndexColor(FramerateImages, _framerateIdx);
        UpdateOptionIndexColor(TextureQualityImages, _textureQualityIdx);
        UpdateOptionIndexColor(ShadowQualityImages, _shadowQualityIdx + 1);
    }

    private void UpdateButtonInteractable(Button left, Button right, int idx, int min, int max)
    {
#if UNITY_WEBGL
        left.interactable = false;
        right.interactable = false;
#else
        left.interactable = idx > min;
        right.interactable = idx < max;
#endif
    }

    private void UpdateOptionIndexColor(List<Image> images, int idx)
    {
        for (int i = 0; i < images.Count; i++)
            images[i].color = (i == idx) ? Color.cyan : Color.white;
    }

    public void ApplyOptions()
    {
        Screen.SetResolution(_resolutionList[_resolutionIdx].w, _resolutionList[_resolutionIdx].h, (FullScreenMode)_fullScreenModeIdx);
        Application.targetFrameRate = _framerateList[_framerateIdx];
        QualitySettings.globalTextureMipmapLimit = _textureQualityIdx;

        if (_shadowQualityIdx == -1)
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
        }
        else
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)_shadowQualityIdx;
        }

        AudioListener.volume = MasterSlider.value;

        SaveOption();
    }

    private void SaveOption()
    {
        _saveManager.Data.ResolutionIdx = _resolutionIdx;
        _saveManager.Data.FullScreenModeIdx = _fullScreenModeIdx;
        _saveManager.Data.FramerateIdx = _framerateIdx;
        _saveManager.Data.TextureQualityIdx = _textureQualityIdx;
        _saveManager.Data.ShadowQualityIdx = _shadowQualityIdx;
        _saveManager.Data.MasterVolume = MasterSlider.value;

        _saveManager.SaveOptionData();
    }
}
