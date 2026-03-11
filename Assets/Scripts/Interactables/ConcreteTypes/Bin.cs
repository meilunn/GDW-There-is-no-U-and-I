using UnityEngine;

public class Bin : Interactable {
	private PlaceSlot trashSlot;
	private PlaceSlot overflowSlot;
	[SerializeField] private GameObject trashItemPrefab;

	void Start() {
		trashSlot = transform.Find("Trash Slot").GetComponent<PlaceSlot>();
		overflowSlot = transform.Find("Overflow Slot").GetComponentInChildren<PlaceSlot>();
		overflowSlot.OnItemPlaced += ClearOverflow;
	}

	public override bool Interact() {
		MovableInteractable item = Player.Instance.ItemInHand;
		if (item == null) {
			if(!trashSlot.item) return false;
			trashSlot.TakeItem();
			var trashItem = trashSlot.item;
			trashItem.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			trashItem.Place(Player.Instance.PlayerHand);
			Player.Instance.ItemInHand = trashItem;
			GameManager.instance.questManager.CompleteObjective(ObjectiveId.TrashEmptyBin, null);
		} else {
			FillBin(item);
			overflowSlot.PlaceItem(item);
		}
		return true;
	}

	public void FillBin(MovableInteractable item) {
		if (trashSlot.item != null) return;
		var newItem = Instantiate(trashItemPrefab).GetComponent<MovableInteractable>();
		trashSlot.PlaceItem(newItem);
	}

	void ClearOverflow(MovableInteractable _) {
		if (overflowSlot.item != null) {
			overflowSlot.TakeItem();
		}
	}
}