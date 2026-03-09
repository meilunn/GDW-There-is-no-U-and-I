using UnityEngine;

public enum SpeakerName
{
    Main,
    Enemy,
    Peter,
    John,
    Susy,
    Beatrice
}
[CreateAssetMenu(fileName = "NewSpeaker", menuName = "Dialogue/Speaker")]
public class Speaker : ScriptableObject
{
    public SpeakerName speakerName;   // The name of the speaker
    public Sprite speakerImage; // The image representing the speaker
}