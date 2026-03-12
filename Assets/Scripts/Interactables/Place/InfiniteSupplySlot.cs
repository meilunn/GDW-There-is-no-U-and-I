public class InfiniteSupplySlot : PlaceSlot {
	public override bool CanPlace(MovableInteractable _) {
		return false;
	}

	public override void PlaceItem(MovableInteractable _) {
		// this is intentionally left empty as items can never be placed in this slot
	}

	public override void TakeItem() {
		var newItem = Instantiate(item);
		base.TakeItem();
		item = newItem;
		isTaken = true;
		item.OnItemTaken += TakeItem;
		item.PlaceInWorld(gameObject.transform);
	}
}