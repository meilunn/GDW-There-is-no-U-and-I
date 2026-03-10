using System.Collections.Generic;
using UnityEngine;

// should be replaced by ai agents lateron
public enum TempAgent {
	Agent1,
	Agent2,
	Agent3
}

public class QuestManager : MonoBehaviour {
	private readonly List<Quest> quests = new();

	/// <summary>
	/// Completes an objective with the given id.
	/// </summary>
	/// <param name="objectiveId"><see cref="ObjectiveId"/> of the completed <see cref="Objective"/>. </param>
	/// <returns>true if an active objective was found and completed, false otherwise.</returns>
	public bool CompleteObjective(ObjectiveId objectiveId, TempAgent owner) {
		foreach (var quest in quests) {
			if(!quest.IsActive) continue;
			if(quest.CurrentObjective.id != objectiveId) continue;
			if(quest.CurrentObjective.ownerRequired && quest.owner != owner) continue;
			quest.CompleteCurrentObjective();
			return true;
		}
		return false;
	}

	public void ClearQuests() {
		quests.Clear();
	}

	public void AddQuest(QuestId questId, TempAgent owner) {
		if(quests.Exists(q => q.id == questId && q.owner == owner)) return;

		var quest = Instantiate(Resources.Load<Quest>($"Quests/{questId}"));
		quest.owner = owner;
		quests.Add(quest);
	}

	/// <summary>
	/// Starts a quest with the given id. If the quest exists multiple times (e.g. for different owners), the first one found will be started.
	/// </summary>
	/// <param name="questId">ID of the quest to be started</param>
	public void StartQuest(QuestId questId) {
		var quest = quests.Find(q => q.id == questId);
		if(quest == null) {
			Debug.LogError($"Tried to start quest {questId} but it was not found in the quest manager.");
			return;
		}
		quest.Start();
	}

	public void CreateAndStartQuest(QuestId questId, TempAgent owner) {
		AddQuest(questId, owner);
		StartQuest(questId);
	}
}