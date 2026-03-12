using UnityEngine;

public class WasteItem : MovableInteractable {
	private GameObject trashBag;
	private GameObject binContent;

	void Awake() {
		Debug.Log("Awake");
		trashBag = transform.Find("Trash Bag").gameObject;
		binContent = transform.Find("Bin Content").gameObject;
	}

	void OnEnable() {
		OnItemPlaced += UpdateVisuals;
		OnItemTaken += UpdateVisuals;
	}

	void OnDisable() {
		OnItemPlaced -= UpdateVisuals;
		OnItemTaken -= UpdateVisuals;
	}

	void UpdateVisuals() {
		Debug.Log("Updating visuals");
		bool isInBin = transform.parent.CompareTag("Bin");
		trashBag.SetActive(!isInBin);
		binContent.SetActive(isInBin);
		if (isInBin) {
			outline = null;
		} else {
			outline = trashBag.GetComponent<Outline>();
		}
	}
}