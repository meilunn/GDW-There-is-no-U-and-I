using DG.Tweening;
using UnityEngine;

public class CoffeeMachine : PlaceInteractable {
	bool running = false;

	[SerializeField]
	private GameObject coffeeButton;
	[SerializeField]
	private GameObject decafbutton;
	[SerializeField]
	private EdibleData coffeeData;
	[SerializeField]
	private EdibleData decafData;
	[SerializeField]
	private GameObject filledCupPrefab;

	public void OnCoffeePress() {
		if(running) return;
		_ = Brew(false);
	}

	public void OnDecafPress() {
		if(running) return;
		_ = Brew(true);
	}

	private async Awaitable Brew(bool decaf) {
		await PressButton(decaf ? decafbutton : coffeeButton);
		if(running) return;
		if(!CheckForItem(MovableInteractable.Type.Cup, out var item, out var slot)) return;
		if(CheckForItem(EdibleData.EdibleType.Coffee, out var _, out var _)) return;
		running = true;
		item.Disable(); //prevents the player from picking up the cup while it's brewing

		// TODO: Start Animation

		await Awaitable.WaitForSecondsAsync(10f);

		var filledCupObject = Instantiate(filledCupPrefab);
		var filledCup = filledCupObject.GetComponent<MovableInteractable>();
		filledCup.MakeEdible(decaf ? decafData : coffeeData);
		slot.TakeItem();
		slot.PlaceItem(filledCup);
		Destroy(item.gameObject);
		running = false;
	}

	private async Awaitable PressButton(GameObject button) {
		await DOTween.Sequence()
			.Append(button.transform.DOLocalMoveZ(button.transform.localPosition.z - 0.03f, 0.1f))
			.AppendInterval(0.2f)
			.Append(button.transform.DOLocalMoveZ(button.transform.localPosition.z, 0.1f))
			.AsyncWaitForCompletion();
	}
}