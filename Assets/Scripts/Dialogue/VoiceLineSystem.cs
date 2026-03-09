using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class VoiceLineSystem : MonoBehaviour
{
    [SerializeField] private Dictionary<string, Bark> barks;
    [SerializeField] private TextMeshProUGUI lineText;
    [SerializeField] private GameObject speechBubbleUI;
    [SerializeField] private TMP_Text speechBubbleText;
    private AudioSource audioSource;
    
    private Tween hideBubbleTween;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PlayBark(string barkName)
    {
        // Kill any previous delayed hide so it resets the timer
        hideBubbleTween?.Kill();

        //Display Speech Bubble
        speechBubbleUI.SetActive(true);
        speechBubbleText.text = barks[barkName].barkText;

        audioSource.PlayOneShot(barks[barkName].voiceClip);

        // Disable the speech bubble after 5 seconds
        hideBubbleTween = DOVirtual.DelayedCall(5f, () =>
        {
            speechBubbleUI.SetActive(false);
        });
    }
}