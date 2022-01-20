using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializeManager : MonoBehaviour
{
    private PlayersData _playersDataFromFile;

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    public void SaveProgress(PlayersData progress)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Create(Application.persistentDataPath + "/Saves/save.binary");
        formatter.Serialize(saveFile, progress);

        saveFile.Close();
        _playersDataFromFile = progress;
    }

    private PlayersData LoadDataFromFile()
    {
        if (Directory.Exists(Application.persistentDataPath + "/Saves"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream saveFile = File.Open(Application.persistentDataPath + "/Saves/save.binary", FileMode.Open);

            _playersDataFromFile = (PlayersData)formatter.Deserialize(saveFile);

            saveFile.Close();
        }
        else _playersDataFromFile = new PlayersData();

        return _playersDataFromFile;
    }

    public PlayersData LoadProgress()
    {
        PlayersData data;
        if (_playersDataFromFile == null)
            data = LoadDataFromFile();
        else data = _playersDataFromFile;

        return data;
    }
}