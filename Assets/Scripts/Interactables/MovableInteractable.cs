using System;
using UnityEngine;

public class MovableInteractable : Interactable
{
    public event Action OnItemPlaced;
    public event Action OnItemTaken;
    
    public Type type;

    public bool isEdible => GetComponent<Edible>() != null;
    public bool isSuspicious;
    
    
    public override bool Interact()
    {
		if(!enabled) return false;
        MovableInteractable itemInHand = Player.Instance.ItemInHand;
        if (itemInHand == null)
        {
            //to be ignored by interaction
            gameObject.layer = LayerMask.NameToLayer("Item In Hand");
            Transform hand = Player.Instance.PlayerHand;
            Place(hand);
            Player.Instance.ItemInHand = this;
            OnItemTaken?.Invoke();
            return true;
        }
        
        else if (isEdible && itemInHand.TryGetComponent<Medicine>(out Medicine medicine))
        {
            GetComponent<Edible>().AddIngredient(medicine.medicineData);
            Player.Instance.ItemInHand = null;
            Destroy(itemInHand.gameObject);
            return true;
        }
        return false;
    }
    
    //e.g. for coffee machine
    public void MakeEdible(EdibleData baseIngredient)
    {
        if (baseIngredient == null)
        {
            Debug.LogError("Making edible requires base Ingredient");
            return;
        }
        var edible = gameObject.AddComponent<Edible>();
        edible.baseIngredient = baseIngredient;
    }
    public void PlaceInWorld(Transform parent)
    {
        Place(parent);
        gameObject.layer = LayerMask.NameToLayer("Default");
        if (Player.Instance.ItemInHand == this) Player.Instance.ItemInHand = null;
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
        //Sizes to place
        Small = 1 << 0,
        Medium = 1 << 1,
        Big = 1 << 2,
        //Cup is not edible, but can be make edible 
        Cup = 1 << 3,
        ToiletPaper = 1 << 4,
        //Plates can have food in them (both Movable & Place Interactables)
        Plate =  1 << 5,
        EnergyDrink =  1 << 6,
        USB = 1 << 7,
		Trash = 1 << 8,
		WaterBottle = 1 << 10,
		Vodka = 1 << 11
    }
}
