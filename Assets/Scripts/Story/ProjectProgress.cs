using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Project Progress Tracker", menuName = "Story/Project Progress Tracker")]
public class ProjectProgress : ScriptableObject {
	[NonSerialized]
	public float currentProgress;

	[SerializeField]
	private float expectedProgressPerDay;

	[SerializeField]
	private float progressPerWorkUnit;

	[SerializeField]
	private ProjectTodoItem[] todos;

	/// <summary>
	/// Should be called every time a unit of work is done on the project.
	/// </summary>
	/// <param name="efficiency">Efficiency is used as a factor and should be around 1. Higher values indicate higher productivity while lower values result in less productivity.</param>
	public void Work(float efficiency) {
		currentProgress += progressPerWorkUnit * efficiency;
	}

	/// <summary>
	/// Generates the list of all todo items that are expected to be completed during the next day.
	/// </summary>
	public List<ProjectTodoItem> CurrentTodoItems {
		get {
			List<ProjectTodoItem> items = new();
			float progress = 0;
			foreach (var item in todos) {
				progress += item.progressRequired;
				if (currentProgress < progress) {
					if (currentProgress + expectedProgressPerDay >= progress) {
						break;
					}
					items.Add(item);
				}
			}
			return items;
		}
	}
}