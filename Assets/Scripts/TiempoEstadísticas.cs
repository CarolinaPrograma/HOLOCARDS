using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TiempoEstadísticas : MonoBehaviour
{
    private Stopwatch stopwatch;
    private int mistakesCount;

    private void Start()
    {
        stopwatch = new Stopwatch();
        stopwatch.Start();

        mistakesCount = 0;
    }

    private void Update()
    {

    }

    private void SaveGameStats(TimeSpan elapsedTime, int mistakes)
    {

        List<GameStats> gameStatsList = LoadGameStats(); // Cargar estadísticas existentes si las hay
        GameStats newGameStats = new GameStats(elapsedTime.TotalSeconds, mistakes);
        gameStatsList.Add(newGameStats);

        // Guardar en PlayerPrefs (ejemplo)
        string statsJson = JsonUtility.ToJson(gameStatsList);
        PlayerPrefs.SetString("GameStats", statsJson);
        PlayerPrefs.Save();

        UnityEngine.Debug.Log("Estadísticas guardadas correctamente.");
    }

    private List<GameStats> LoadGameStats()
    {
        // Cargar estadísticas guardadas si las hay
        string statsJson = PlayerPrefs.GetString("GameStats", "");
        if (!string.IsNullOrEmpty(statsJson))
        {
            return JsonUtility.FromJson<List<GameStats>>(statsJson);
        }
        else
        {
            return new List<GameStats>();
        }
    }

    // Clase para guardar estadísticas de la partida
    [System.Serializable]
    private class GameStats
    {
        public double totalTime; // Tiempo total en segundos
        public int mistakes;     // Número de equivocaciones

        public GameStats(double totalTime, int mistakes)
        {
            this.totalTime = totalTime;
            this.mistakes = mistakes;
        }
    }
}
