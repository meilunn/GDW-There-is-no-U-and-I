using UnityEngine;

public class PlaceInteractable : Interactable
{

    public Transform itemPosition;

    private PlaceSlot[] slots;
    void Awake()
    {
        slots = GetComponentsInChildren<PlaceSlot>();
    }
    public override void Interact()
    {
        MovableInteractable item = Player.Instance.ItemInHand;
        if (item != null)
        {
            foreach (PlaceSlot slot in slots)
            {
                if (slot.CanPlace(item))
                {
                    slot.PlaceItem(item);
                    break;
                }
            }
        }
    }
}
