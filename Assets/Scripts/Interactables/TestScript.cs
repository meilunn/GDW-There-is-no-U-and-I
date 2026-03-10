using UnityEngine;

public class TestScript : MonoBehaviour
{
    public PlaceInteractable table;
    public MovableInteractable cup;
    public EdibleData coffee;
    public void CheckTableFor(EdibleData.EdibleType type)
    {
        if (table.CheckForItem(type, out var edible, out var slot))
        {
            Debug.Log("Found item of type " + type);
            Debug.Log("Items stats: " + edible.GetParams());
        }
        else
        {
            Debug.Log("Didn't find item of type " + type);
        }
        
    }

    public void CheckTableForCoffe()
    {
        CheckTableFor(EdibleData.EdibleType.Coffee);
    }

    public void CheckTableForSandwich()
    {
        CheckTableFor(EdibleData.EdibleType.Sandwich);
    }
    public void makeCupCoffee()
    {
        cup.MakeEdible(coffee);
    }

    public Edible sandwich;
    public void ConsumeSandwich()
    {
        sandwich.Consume();
    }
}
