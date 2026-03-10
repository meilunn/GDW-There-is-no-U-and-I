using UnityEngine;

[CreateAssetMenu(fileName = "NewBark", menuName = "Dialogue/Bark")]
public class Bark : ScriptableObject
{
    [TextArea] public string barkText; // Dialogue text
    public AudioClip voiceClip;  // Audio clip for the dialogue
    public Character character;
}