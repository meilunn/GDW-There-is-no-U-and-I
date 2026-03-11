using System;
using System.Collections.Generic;
using UnityEngine;

public class Edible : MonoBehaviour
{
    public EdibleData baseIngredient;

    public List<ExtraIngredientData> extraIngredients = new  List<ExtraIngredientData>();

    public GameObject edibleItem;
    
    public EdibleData.EdibleType GetEdibleType() => baseIngredient.type;
    

    public Vector3 GetParams()
    {
        float bladder = baseIngredient.bladder;
        float hunger = baseIngredient.hunger;
        float energy = baseIngredient.energy;

        foreach (var ingredient in extraIngredients)
        {
            bladder += ingredient.bladder;
            hunger += ingredient.hunger;
            energy += ingredient.energy;
        }

        return new Vector3(bladder, hunger, energy);
    }
    
    public void Consume()
    {
        if (edibleItem != null)
        {
            if (edibleItem.TryGetComponent<MovableInteractable>(out var movable) &&
                edibleItem.transform.parent?.TryGetComponent<PlaceSlot>(out var placeSlot) == true)
            {
                placeSlot.TakeItem(movable);
            }
            Destroy(edibleItem);
        }

        Destroy(this);
    }
    
    public void AddIngredient(ExtraIngredientData ingredient)
    {
        extraIngredients.Add(ingredient);
    }
}
