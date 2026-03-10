using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract bool Interact();
    
    [SerializeField]
    private Outline outline;
    
    void Start()
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }
        TurnInteractable(false);
        OnStart();
    }
    
    protected virtual void OnStart() {}
    public void TurnInteractable(bool toActive)
    {
        if (outline)
        {
            outline.enabled = toActive;
        }
        //todo: optionally dis/enable hints or so
    }

    
}
