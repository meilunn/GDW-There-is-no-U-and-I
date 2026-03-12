public class Container : PlaceInteractable {
	private Hinge hinge;

	public override void Awake() {
		base.Awake();
		hinge = GetComponentInChildren<Hinge>();
	}

	public override bool Interact() {
		if(hinge.IsOpen) {
			if(Player.Instance.ItemInHand) return base.Interact();
			else _ = hinge.Close();
		} else {
			_ = hinge.Open();
		}
		return true;
	}
}