using UnityEngine;
using UnityEngine.Events;

public class TriggerInteractable : Interactable {
	public UnityEvent onInteract;
	[SerializeField]
	private MovableInteractable.Type types;
	[SerializeField]
	private bool emptyHand = true;

	//assign in inspector
	public override bool Interact() {
		if (Player.Instance.ItemInHand == null && emptyHand || (Player.Instance.ItemInHand.type & types) != 0)
			onInteract.Invoke();
		return true;
	}
}