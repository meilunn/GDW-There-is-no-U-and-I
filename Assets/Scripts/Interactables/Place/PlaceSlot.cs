using UnityEngine;

public class PlaceSlot: MonoBehaviour
{
    public MovableInteractable.Type itemsAllowed;
    public bool isTaken;
    public MovableInteractable item;
	public delegate void PlaceSlotEvent(MovableInteractable item);
	public event PlaceSlotEvent OnItemRemoved;
	public event PlaceSlotEvent OnItemPlaced;

    private void Start()
    {
        if (item != null)
        {
            item.OnItemTaken += TakeItem;
            isTaken = true;
            item.Place(gameObject.transform);
        }
    }
    public bool CanPlace(MovableInteractable newItem)
    {
        if (isTaken || newItem == null) return false;

        return (itemsAllowed & newItem.type) != 0;
    }
    
    public void PlaceItem(MovableInteractable newItem)
    {
        if (newItem == null || item != null) return;
        isTaken = true;
        item = newItem;
        item.OnItemTaken += TakeItem;
        item.PlaceInWorld(gameObject.transform);
		OnItemPlaced?.Invoke(item);
    }

    public void TakeItem()
    {
        item.OnItemTaken -= TakeItem;
        item = null;
        isTaken = false;
		OnItemRemoved?.Invoke(item);
    }

    public void TakeItem(MovableInteractable itemToTake)
    {
        if (itemToTake == item) TakeItem();
    }
}
