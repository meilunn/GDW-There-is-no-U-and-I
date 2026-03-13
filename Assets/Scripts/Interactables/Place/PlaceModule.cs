using UnityEngine;

public abstract class PlaceModule : MonoBehaviour {
	public abstract void OnPlace(MovableInteractable item);
	public abstract void OnTake(MovableInteractable item);
}