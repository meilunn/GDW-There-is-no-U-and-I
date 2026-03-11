using UnityEngine;

public class Bin : Interactable {
	private PlaceSlot trashSlot;
	private VoidSlot overflowSlot;
	[SerializeField] private GameObject trashItemPrefab;

	void Awake() {
		trashSlot = transform.Find("Trash Slot").GetComponent<PlaceSlot>();
		overflowSlot = transform.Find("Overflow Slot").GetComponentInChildren<VoidSlot>();
	}

	public override bool Interact() {
		MovableInteractable item = Player.Instance.ItemInHand;
		if (item == null) {
			if(!trashSlot.item) return false;
			var trashItem = trashSlot.item;
			trashSlot.TakeItem();
			trashItem.Place(Player.Instance.PlayerHand);
			trashItem.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
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
}