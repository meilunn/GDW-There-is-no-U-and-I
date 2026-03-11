using System.Collections.Generic;
using System.Text;
using TMPro;

public class Whiteboard : Interactable {
	private TMP_Text teamTodos;
	private TMP_Text playerQuests;

	void Awake() {
		teamTodos = transform.Find("Canvas/TeamTodos").GetComponent<TMP_Text>();
		playerQuests = transform.Find("Canvas/PlayerTasks").GetComponent<TMP_Text>();
	}

	void OnEnable() {
		GameManager.OnDayStart += DailyUpdate;
	}

	void OnDisable() {
		GameManager.OnDayStart -= DailyUpdate;
	}

	public override bool Interact() {
		return false;
	}

	private void DailyUpdate() {
		UpdateTeamTasks(GameManager.instance.projectProgress.CurrentTodoItems);
		UpdatePlayerTasks(GameManager.instance.questManager.Quests);
	}

	public void UpdateTeamTasks(List<ProjectTodoItem> teamTodos) {
		StringBuilder sb = new();
		foreach (var item in teamTodos) {
			sb.AppendLine($"- {item.title}");
		}
		this.teamTodos.text = sb.ToString();
	}

	public void UpdatePlayerTasks(List<Quest> playerQuests) {
		StringBuilder sb = new();
		foreach (var quest in playerQuests) {
			if(quest.type != QuestType.Job) continue;
			string statusStart = quest.State == QuestState.Completed ? "<s>" : "";
			string statusEnd = quest.State == QuestState.Completed ? "</s>" : "";
			sb.AppendLine($"- {statusStart}{quest.title}{statusEnd}");
		}
		this.playerQuests.text = sb.ToString();
	}
}