using UnityEngine;

public class PlaceSlot: MonoBehaviour
{
    public MovableInteractable.Type itemsAllowed;
    public bool isTaken;
    public MovableInteractable item;

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
    }

    public void TakeItem()
    {
        item.OnItemTaken -= TakeItem;
        item = null;
        isTaken = false;
    }

    public void TakeItem(MovableInteractable itemToTake)
    {
        if (itemToTake == item) TakeItem();
    }
}
