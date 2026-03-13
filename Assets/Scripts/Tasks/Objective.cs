using UnityEngine;

public enum ObjectiveId {
	TrashEmptyBin,
	TrashVoidGarbageBag,
	ToiletPaperRefill,
	BringFoodToTable,
	BringCoffeeToTable,
	BringEnergyToTable,
	WaterPlant,
}

[CreateAssetMenu(fileName = "New Objective", menuName = "Quests/Objective")]
public class Objective : ScriptableObject 
{
	public ObjectiveId id;
	public string description;
	public bool ownerRequired;
}