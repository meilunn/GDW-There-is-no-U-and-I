using System;
using UnityEngine;

public class StandUpMeeting : MonoBehaviour {
	[SerializeField]
	private BaseStandUpSeat[] seats;

	private void Start()
	{
		DialogueSystem.Instance.OnDialogueEnd += EndMeeting;
	}

	private void OnDestroy()
	{
		if (DialogueSystem.Instance != null)
			DialogueSystem.Instance.OnDialogueEnd -= EndMeeting;
	}

	public async Awaitable StartMeeting(int curDay) {
		Debug.Log("Start Standup Meeting");
		foreach (var seat in seats) {
			seat.Sit();
		}
		DialogueSystem.Instance.StartDialogue(curDay - 1, DialogueSystem.DialogueType.StandUp);
	}

	public void EndMeeting()
	{
		if (DialogueSystem.Instance == null || DialogueSystem.Instance.CurrentDialogueType != DialogueSystem.DialogueType.StandUp)
			return;

		Whiteboard.AnimateTasksForAll();

		switch (GameManager.instance.EmploymentState) {
			case GameManager.PlayerEmploymentState.Employed: {
				foreach (var seat in seats) {
					seat.Release();
				}
				break;
			}
			case GameManager.PlayerEmploymentState.TooSus: {

				break;
			}
			case GameManager.PlayerEmploymentState.TooLazy: {

				break;
			}
		}
	}
}