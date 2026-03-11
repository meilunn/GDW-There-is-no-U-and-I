public class VoidSlot : PlaceSlot {
	public override bool CanPlace(MovableInteractable newItem) {
		return (itemsAllowed & newItem.type) != 0;
	}

	public override void PlaceItem(MovableInteractable newItem) {
        if (newItem == null) return;
        newItem.PlaceInWorld(gameObject.transform);
		Destroy(newItem.gameObject);
	}
}