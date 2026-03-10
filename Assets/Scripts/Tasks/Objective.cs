using UnityEngine;

public enum ObjectiveId {

}

[CreateAssetMenu(fileName = "New Objective", menuName = "Quests/Objective")]
public class Objective : ScriptableObject {

	public ObjectiveId id;
	public string description;
	public bool ownerRequired;
}