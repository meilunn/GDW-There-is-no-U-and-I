using System;
using UnityEngine;

public class PlaceSlot : MonoBehaviour {
	public MovableInteractable.Type itemsAllowed;
	[NonSerialized]
	public bool isTaken;
	public MovableInteractable item;
	public event Action<MovableInteractable> OnItemRemoved;
	public event Action<MovableInteractable> OnItemPlaced;
	protected PlaceModule[] modules;
	public TeammateController owner;

	private void Start() {
		modules = GetComponents<PlaceModule>();
		if (item != null) {
			if(GetComponentInChildren<MovableInteractable>() != item) {
				item = Instantiate(item);
			}
			item.OnItemTaken += TakeItem;
			isTaken = true;
			item.Place(gameObject.transform);
		}
	}
	public virtual bool CanPlace(MovableInteractable newItem) {
		if (isTaken || newItem == null) return false;

		return (itemsAllowed & newItem.type) != 0;
	}

	public virtual void PlaceItem(MovableInteractable newItem) {
		if (newItem == null || item != null) return;
		isTaken = true;
		item = newItem;
		item.OnItemTaken += TakeItem;
		item.PlaceInWorld(gameObject.transform);
		foreach (var module in modules) {
			module.OnPlace(item);
		}
		OnItemPlaced?.Invoke(item);
	}

	public virtual void TakeItem() {
		item.OnItemTaken -= TakeItem;
		item = null;
		isTaken = false;
		foreach (var module in modules) {
			module.OnTake(item);
		}
		OnItemRemoved?.Invoke(item);
	}

	public void TakeItem(MovableInteractable itemToTake) {
		if (itemToTake == item) TakeItem();
	}
}
