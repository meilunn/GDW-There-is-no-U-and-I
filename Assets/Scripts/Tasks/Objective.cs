using UnityEditor;

public enum ObjectiveId {

}

public class Objective : SerializedObject {
	public Objective(UnityEngine.Object obj) : base(obj) { }

	public ObjectiveId id;
	public string description;
	public bool ownerRequired;
}