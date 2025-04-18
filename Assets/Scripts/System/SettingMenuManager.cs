using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingMenuManager : MonoBehaviour
{
    public GameObject Canvas_Setting;

    [SerializeField] private TextMeshProUGUI _graphicSettingButtonText;
    [SerializeField] private TextMeshProUGUI _soundSettingButtonText;

    [SerializeField] private GameObject _graphicSetting;
    [SerializeField] private GameObject _soundSetting;
    [SerializeField] private Transform _resolutionObject;
    [SerializeField] private Transform _fullScreenModeObject;
    [SerializeField] private Transform _framerateObject;
    [SerializeField] private Transform _textureQualityObject;
    [SerializeField] private Transform _shadowQualityObject;

    // Setting - Graphic
    private List<(int, int)> _resolutionList = new List<(int, int)>() { (960, 540), (1280, 720), (1366, 768), (1600, 900), (1920, 1080), (2560, 1440), (3840, 2160), (7680, 4320) };
    private List<Image> _resolutionImages = new List<Image>();
    private List<Image> _fullScreenModeImages = new List<Image>();
    private List<int> _framerateList = new List<int>() { 30, 60, 120, 144, 240 };
    private List<Image> _framerateImages = new List<Image>();
    private List<Image> _textureQualityImages = new List<Image>();
    private List<Image> _shadowQualityImages = new List<Image>();

    private int _resolutionIdx = 0;
    private int _fullScreenModeIdx = 0;               //0: 전체화면, 1: 전체 창모드, 3: 윈도우 (2는 MAC 전용)
    private int _framerateIdx = 0;
    private int _textureQualityIdx = 0;
    private int _shadowQualityIdx = 0;

    // Setting - Sound
    [SerializeField] private Slider _masterSlider;

    // Text
    private TextMeshProUGUI _resolutionText;
    private TextMeshProUGUI _fullScreenModeText;
    private TextMeshProUGUI _framerateText;
    private TextMeshProUGUI _textureQualityText;
    private TextMeshProUGUI _shadowQualityText;

    // Button
    private Button _resolutionLeftButton;
    private Button _resolutionRightButton;
    private Button _fullScreenModeLeftButton;
    private Button _fullScreenModeRightButton;
    private Button _framerateLeftButton;
    private Button _framerateRightButton;
    private Button _textureQualityLeftButton;
    private Button _textureQualityRightButton;
    private Button _shadowQualityLeftButton;
    private Button _shadowQualityRightButton;

    void Awake()
    {
        InitSettingOption(_resolutionObject, _resolutionImages, 8, out _resolutionText, out _resolutionLeftButton, out _resolutionRightButton, OnClickResolutionLeft, OnClickResolutionRight);
        InitSettingOption(_fullScreenModeObject, _fullScreenModeImages, 3, out _fullScreenModeText, out _fullScreenModeLeftButton, out _fullScreenModeRightButton, OnClickFullScreenModeLeft, OnClickFullScreenModeRight);
        InitSettingOption(_framerateObject, _framerateImages, 5, out _framerateText, out _framerateLeftButton, out _framerateRightButton, OnClickFramerateLeft, OnClickFramerateRight);
        InitSettingOption(_textureQualityObject, _textureQualityImages, 3, out _textureQualityText, out _textureQualityLeftButton, out _textureQualityRightButton, OnClickTextureQualityLeft, OnClickTextureQualityRight);
        InitSettingOption(_shadowQualityObject, _shadowQualityImages, 5, out _shadowQualityText, out _shadowQualityLeftButton, out _shadowQualityRightButton, OnClickShadowQualityLeft, OnClickShadowQualityRight);


        _masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
    }
    public void VisibleSettingMenu(bool bOffOn)
    {
        Canvas_Setting.SetActive(bOffOn);
    }

    public void VisibleGraphicSetting(bool bOffOn)
    {
        _graphicSetting.SetActive(bOffOn);

        if (bOffOn)
        {
            _graphicSettingButtonText.color = Color.cyan;
            _soundSettingButtonText.color = Color.white;
        }

        _soundSetting.SetActive(false);
    }

    public void VisibleSoundSetting(bool bOffOn)
    {
        _soundSetting.SetActive(bOffOn);

        if (bOffOn)
        {
            _graphicSettingButtonText.color = Color.white;
            _soundSettingButtonText.color = Color.cyan;
        }

        _graphicSetting.SetActive(false);
    }

    // Graphic - Resolution
    private void OnClickResolutionLeft()
    {
        if (_resolutionIdx - 1 >= 0)
        {
            _resolutionIdx--;

            _resolutionText.text = _resolutionList[_resolutionIdx].Item1 + " x " + _resolutionList[_resolutionIdx].Item2;

            _resolutionImages[_resolutionIdx].color = Color.cyan;
            _resolutionImages[_resolutionIdx + 1].color = Color.white;
        }

#if UNITY_WEBGL
        _resolutionLeftButton.interactable = false;
        _resolutionRightButton.interactable = false;
#else
        _resolutionLeftButton.interactable = _resolutionIdx > 0 ? true : false;
        _resolutionRightButton.interactable = _resolutionIdx < _resolutionList.Count - 1 ? true : false;
#endif
    }

    private void OnClickResolutionRight()
    {
        if (_resolutionIdx + 1 <= _resolutionList.Count - 1)
        {
            _resolutionIdx++;

            _resolutionText.text = _resolutionList[_resolutionIdx].Item1 + " x " + _resolutionList[_resolutionIdx].Item2;

            _resolutionImages[_resolutionIdx].color = Color.cyan;
            _resolutionImages[_resolutionIdx - 1].color = Color.white;
        }

#if UNITY_WEBGL
        _resolutionLeftButton.interactable = false;
        _resolutionRightButton.interactable = false;
#else
        _resolutionLeftButton.interactable = _resolutionIdx > 0 ? true : false;
        _resolutionRightButton.interactable = _resolutionIdx < _resolutionList.Count - 1 ? true : false;
#endif
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
                _fullScreenModeText.text = "전체 화면";
                break;
            case 1:
                _fullScreenModeText.text = "전체 창모드";
                break;
            case 3:
                _fullScreenModeText.text = "창모드";
                break;
            default:
                _fullScreenModeText.text = "Error";
                break;
        }

        
        if (_fullScreenModeIdx == 1)
        {
            _fullScreenModeImages[1].color = Color.cyan;
            _fullScreenModeImages[2].color = Color.white;
        } 
        else
        {
            _fullScreenModeImages[_fullScreenModeIdx].color = Color.cyan;
            _fullScreenModeImages[_fullScreenModeIdx + 1].color = Color.white;
        }

#if UNITY_WEBGL
        _fullScreenModeLeftButton.interactable = false;
        _fullScreenModeRightButton.interactable = false;
#else
        _fullScreenModeLeftButton.interactable = _fullScreenModeIdx != 0 ? true : false;
        _fullScreenModeRightButton.interactable = _fullScreenModeIdx != 3 ? true : false;
#endif
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
                _fullScreenModeText.text = "전체 화면";
                break;
            case 1:
                _fullScreenModeText.text = "전체 창모드";
                break;
            case 3:
                _fullScreenModeText.text = "창모드";
                break;
            default:
                _fullScreenModeText.text = "Error";
                break;
        }

        if (_fullScreenModeIdx == 3)
        {
            _fullScreenModeImages[2].color = Color.cyan;
            _fullScreenModeImages[1].color = Color.white;
        }
        else
        {
            _fullScreenModeImages[_fullScreenModeIdx].color = Color.cyan;
            _fullScreenModeImages[_fullScreenModeIdx - 1].color = Color.white;
        }

#if UNITY_WEBGL
        _fullScreenModeLeftButton.interactable = false;
        _fullScreenModeRightButton.interactable = false;
#else
        _fullScreenModeLeftButton.interactable = _fullScreenModeIdx != 0 ? true : false;
        _fullScreenModeRightButton.interactable = _fullScreenModeIdx != 3 ? true : false;
#endif
    }

    // Graphic - Framerate
    private void OnClickFramerateLeft()
    {
        if (_framerateIdx - 1 >= 0)
        {
            _framerateIdx--;

            _framerateText.text = _framerateList[_framerateIdx] + " Hz";

            _framerateImages[_framerateIdx].color = Color.cyan;
            _framerateImages[_framerateIdx + 1].color = Color.white;
        }

        _framerateLeftButton.interactable = _framerateIdx > 0 ? true : false;
        _framerateRightButton.interactable = _framerateIdx < _framerateList.Count - 1 ? true : false;
    }

    private void OnClickFramerateRight()
    {
        if (_framerateIdx + 1 <= _framerateList.Count - 1)
        {
            _framerateIdx++;

            _framerateText.text = _framerateList[_framerateIdx] + " Hz";

            _framerateImages[_framerateIdx].color = Color.cyan;
            _framerateImages[_framerateIdx - 1].color = Color.white;
        }

        _framerateLeftButton.interactable = _framerateIdx > 0 ? true : false;
        _framerateRightButton.interactable = _framerateIdx < _framerateList.Count - 1 ? true : false;
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
                    _textureQualityText.text = "높음";
                    break;
                case 1:
                    _textureQualityText.text = "중간";
                    break;
                case 2:
                    _textureQualityText.text = "낮음";
                    break;
                default:
                    _textureQualityText.text = "Error";
                    break;
            }

            _textureQualityImages[_textureQualityIdx].color = Color.cyan;
            _textureQualityImages[_textureQualityIdx + 1].color = Color.white;
        }

        _textureQualityLeftButton.interactable = _textureQualityIdx != 0 ? true : false;
        _textureQualityRightButton.interactable = _textureQualityIdx != 2 ? true : false;
    }

    private void OnClickTextureQualityRight()
    {
        if (_textureQualityIdx + 1 <= 2)
        {
            _textureQualityIdx++;

            switch (_textureQualityIdx)
            {
                case 0:
                    _textureQualityText.text = "높음";
                    break;
                case 1:
                    _textureQualityText.text = "중간";
                    break;
                case 2:
                    _textureQualityText.text = "낮음";
                    break;
                default:
                    _textureQualityText.text = "Error";
                    break;
            }

            _textureQualityImages[_textureQualityIdx].color = Color.cyan;
            _textureQualityImages[_textureQualityIdx - 1].color = Color.white;
        }

        _textureQualityLeftButton.interactable = _textureQualityIdx != 0 ? true : false;
        _textureQualityRightButton.interactable = _textureQualityIdx != 2 ? true : false;
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
                    _shadowQualityText.text = "끄기";
                    break;
                case 0:
                    _shadowQualityText.text = "낮음";
                    break;
                case 1:
                    _shadowQualityText.text = "중간";
                    break;
                case 2:
                    _shadowQualityText.text = "높음";
                    break;
                case 3:
                    _shadowQualityText.text = "매우 높음";
                    break;
                default:
                    _shadowQualityText.text = "Error";
                    break;
            }

            _shadowQualityImages[_shadowQualityIdx + 1].color = Color.cyan;
            _shadowQualityImages[_shadowQualityIdx + 2].color = Color.white;
        }

        _shadowQualityLeftButton.interactable = _shadowQualityIdx != -1 ? true : false;
        _shadowQualityRightButton.interactable = _shadowQualityIdx != 3 ? true : false;
    }

    private void OnClickShadowQualityRight()
    {
        if (_shadowQualityIdx + 1 <= 3)
        {
            _shadowQualityIdx++;

            switch (_shadowQualityIdx)
            {
                case -1:
                    _shadowQualityText.text = "끄기";
                    break;
                case 0:
                    _shadowQualityText.text = "낮음";
                    break;
                case 1:
                    _shadowQualityText.text = "중간";
                    break;
                case 2:
                    _shadowQualityText.text = "높음";
                    break;
                case 3:
                    _shadowQualityText.text = "매우 높음";
                    break;
                default:
                    _shadowQualityText.text = "Error";
                    break;
            }

            _shadowQualityImages[_shadowQualityIdx + 1].color = Color.cyan;
            _shadowQualityImages[_shadowQualityIdx].color = Color.white;
        }

        _shadowQualityLeftButton.interactable = _shadowQualityIdx != -1 ? true : false;
        _shadowQualityRightButton.interactable = _shadowQualityIdx != 3 ? true : false;
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

    public void OnClickSettingApply()
    {
        Screen.SetResolution(_resolutionList[_resolutionIdx].Item1, _resolutionList[_resolutionIdx].Item2, (FullScreenMode)_fullScreenModeIdx);
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
    }

    public void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
        Debug.Log(value);
    }
}
