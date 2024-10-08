using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;

    public GameStats game1Stats = new GameStats();
    public GameStats game2Stats = new GameStats();
    public GameStats game3Stats = new GameStats();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadStats();
    }

    public void SaveStats()
    {
        string json = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/stats.json", json);
    }

    public void LoadStats()
    {
        string path = Application.persistentDataPath + "/stats.json";
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    public void AddGameSession(int gameIndex, GameSession session)
    {
        switch (gameIndex)
        {
            case 1:
                game1Stats.AddSession(session);
                break;
            case 2:
                game2Stats.AddSession(session);
                break;
            case 3:
                game3Stats.AddSession(session);
                break;
        }

        SaveStats();
    }
}
