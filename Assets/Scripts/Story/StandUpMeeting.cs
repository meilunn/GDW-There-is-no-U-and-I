using UnityEngine;

public class StandUpMeeting : MonoBehaviour {
	[SerializeField]
	private BaseStandUpSeat[] seats;

	public async Awaitable StartMeeting() {
		foreach (var seat in seats) {
			seat.Sit();
		}

		switch (GameManager.instance.EmploymentState) {
			case GameManager.PlayerEmploymentState.Employed: {

				foreach (var seat in seats) {
					seat.Release();
				}
				break;
			}
			case GameManager.PlayerEmploymentState.TooSus: {

				break;
			}
			case GameManager.PlayerEmploymentState.TooLazy: {

				break;
			}
		}
	}
}