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
        if (isTaken)
            return false;

        return (itemsAllowed & newItem.type) != 0;
    }
    
    public void PlaceItem(MovableInteractable newItem)
    {
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
    
    //todo: AI checks whether there is an item of typeB in placeSlots
}
