using DG.Tweening;
using UnityEngine;

partial class Door : Interactable {
	protected bool Open { get; private set; } = false;
	public bool locked = false;
	[SerializeField]
	private float openingAngle = 85f;
	[SerializeField]
	private float animationDuration = 0.5f;


	public override bool Interact() {
		if(locked) return false;

		Open = !Open;
		float targetAngle = Open ? openingAngle : 0f;
		transform.DOLocalRotate(new Vector3(0, targetAngle, 0), animationDuration);
		return true;
	}
}