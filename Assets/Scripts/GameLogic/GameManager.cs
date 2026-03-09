using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    public enum GameState
    {
        TitleScreen,
        StandUp,
        Work,
        EndOfDay,
        GameOver
    }
    public GameState curGameState;

    public int MaxDays;
    public int curDay;

    //public TimerForDay

    public float susMeter = 0;  // 0-100
    public float progressMeter = 0;  // 0-100

    // public List<Task> tasksOfTheDay = new();


    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (susMeter > 100.0f || curDay > MaxDays)
            curGameState = GameState.GameOver;
    }


}
