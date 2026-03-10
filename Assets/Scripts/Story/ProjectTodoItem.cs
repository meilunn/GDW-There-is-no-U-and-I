using UnityEngine;

[CreateAssetMenu(fileName = "New Project Todo Item", menuName = "Story/Project Todo Item")]
public class ProjectTodoItem : ScriptableObject {
	public string title;
	// @RadiergummiXD idk ob du das brauchst, aber es ist mal da
	public string dialogContent;
	public float progressRequired;
}