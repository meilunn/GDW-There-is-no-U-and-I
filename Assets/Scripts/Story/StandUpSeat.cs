using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseStandUpSeat : MonoBehaviour {
	public abstract void Sit();
	public abstract void Release();
}

public class StandUpSeat : BaseStandUpSeat {
	[SerializeField]
	private TeammateController occupant;
	private NavMeshAgent occupantAgent;

	private void Awake() {
		if (occupant != null)
			occupantAgent = occupant.GetComponent<NavMeshAgent>();
	}

	public override void Release() {
		if (occupantAgent != null) {
			if (!occupantAgent.enabled)
				occupantAgent.enabled = true;
			occupantAgent.isStopped = false;
		}

		occupant.curTeammateState = TeammateController.TeammateState.GoingToDestination;
		occupant.curDestination = TeammateController.Place.Workplace;
		occupant.enabled = true;
		Debug.Log(occupant.gameObject + " is released to work");

	}

	public override void Sit() {
		Vector3 seatPosition = transform.position + Vector3.up;

		if (occupantAgent != null) {
			if (!occupantAgent.enabled)
				occupantAgent.enabled = true;
			occupantAgent.ResetPath();
			bool suc = occupantAgent.Warp(seatPosition);
			Debug.Log(occupant.gameObject + " is warped to " + seatPosition);
			Debug.Log("Warp succesful:" + suc);
			occupantAgent.isStopped = true;
		}

		occupant.enabled = false;
		occupant.transform.SetPositionAndRotation(seatPosition, transform.rotation);
		Debug.Log(occupant.gameObject + " is sitting");
	}
}