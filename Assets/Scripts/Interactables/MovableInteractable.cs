using System;
using UnityEngine;

public class MovableInteractable : Interactable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public event Action OnItemPlaced;
    public event Action OnItemTaken;
    
    public Type type;
    public override void Interact()
    {
        if (Player.Instance.ItemInHand == null)
        {
            //to be ignored by interaction
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            Transform hand = Player.Instance.PlayerHand;
            Place(hand);
            Player.Instance.ItemInHand = this;
            OnItemTaken?.Invoke();
        }
    }

    public void PlaceInWorld(Transform parent)
    {
        if (Player.Instance.ItemInHand != this) return;
        Place(parent);
        gameObject.layer = LayerMask.NameToLayer("Default");
        Player.Instance.ItemInHand = null;
        OnItemPlaced?.Invoke();
    }

    public void Place(Transform parent)
    {
        gameObject.transform.parent = parent;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;
    }
    [Flags]
    public enum Type
    {
        None = 0,
        Small = 1 << 0,
        Medium = 1 << 1,
        Cup = 1 << 2,
        ToiletPaper = 1 << 3
    }
}
