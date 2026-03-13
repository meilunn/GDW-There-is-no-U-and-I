public class StandUpPlayerSeat : BaseStandUpSeat {
	public override void Release() {
		Player.Instance.RemoveMoveLock(gameObject);
	}

	public override void Sit() {
		Player.Instance.transform.SetPositionAndRotation(transform.position, transform.rotation);
		Player.Instance.AddMoveLock(gameObject);
	}
}