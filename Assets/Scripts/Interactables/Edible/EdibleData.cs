using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EdibleData", menuName = "Interactable/EdibleData")]
public class EdibleData : ScriptableObject
{
    public EdibleType type;
    public float bladder;
    public float hunger;
    public float energy;
    
    [Serializable]
    public enum EdibleType
    {
        Coffee,
        DecafCoffee,
        Sandwich,
        EnergyDrink
    }
}
