using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string _saveDataFileName = "Savefile.json";
    private SaveData _saveData;

    public SaveData Data
    {
        get
        {
            return _saveData;
        }
        set
        {
            _saveData.ResolutionIdx = value.ResolutionIdx;
            _saveData.FullScreenModeIdx = value.FullScreenModeIdx;
            _saveData.FramerateIdx = value.FramerateIdx;
            _saveData.TextureQualityIdx = value.TextureQualityIdx;
            _saveData.ShadowQualityIdx = value.ShadowQualityIdx;
            _saveData.MasterVolume = value.MasterVolume;
        }
    }

    public bool LoadOptionData()
    {
        string filePath = Application.persistentDataPath + _saveDataFileName;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            _saveData = JsonUtility.FromJson<SaveData>(json);

            return true;
        }
        else
        {
            Debug.Log("옵션 파일이 없어서 초기화합니다.");
            ResetOptionData();

            return false;
        }
    }
    public void ResetOptionData()
    {
        Debug.Log("새로운 옵션 파일 생성");
        _saveData = new SaveData();

        SaveOptionData();
    }

    // 옵션 데이터 저장하기
    public void SaveOptionData()
    {
        string json = JsonUtility.ToJson(_saveData, true);
        string filePath = Application.persistentDataPath + _saveDataFileName;
        File.WriteAllText(filePath, json);
        Debug.Log("옵션 저장됨: " + filePath);
    }
}

[System.Serializable]
public class SaveData
{
    public int ResolutionIdx;
    public int FullScreenModeIdx;
    public int FramerateIdx;
    public int TextureQualityIdx;
    public int ShadowQualityIdx;
    public float MasterVolume;
}