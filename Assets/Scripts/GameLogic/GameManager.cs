using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager instance;

	public static event Action OnDayStart;

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
	private float happinessMeter = 0;
	private float maxHappinessMeter = 0;
	[Tooltip("The amount of happiness the player can achieve per day. Higher values result in more quests and a higher difficulty.")]
	[SerializeField]
	[Min(0)]
	private float targetHappinessPerDay = 20f;
	[SerializeField]
	[Tooltip("This changes the maximum amount of happiness the player can achieve per day. Use this to balance quest decisionmaking. Not considered when checking for laziness.")]
	[Min(0)]
	private float maxHappinessOvertime = 3f;
	[SerializeField]
	[Tooltip("The percentage of the target happiness per day that the player needs to achieve in order to not get fired for laziness. Higher values result in a lower tolerance for missing the target happiness.")]
	[Range(0, 1)]
	private float requiredHappinessPercentage = 0.75f;
	private float requiredHappinessPerDay;
	private float happinessToday;
	public ProjectProgress projectProgress;

	void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}

		// also need to set happiness in order to avoid getting fired on the first day
		happinessToday = requiredHappinessPerDay = targetHappinessPerDay * 0.75f;
		instance = this;
	}

	void Start() {
		StartCoroutine(SetupNewDay());
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
		return 0;
		// das ist irgendwie bullshit
		// float modifier = Mathf.Pow(100f / MaxDays / requiredProgressPerDay, 2) - 1f;
		// float susIncrease = modifier * Mathf.Pow(curDay, 2.8f);
		// return Mathf.Max(0f, Mathf.Min(100f, susIncrease));
	}

	IEnumerator OnDayEnd() {
		yield return new WaitForSeconds(3f);

		susMeter += CalculateSusIncrease();

		StartCoroutine(SetupNewDay());
	}

	IEnumerator SetupNewDay() {
		curDay++;
		dayTime = dayStartTime;
		bool fired = susMeter >= 100f || happinessToday < requiredHappinessPerDay;
		curGameState = GameState.StandUp;
		questManager.ClearQuests();


		if (!fired) {
			// Generate new daily quests

			List<Quest> selectedQuests = new();
			float currentWeight = 0;

			while (true) {
				float remaining = targetHappinessPerDay - currentWeight;
				List<KeyValuePair<Quest, float>> candidates = new();
				foreach (var quest in questManager.AvailableJobQuests) {
					if (quest.availableAsOfDay > curDay) continue;
					int selectedCount = selectedQuests.Count(q => q.id == quest.id);
					float baseP = selectedCount != 0 ?
						quest.chooseAgainProbability - (selectedCount - 1) * quest.chooseAgainProbabilityDecrease :
						quest.chooseProbability;

					if (baseP <= 0) continue;
					if (currentWeight + quest.weight > targetHappinessPerDay + maxHappinessOvertime) continue;

					float fit = remaining / quest.weight;
					float fitFactor = Mathf.Min(1f, fit);

					float weightBias = Mathf.Sqrt(quest.weight);

					float effectiveP = baseP * fitFactor * weightBias;

					candidates.Add(new KeyValuePair<Quest, float>(quest, effectiveP));
				}

				if (candidates.Count == 0) break;

				Quest chosen = Util.WeightedRandom(candidates, new System.Random());

				selectedQuests.Add(chosen);
				currentWeight += chosen.weight;

				if (currentWeight >= targetHappinessPerDay) {
					requiredHappinessPerDay = currentWeight * requiredHappinessPercentage;
					maxHappinessMeter += currentWeight;
					break;
				}
			}

			foreach (var quest in selectedQuests) {
				questManager.AddQuest(quest.id, null, true);
			}
		}

		// TODO: Handle Standup Meeting

		if (fired) {
			Debug.Log("You're fired!");
			curGameState = GameState.GameOver;
			yield break;
		}

		curGameState = GameState.Work;
		happinessToday = 0;
		OnDayStart?.Invoke();
	}

	public void AddHappiness(float amount) {
		happinessMeter += amount;
		happinessToday += amount;
	}
}
