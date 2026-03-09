using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;


public class DialogueSystem : MonoBehaviour
{
    public enum DialogueType
    {
        Enemy,
        StandUp,
        Story
    }
    
    public static DialogueSystem Instance { get; private set; } // Singleton instance
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text dialogueText; // UI Text component for displaying dialogue
    [SerializeField] private GameObject dialoguePanel; // The dialogue UI panel
    [SerializeField] private TMP_Text speakerLabel;
    [SerializeField] private Image speakerImage;

    

    [FormerlySerializedAs("StandUpDialogues")]
    [Header("Dialogues")]
    [SerializeField] private List<Dialogue> standUpDialogues;
    [FormerlySerializedAs("EnemyDialogues")] [SerializeField] private List<Dialogue> enemyDialogues;
    [FormerlySerializedAs("StoryDialogues")] [SerializeField] private List<Dialogue> storyDialogues;
    /* Play only once + save (not necessary)
    private bool[] storyDialoguePlayed;
    private bool[] tutorialDialoguePlayed;
    private bool[] towerDialoguePlayed;
    
    private const string StoryKey = "StoryDialoguePlayed";
    private const string TutorialKey = "TutorialDialoguePlayed";
    private const string TowerKey = "TowerDialoguePlayed";
    */
    
    private Queue<DialogueLine> dialogueQueue; // Queue of dialogue lines with speakers
    private string currentLine = "";
    private bool isTyping = false;
    [SerializeField]private bool typeWriterEffect = false;
    [SerializeField] private float timeBetweenLetters = 0.5f;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dialogueQueue = new Queue<DialogueLine>();
        dialoguePanel.SetActive(false);
        
        //LoadProgress();
    }

    
    public void StartDialogue(int index, DialogueType dialogueType)
    {
        List<Dialogue> dialogues = null;
        //bool[] dialoguePlayed = null;
        switch (dialogueType)
        {
            case DialogueType.StandUp:
                dialogues = standUpDialogues;
                //dialoguePlayed = tutorialDialoguePlayed;
                break;
            case DialogueType.Enemy:
                dialogues = standUpDialogues;
                //dialoguePlayed = tutorialDialoguePlayed;
                break;
            case  DialogueType.Story:
                dialogues = storyDialogues;
                break;
        }
        
        // Ensure Dialogues is valid before proceeding
        //if (dialogues == null || dialoguePlayed == null)
        if(dialogues == null)
        {
            Debug.LogError("Invalid uninitialized dialogue group.");
            EndDialogue();
            return;
        }

        if (index < 0 || index >= dialogues.Count)
        {
            Debug.LogError("Invalid Dialogue index");
            EndDialogue();
            return;
        }

        //if (!dialoguePlayed[index])
        //{
        StartDialogue(dialogues[index]);
        
        //dialoguePlayed[index] = true;
        //}
    }

    
    
    
    public void StartDialogue(Dialogue dialogue)
    {
        dialogueQueue.Clear();

        foreach (var line in dialogue.dialogues)
        {
            dialogueQueue.Enqueue(line);
        }
        
        
        
        dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (isTyping)
        {
            // Skip typing animation
            StopAllCoroutines();
            dialogueText.text = currentLine;
            isTyping = false;
            return;
        }

        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentDialogue = dialogueQueue.Dequeue();
        currentLine = currentDialogue.dialogueText;

        // Update UI elements
        if (currentDialogue.speaker != null)
        {
            speakerLabel.text = Enum.GetName(typeof(SpeakerName),currentDialogue.speaker.speakerName);
            speakerImage.sprite = currentDialogue.speaker.speakerImage;
        }
        else
        {
            speakerLabel.text = "";
            speakerImage.sprite = null;
        }

        // Play audio
        audioSource.Stop();
        if (currentDialogue.voiceClip != null)
        {
            audioSource.clip = currentDialogue.voiceClip;
            audioSource.Play();
        }
        
        if(typeWriterEffect){
            // Start text typing coroutine
            StartCoroutine(TypeLine(currentLine));
        }
        else
        {
            dialogueText.text = currentLine;
        }
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (var letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(timeBetweenLetters); 
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        //gameManager.InvokeResumeGame(); Start Game
    }

    /*
    #region Save & Load
   
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextLine();
        }
    }

    public void SaveProgress()
    {
        SaveBoolArray(StoryKey, storyDialoguePlayed);
        SaveBoolArray(TutorialKey, tutorialDialoguePlayed);
        SaveBoolArray(TowerKey, towerDialoguePlayed);
    }

    private void LoadProgress()
    {
        bool resetDialogues = PlayerPrefs.GetInt("resetDialogues", 1) == 1; // Default is true (1)

        if (resetDialogues)
        {
            // Initialize fresh dialogue progress
            storyDialoguePlayed = new bool[storyDialogues.Count];
            tutorialDialoguePlayed = new bool[tutorialDialogue.Count];
            towerDialoguePlayed = new bool[towerDialogue.Count];

            Debug.Log("playDialogues = true - Resetting dialogue progress.");
        }
        else
        {
            // Load existing progress from PlayerPrefs
            storyDialoguePlayed = LoadBoolArray(StoryKey, storyDialogues.Count);
            tutorialDialoguePlayed = LoadBoolArray(TutorialKey, tutorialDialogue.Count);
            towerDialoguePlayed = LoadBoolArray(TowerKey, towerDialogue.Count);
            
            Debug.Log("playDialogues = false - Loading dialogue progress from PlayerPrefs.");
        }
    }

    private void SaveBoolArray(string key, bool[] boolArray)
    {
        string data = "";
        foreach (bool b in boolArray)
        {
            data += b ? "1" : "0"; // Convert bools to a "10101" string
        }
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
    }

    private bool[] LoadBoolArray(string key, int expectedSize)
    {
        if (!PlayerPrefs.HasKey(key)) // If no saved data, initialize a new array
        {
            return new bool[expectedSize]; // Defaults to all false
        }

        string data = PlayerPrefs.GetString(key);
        bool[] boolArray = new bool[expectedSize];

        for (int i = 0; i < expectedSize; i++)
        {
            if (i < data.Length) // Ensure we don't go out of bounds
            {
                boolArray[i] = data[i] == '1';
            }
            else
            {
                boolArray[i] = false; // Default to false if data is missing
            }
        }
        return boolArray;
    }
    #endregion
    */
}











