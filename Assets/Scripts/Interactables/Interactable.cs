using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract void Interact();
    
    [SerializeField]
    private Outline outline;
    
    
    void Start()
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }
        TurnInteractable(false);
    }

    public void TurnInteractable(bool toActive)
    {
        if (outline)
        {
            outline.enabled = toActive;
        }
        //todo: optionally dis/enable hints or so
    }

    
}
