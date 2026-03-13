using UnityEngine;

public class StandUpPlayerSeat : BaseStandUpSeat {
	public override void Release() {
		Player.Instance.RemoveMoveLock(gameObject);
	}

	public override void Sit() {
		Vector3 seatPosition = transform.position + Vector3.up;
		Player.Instance.transform.SetPositionAndRotation(seatPosition, transform.rotation);
		Player.Instance.AddMoveLock(gameObject);
	}
}