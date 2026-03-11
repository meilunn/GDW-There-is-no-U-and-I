using UnityEngine;

public class PlaceInteractable : Interactable {

	public Transform itemPosition;
	public TeammateController owner;

	private PlaceSlot[] slots;
	void Awake() {
		slots = GetComponentsInChildren<PlaceSlot>();
	}
	public override bool Interact() {
		MovableInteractable item = Player.Instance.ItemInHand;
		if (item != null) {
			foreach (PlaceSlot slot in slots) {
				if (slot.CanPlace(item)) {
					slot.PlaceItem(item);
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Checks if the Place includes an item of the given type. If so, returns true and outputs the item and its slot.
	/// </summary>
	/// <param name="itemType"></param>
	/// <param name="item"></param>
	/// <param name="takenSlot"></param>
	/// <returns>true if an item exists, false otherwise</returns>
	public bool CheckForItem(MovableInteractable.Type itemType, out MovableInteractable item, out PlaceSlot takenSlot) {
		item = null;
		takenSlot = null;
		foreach (PlaceSlot slot in slots) {
			if (slot.item != null) {
				if (slot.item.type == itemType) {
					item = slot.item;
					takenSlot = slot;
					return true;
				}
				//e.g. for plates
				if (slot.item.TryGetComponent<PlaceInteractable>(out PlaceInteractable recursivePlace)) {
					if (recursivePlace.CheckForItem(itemType, out item, out takenSlot)) return true;
				}

			}
		}
		return false;
	}

	/// <summary>
	/// Checks if the Place includes an item of the given type. If so, returns true and outputs the item and its slot.
	/// </summary>
	/// <param name="itemType"></param>
	/// <param name="item"></param>
	/// <param name="takenSlot"></param>
	/// <returns>true if an item exists, false otherwise</returns>
	public bool CheckForItem(EdibleData.EdibleType itemType, out Edible item, out PlaceSlot takenSlot) {
		item = null;
		takenSlot = null;
		foreach (PlaceSlot slot in slots) {
			if (slot.item != null) {
				if (slot.item.TryGetComponent<Edible>(out Edible edible) && edible.GetEdibleType() == itemType) {
					item = edible;
					takenSlot = slot;
					return true;
				}
				//e.g. for plates
				if (slot.item.TryGetComponent<PlaceInteractable>(out PlaceInteractable recursivePlace)) {
					if (recursivePlace.CheckForItem(itemType, out item, out takenSlot)) return true;
				}

			}
		}
		return false;
	}

	public PlaceSlot GetEmptySlotForType(MovableInteractable.Type itemType) {
		foreach (PlaceSlot slot in slots) {
			if(slot.itemsAllowed.HasFlag(itemType) && !slot.isTaken) return slot;
		}
		return null;
	}
}
