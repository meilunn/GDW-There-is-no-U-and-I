public class VoidSlot : PlaceSlot {
	public override bool CanPlace(MovableInteractable newItem) {
		return (itemsAllowed & newItem.type) != 0;
	}

	public override void PlaceItem(MovableInteractable item) {
		if (item == null) return;
		item.PlaceInWorld(gameObject.transform);
		foreach (var module in modules) {
			module.OnPlace(item);
		}
		item.gameObject.SetActive(false);
	}
}