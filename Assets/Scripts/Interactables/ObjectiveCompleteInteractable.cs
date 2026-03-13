using UnityEngine;

public class ObjectiveCompleteInteractable : Interactable {
	public ObjectiveId objectiveId;
	[SerializeField]
	private MovableInteractable.Type types;
	[SerializeField]
	private bool emptyHand = true;

	//assign in inspector
	public override bool Interact() {
		if (Player.Instance.ItemInHand == null && emptyHand || ((Player.Instance.ItemInHand?.type ?? 0) & types) != 0)
			GameManager.instance.questManager.CompleteObjective(objectiveId, null);
		return true;
	}
}