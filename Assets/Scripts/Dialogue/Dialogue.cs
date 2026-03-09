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
    public Speaker speaker;
}

[CreateAssetMenu(fileName = "NewBark", menuName = "Dialogue/Line")]
public class Bark : ScriptableObject
{
    [TextArea] public string barkText; // Dialogue text
    public AudioClip voiceClip;  // Audio clip for the dialogue
    public Speaker speaker;
}
