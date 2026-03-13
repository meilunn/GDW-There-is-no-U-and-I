public class Container : PlaceInteractable {
	private Hinge[] hinges;

	public override void Awake() {
		base.Awake();
		hinges = GetComponentsInChildren<Hinge>();
	}

	public override bool Interact() {
		foreach (Hinge hinge in hinges) {
			if (hinge.IsOpen) {
				if (Player.Instance.ItemInHand) return base.Interact();
				else _ = hinge.Close();
			} else {
				_ = hinge.Open();
			}
		}
		return true;
	}
}