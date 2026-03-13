using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {
	public List<Quest> Quests { get; private set; } = new();
	private readonly List<Quest> availableJobQuests = new();
	public List<Quest> AvailableJobQuests => availableJobQuests;
	private readonly List<Quest> availableSabotageQuests = new();

	void Awake() {
		// foreach(var quest in Resources.LoadAll<Quest>("Quests")) {
		// 	Debug.Log($"Loaded quest {quest.id}");
		// 	switch (quest.type) {
		// 		case QuestType.Job:
		// 			availableJobQuests.Add(quest);
		// 			break;
		// 		case QuestType.Sabotage:
		// 			availableSabotageQuests.Add(quest);
		// 			break;
		// 	}
		// }
	}

	/// <summary>
	/// Completes an objective with the given id.
	/// </summary>
	/// <param name="objectiveId"><see cref="ObjectiveId"/> of the completed <see cref="Objective"/>. </param>
	/// <returns>true if an active objective was found and completed, false otherwise.</returns>
	public bool CompleteObjective(ObjectiveId objectiveId, TeammateController owner) {
		foreach (var quest in Quests) {
			if(!quest.IsActive) continue;
			if(quest.CurrentObjective.id != objectiveId) continue;
			if(quest.CurrentObjective.ownerRequired && quest.owner != owner) continue;
			Debug.Log("Completing objective " + objectiveId.ToString());
			bool res = quest.CompleteCurrentObjective();
			if(res) {
				Debug.Log($"Completed quest {quest.Title}");
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Clears all quests from the quest manager.
	/// </summary>
	public void ClearQuests() {
		Quests.Clear();
	}

	/// <summary>
	/// Adds a quest with the given id and owner to the quest manager. If a quest with the same id and owner already exists, it will not be added again.
	/// </summary>
	/// <param name="questId"></param>
	/// <param name="owner"></param>
	/// <param name="allowDuplicates">Will skip duplicate check if true.</param>
	public Quest AddQuest(QuestId questId, TeammateController owner, bool allowDuplicates = false) {
		if(!allowDuplicates && Quests.Exists(q => q.id == questId && q.owner == owner)) return null;

		var quest = Instantiate(Resources.Load<Quest>($"Quests/{questId}"));
		quest.owner = owner;
		Quests.Add(quest);
		return quest;
	}

	/// <summary>
	/// Starts a quest with the given id. If the quest exists multiple times (e.g. for different owners), the first one found will be started.
	/// </summary>
	/// <param name="questId">ID of the quest to be started</param>
	public void StartQuest(QuestId questId) {
		var quest = Quests.Find(q => q.id == questId && !q.IsActive);
		if(quest == null) {
			Debug.LogError($"Tried to start quest {questId} but it was not found in the quest manager.");
			return;
		}
		quest.Start();
	}

	/// <summary>
	/// Starts a quest with the given id and owner, even if it doesn't exist yet.
	/// </summary>
	/// <param name="questId"></param>
	/// <param name="owner"></param>
	public void CreateAndStartQuest(QuestId questId, TeammateController owner) {
		AddQuest(questId, owner);
		StartQuest(questId);
	}
}