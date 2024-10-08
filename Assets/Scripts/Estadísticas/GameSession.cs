using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSession : MonoBehaviour
{
    public DateTime date;
    public int correctAnswers;
    public int hintsRequested;
    public float totalTime;

    public GameSession(int correctAnswers, int hintsRequested, float totalTime)
    {
        this.date = DateTime.Now;
        this.correctAnswers = correctAnswers;
        this.hintsRequested = hintsRequested;
        this.totalTime = totalTime;
    }
}


[System.Serializable]
public class GameStats
{
    public List<GameSession> sessions;

    public GameStats()
    {
        sessions = new List<GameSession>();
    }

    public void AddSession(GameSession session)
    {
        sessions.Add(session);
    }

    public void Reset()
    {
        sessions.Clear();
    }
}