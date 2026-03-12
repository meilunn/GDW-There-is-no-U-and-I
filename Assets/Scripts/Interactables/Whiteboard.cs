using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class Whiteboard : Interactable {
	private TMP_Text teamTodos;
	private TMP_Text playerQuests;
	[SerializeField] private float timeBetweenLetters = 0.03f;

	void Awake() {
		teamTodos = transform.Find("Canvas/TeamTodos").GetComponent<TMP_Text>();
		playerQuests = transform.Find("Canvas/PlayerTasks").GetComponent<TMP_Text>();
	}

	void Start() {
		DialogueSystem.Instance.OnDialogueEnd.AddListener(DisplayTasks);
	}

	void OnDestroy() {
		if (DialogueSystem.Instance != null)
			DialogueSystem.Instance.OnDialogueEnd.RemoveListener(DisplayTasks);
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

	private void DisplayTasks() {
		Debug.Log("Displaying tasks");
		if (DialogueSystem.Instance.CurrentDialogueType != DialogueSystem.DialogueType.StandUp) return;
		DailyUpdate();
		StartCoroutine(TypewriterEffect(teamTodos));
		StartCoroutine(TypewriterEffect(playerQuests));
	}

	private IEnumerator TypewriterEffect(TMP_Text textComponent) {
		DailyUpdate();
		textComponent.ForceMeshUpdate();
		int totalCharacters = textComponent.textInfo.characterCount;
		textComponent.maxVisibleCharacters = 0;

		for (int i = 1; i <= totalCharacters; i++) {
			textComponent.maxVisibleCharacters = i;
			yield return new WaitForSecondsRealtime(timeBetweenLetters);
		}
	}
	
}