using UnityEngine;

[RequireComponent(typeof(PlaceSlot))]
public class PlaceObjectiveCompleteModule : PlaceModule {
	[SerializeField]
	private MovableInteractable.Type requiredItemType;
	[SerializeField]
	private ObjectiveId completedObjective;
	private PlaceSlot slot;

	void Start() {
		slot = GetComponent<PlaceSlot>();
	}

	public override void OnPlace(MovableInteractable item) {
		if((item.type & requiredItemType) == 0) return;
		GameManager.instance.questManager.CompleteObjective(completedObjective, slot.owner);
	}

	public override void OnTake(MovableInteractable item) {
		
	}
}