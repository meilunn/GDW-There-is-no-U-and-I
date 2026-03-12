using DG.Tweening;
using UnityEngine;

public class Hinge : MonoBehaviour {
	[SerializeField] 
	private Vector3 openRotation;
	[SerializeField]
	private float animationDuration = 0.5f;
	public bool IsOpen { get; private set; } = false;
	private bool isAnimating = false;

	public async Awaitable Open() {
		if(isAnimating || IsOpen) return;
		isAnimating = true;
		await transform.DOLocalRotate(openRotation, animationDuration).SetEase(Ease.InOutFlash).OnComplete(() => isAnimating = !(IsOpen = true)).AsyncWaitForCompletion();
	}

	public async Awaitable Close() {
		if(isAnimating || !IsOpen) return;
		isAnimating = true;
		IsOpen = false;
		await transform.DOLocalRotate(Vector3.zero, animationDuration).SetEase(Ease.InOutFlash).OnComplete(() => isAnimating = false).AsyncWaitForCompletion();
	}
}