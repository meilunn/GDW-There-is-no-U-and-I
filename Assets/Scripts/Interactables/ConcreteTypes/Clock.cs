using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Clock : Interactable {
	private Transform hourHand;
	private Transform minuteHand;

	const double hourDivisor = 21600 / 180;
	const double minuteDivisor = 1800 / 180;
	private float activeTimeOffset;
	[SerializeField]
	private float timeOffset;
	[SerializeField]
	private float timeChangeDuration;
	[SerializeField]
	private float timeChangeEffectDuration;
	private float timeChangePerSecond;
	private SusData susData;

	public override bool Interact() {
		if (activeTimeOffset != 0) return false;
		StartCoroutine(TimeChangeCoroutine());
		return true;
	}

	protected override void OnStart() {
		hourHand = transform.Find("Hour Hand");
		minuteHand = transform.Find("Minute Hand");
		timeChangePerSecond = timeOffset / timeChangeDuration;
		susData = GetComponent<SusData>();
	}

	void OnEnable() {
		GameManager.OnDayStart += ResetTimeOffset;
	}

	void OnDisable() {
		GameManager.OnDayStart -= ResetTimeOffset;
	}

	void ResetTimeOffset() {
		activeTimeOffset = 0;
	}

	void Update() {
		if (GameManager.instance.curGameState == GameManager.GameState.Work)
		{
			double time = GameManager.instance.dayTime + activeTimeOffset;
			double hourRevs = time / hourDivisor;
			double minuteRevs = time / minuteDivisor;

			hourHand.localRotation = Quaternion.Euler(0, 0, (float)hourRevs);
			minuteHand.localRotation = Quaternion.Euler(0, 0, (float)minuteRevs);
		}
	}

	IEnumerator TimeChangeCoroutine() {
		susData.Enable();
		yield return DOTween.To(x => activeTimeOffset = x, activeTimeOffset, timeOffset, timeChangeDuration).SetEase(Ease.InOutCubic).WaitForCompletion();
		GameManager.instance.dayTime += activeTimeOffset;
		activeTimeOffset = 0;
		susData.Disable();
	}
}