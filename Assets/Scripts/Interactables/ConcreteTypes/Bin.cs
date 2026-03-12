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
			// borderline kriminell
			trashItem.Interact();
			GameManager.instance.questManager.CompleteObjective(ObjectiveId.TrashEmptyBin, null);
		} else {
			FillBin();
			overflowSlot.PlaceItem(item);
		}
		return true;
	}

	public void FillBin() {
		if (trashSlot.item != null) return;
		var newItem = Instantiate(trashItemPrefab).GetComponent<MovableInteractable>();
		trashSlot.PlaceItem(newItem);
	}

	private void OnGUI() {
		if(GUILayout.Button("Fill Bin (Debug)")) {
			FillBin();
		}
	}
}