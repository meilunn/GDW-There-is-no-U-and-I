using UnityEngine;

public class PlaceInteractable : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Transform itemPosition;
    public override void Interact()
    {
        if (Player.Instance.ItemInHand != null)
        {
            Player.Instance.ItemInHand.PlaceInWorld(itemPosition);
        }
    }
}
