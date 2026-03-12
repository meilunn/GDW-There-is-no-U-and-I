using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

	public enum PlayerEmploymentState {
		Employed,
		TooSus,
		TooLazy,
		TeamWon,
		TeamLost
	}

	public GameState curGameState;
	public PlayerEmploymentState EmploymentState { get; private set; }
	public QuestManager questManager;
	public StandUpMeeting standUpMeeting;
	private CanvasGroup fadeToBlack;

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

	public float susIncrease;
	private float susMeter = 0;  // 0-100
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

	public TeammateController[] teammates;

	void Awake() {
		if (instance != null) {
			Destroy(gameObject);
			return;
		}

		fadeToBlack = transform.Find("Overlays/ScreenFade").GetComponent<CanvasGroup>();

		// also need to set happiness in order to avoid getting fired on the first day
		happinessToday = requiredHappinessPerDay = targetHappinessPerDay * 0.75f;
		instance = this;
	}

	void Start() {
		if (teammates == null || teammates.Length == 0)
			teammates = FindObjectsByType<TeammateController>(FindObjectsSortMode.None);
		_ = SetupNewDay();
	}

	void OnEnable() {
		if (DialogueSystem.Instance != null)
			DialogueSystem.Instance.OnDialogueEnd.AddListener(StartWorkDay);
	}

	void OnDisable() {
		if (DialogueSystem.Instance != null)
			DialogueSystem.Instance.OnDialogueEnd.RemoveListener(StartWorkDay);
	}

	void Update() {
		if (susMeter > 100.0f || curDay > MaxDays)
			curGameState = GameState.GameOver;

		if (dayTime >= dayEndTime) {
			curGameState = GameState.EndOfDay;
			dayTime = -1;
			StartCoroutine(OnDayEnd());
		}
		if (dayTime >= 0) dayTime += timeScale * Time.deltaTime;
	}

	float CalculateSusIncrease() {
		return 0;
		// das ist irgendwie bullshit
		// float modifier = Mathf.Pow(100f / MaxDays / requiredProgressPerDay, 2) - 1f;
		// float susIncrease = modifier * Mathf.Pow(curDay, 2.8f);
		// return Mathf.Max(0f, Mathf.Min(100f, susIncrease));
	}

	public void IncreaseSus() {
		Debug.Log("Increase sus");
		susMeter += susIncrease;
	}

	private IEnumerator OnDayEnd() {
		// wait for staff leaving
		Debug.Log("Day ended. Waiting for staff to leave...");
		yield return new WaitForSeconds(3f);
		yield return FadeTo(Fade.Black);

		susMeter += CalculateSusIncrease();

		curDay++;
		if (curDay < MaxDays) yield return SetupNewDay();
		else Debug.Log("Game Ended, should show Siegererehrung and evaluate team progress");
	}

	async Awaitable SetupNewDay() {
		Debug.Log("new day");
		dayTime = dayStartTime;
		if (susMeter >= 100f) EmploymentState = PlayerEmploymentState.TooSus;
		else if (happinessToday < requiredHappinessPerDay) EmploymentState = PlayerEmploymentState.TooLazy;
		curGameState = GameState.StandUp;
		DialogueSystem.Instance.StartDialogue(curDay - 1, DialogueSystem.DialogueType.StandUp);
		questManager.ClearQuests();

		if (EmploymentState == 0) {
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

			Debug.Log("Selected quests:");

			foreach (var quest in selectedQuests) {
				Debug.Log(quest.Title);
				questManager.AddQuest(quest.id, null, true).Start();
			}
		}

		await standUpMeeting.StartMeeting();

		if (EmploymentState != 0) {
			Debug.Log("You're fired!");
			curGameState = GameState.GameOver;
			return;
		}
	}

	public void StartWorkDay() {
		curGameState = GameState.Work;
		happinessToday = 0;
		OnDayStart?.Invoke();
	}

	public async Awaitable FadeTo(Fade alpha) {
		await fadeToBlack.DOFade((float)alpha, 1f).AsyncWaitForCompletion();
	}

	public void AddHappiness(float amount) {
		happinessMeter += amount;
		happinessToday += amount;
	}
}
