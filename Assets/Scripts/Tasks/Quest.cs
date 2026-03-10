using System;
using UnityEngine;

public enum QuestId {

}

// this can be used for UI ig?
public enum QuestState {
	NotStarted,
	InProgress,
	Completed
}

public class Quest : ScriptableObject {
	public QuestId id;
	[NonSerialized] public TempAgent owner;

	public int availableAsOfDay;
	[SerializeField]
	private float progressOnComplete;
	public string title;
	public string description;

	public Objective[] objectives;
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
		GameManager.instance.progressMeter += progressOnComplete;
	}
}