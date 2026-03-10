using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EdibleData", menuName = "Interactable/EdibleData")]
public class EdibleData : ScriptableObject
{
    public EdibleType type;
    public int bladder;
    public int hunger;
    public int energy;
    
    [Serializable]
    public enum EdibleType
    {
        Coffee,
        DecafCoffee,
        Sandwich,
        EnergyDrink
    }
}
