using UnityEngine;

public abstract class BaseStandUpSeat : MonoBehaviour {
	public abstract void Sit();
	public abstract void Release();
}

public class StandUpSeat : BaseStandUpSeat {
	[SerializeField]
	private TeammateController occupant;

	public override void Release() {
		occupant.curTeammateState = TeammateController.TeammateState.GoingToDestination;
		occupant.curDestination = TeammateController.Place.Workplace;
		occupant.enabled = true;
	}

	public override void Sit() {
		occupant.enabled = false;
		occupant.transform.SetPositionAndRotation(transform.position, transform.rotation);
	}
}