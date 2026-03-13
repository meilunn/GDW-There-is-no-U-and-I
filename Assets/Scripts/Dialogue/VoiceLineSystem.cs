using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(AudioSource))]
public class VoiceLineSystem : MonoBehaviour
{
    public enum BarkType
    {
        Bored,
        Hungry,
        Shitting,
        Satisfied,
        Sussy,
        Tired,
        Yapping
    }
    [SerializeField] private List<Bark> barks;
    [SerializeField] private GameObject speechBubbleUI;
    [SerializeField] private TMP_Text speechBubbleText;
    [SerializeField] private bool useTypewriterEffect = false;
    [SerializeField] private float timeBetweenLetters = 0.1f;
    private AudioSource audioSource;
    
    private Tween hideBubbleTween;
    private Coroutine typewriterCoroutine;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayBark(0);
    }
    
    public void PlayBark(BarkType index)
    {
        hideBubbleTween?.Kill();

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        speechBubbleUI.SetActive(true);
        Debug.Log("Playing bark: " + index);
        if (useTypewriterEffect)
        {
            typewriterCoroutine = StartCoroutine(TypeLine(barks[(int)index].barkText));
        }
        else
        {
            speechBubbleText.text = barks[(int)index].barkText;
            HideBubble();
        }

        if (barks[(int)index].voiceClip != null)
        {
            audioSource.PlayOneShot(barks[(int)index].voiceClip);
        }
    }

    private IEnumerator TypeLine(string line)
    {
        speechBubbleText.text = line;
        speechBubbleText.ForceMeshUpdate();

        int totalVisibleCharacters = speechBubbleText.textInfo.characterCount;
        speechBubbleText.maxVisibleCharacters = 0;

        for (int i = 1; i <= totalVisibleCharacters; i++)
        {
            speechBubbleText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(timeBetweenLetters);
        }

        typewriterCoroutine = null;
        HideBubble();
    }
    
    private void HideBubble()
    {
        Debug.Log("Hiding bubble");
        hideBubbleTween?.Kill();
        hideBubbleTween = DOVirtual.DelayedCall(3f, () =>
        {
            speechBubbleUI.SetActive(false);
        });
    }
}
