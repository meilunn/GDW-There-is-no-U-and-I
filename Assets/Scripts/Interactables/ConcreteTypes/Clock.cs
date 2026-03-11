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

	public override bool Interact() {
		if (activeTimeOffset != 0) return false;
		StartCoroutine(TimeChangeCoroutine());
		return true;
	}

	protected override void OnStart() {
		hourHand = transform.Find("Hour Hand");
		minuteHand = transform.Find("Minute Hand");
		timeChangePerSecond = timeOffset / timeChangeDuration;
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
		double time = GameManager.instance.dayTime + activeTimeOffset;
		double hourRevs = time / hourDivisor;
		double minuteRevs = time / minuteDivisor;

		hourHand.localRotation = Quaternion.Euler(0, 0, (float)hourRevs);
		minuteHand.localRotation = Quaternion.Euler(0, 0, (float)minuteRevs);
	}

	IEnumerator TimeChangeCoroutine() {
		yield return DOTween.To(x => activeTimeOffset = x, activeTimeOffset, timeOffset, timeChangeDuration).SetEase(Ease.InOutCubic).WaitForCompletion();
		int day = GameManager.instance.curDay;
		yield return new WaitForSeconds(timeChangeEffectDuration);
		if (GameManager.instance.curDay != day) yield break;  // if the day has changed since the time change started, don't change the time again
		yield return DOTween.To(x => activeTimeOffset = x, activeTimeOffset, 43200, (43200-timeOffset) * 2 / timeChangePerSecond).SetEase(Ease.Linear).WaitForCompletion();
	}
}