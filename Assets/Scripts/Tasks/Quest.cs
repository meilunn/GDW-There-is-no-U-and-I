using System;
using UnityEngine;

public enum QuestId {
	EmptyTrash
}

// this can be used for UI ig?
public enum QuestState {
	NotStarted,
	InProgress,
	Completed
}

public enum QuestType {
	Job,
	Request,
	Sabotage,
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject {
	public QuestId id;
	[NonSerialized] public TeammateController owner;
	public Objective[] objectives;

	public QuestType type;
	[Header("Job Quest Config")]
	[Tooltip("The day on which this quest becomes available during the standup meeting.")] [Min(1)]
	public int availableAsOfDay = 1;
	[Tooltip("The base probability that this quest will be chosen.")]
	public float chooseProbability;
	[Tooltip("The probability that this quest will be offered again if it already exists during this day.")]
	public float chooseAgainProbability;
	[Tooltip("The amount by which the probability of this quest being offered again decreases for each time it has already been offered during this day.")]
	public float chooseAgainProbabilityDecrease;
	[Tooltip("The amount of happiness added to the happiness meter when completing this quest. Also taken into account when choosing quests.")]
	public float weight;
	[Tooltip("The amount of days that have to pass before this quest can be offered again after being chosen.")]
	public int cooldown;

	[Header("Quest Info")]
	[Tooltip("The quest's title displayed on the whiteboard and in the UI.")]
	public string title;
	public string description;
	[Tooltip("Indicates whether this quest is a quest given to the player by their team during the standup meeting in the morning.")]


	public QuestState State { get; private set; } = QuestState.NotStarted;

	public bool IsActive => State == QuestState.InProgress;
	private int currentObjectiveIndex;

	public Objective CurrentObjective => objectives[currentObjectiveIndex];

	public void Start() {
		State = QuestState.InProgress;
	}

	/// <summary>
	/// Completes the current objective and moves on to the next one. Also handles quest completion when there are no more objectives left.
	/// </summary>
	/// <returns>true if the quest is completed, false otherwise</returns>
	public bool CompleteCurrentObjective() {
		if (currentObjectiveIndex >= objectives.Length) {
			OnComplete();
			return true;
		}
		currentObjectiveIndex++;
		return false;
	}

	private void OnComplete() {
		State = QuestState.Completed;
		GameManager.instance.AddHappiness(weight);
	}
}