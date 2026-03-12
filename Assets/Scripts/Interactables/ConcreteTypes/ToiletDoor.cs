using UnityEngine;

public class ToiletDoor : Door {
	[SerializeField]
	private PlaceInteractable place;

	public override bool Interact() {
		if(!locked || !Player.Instance.ItemInHand) return base.Interact();
		if(Player.Instance.ItemInHand.type == MovableInteractable.Type.ToiletPaper)
			return place.Interact();
		return base.Interact();
	}
}