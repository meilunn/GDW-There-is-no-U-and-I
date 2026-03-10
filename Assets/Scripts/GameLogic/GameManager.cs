using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager instance;


	public enum GameState {
		TitleScreen,
		StandUp,
		Work,
		EndOfDay,
		GameOver
	}

	public GameState curGameState;
	public QuestManager questManager;

	public int MaxDays;
	public int curDay;

	public double dayTime;
	[Tooltip("Time in seconds since 00:00. 8:00 AM would be 28800 seconds")]
	public double dayStartTime;
	[Tooltip("Time in seconds since 00:00. 6:00 PM would be 64800 seconds")]
	public double dayEndTime;
	[SerializeField]
	[Tooltip("Amount of ingame seconds that pass per real second.")]
	private double timeScale = 120.0f;

	public float susMeter = 0;  // 0-100
	[SerializeField]
	[Range(0, 100)]
	private float progressMeter = 0;  // 0-100
	private float requiredProgressPerDay;
	private float progressMadeToday = 0;

	// public List<Task> tasksOfTheDay = new();


	void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}

		requiredProgressPerDay = 75f / MaxDays;
		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		SetupNewDay();
	}

	void Update() {
		if (susMeter > 100.0f || curDay > MaxDays)
			curGameState = GameState.GameOver;

		if (dayTime >= dayEndTime) {
			curGameState = GameState.EndOfDay;
			StartCoroutine(OnDayEnd());
		}
		dayTime += timeScale * Time.deltaTime;
	}

	float CalculateSusIncrease() {
		float modifier = Mathf.Pow(100f / MaxDays / requiredProgressPerDay, 2) - 1f;
		float susIncrease = modifier * Mathf.Pow(curDay, 2.8f);
		return Mathf.Max(0f, Mathf.Min(100f, susIncrease));
	}

	IEnumerator OnDayEnd() {
		yield return new WaitForSeconds(3f);

		// When everyone left the office, calculate new values
		// susMeter and progressMeter are changed during the day
		// Increase susMeter based on how much progress was made, the less progress, the more sus
		susMeter += CalculateSusIncrease();

		SetupNewDay();
	}

	void SetupNewDay() {
		curDay++;
		dayTime = dayStartTime;
	}

	public void AddProgress(float amount) {
		progressMeter += amount;
		progressMadeToday += amount;
		curGameState = GameState.StandUp;

		// Handle Standup Meeting

		curGameState = GameState.Work;
	}
}
