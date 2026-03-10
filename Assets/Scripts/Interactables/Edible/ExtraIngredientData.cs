using UnityEngine;

[CreateAssetMenu(fileName = "ExtraIngredientData", menuName = "Interactable/ExtraIngredientData")]
public class ExtraIngredientData : ScriptableObject
{
    public int bladder;
    public int hunger;
    public int energy;
    
    //todo: optionally, "suspicious" when consumed
}
