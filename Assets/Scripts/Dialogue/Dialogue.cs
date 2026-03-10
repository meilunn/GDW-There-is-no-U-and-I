using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    public List<DialogueLine> dialogues;
}

[System.Serializable]
public class DialogueLine
{
    [TextArea] public string dialogueText; // Dialogue text
    public AudioClip voiceClip;  // Audio clip for the dialogue
    public Character character;
}