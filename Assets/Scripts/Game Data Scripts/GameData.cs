using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor.Experimental.RestService;

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
} 

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;

    // Start is called before the first frame update
    void Awake()
    {
        if (gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        } else {
            Destroy(this.gameObject);
        }
        Load();
    }

    private void Start()
    {
        
    }

    public void Save()
    {
        // ������� �������� ������������� ��� ������ �������� ������
        BinaryFormatter formatter = new BinaryFormatter();
        
        // ������� ���� �� ��������� � �����
        FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Create);
        
        // ������� ����� ����������� ������
        SaveData data = new SaveData();
        data = saveData;
        
        // ��������� ������ � ����
        formatter.Serialize(file, data);
        
        // ������� ����� ������
        file.Close();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.dataPath + "/player.json", json);

        Debug.Log("Saved");
    }

    public void Load()
    {
        // ��������� ���������� �� ���� ����
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            // ������� �������� ��������������
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
            Debug.Log("Loaded");
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnDisable()
    {
        Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
