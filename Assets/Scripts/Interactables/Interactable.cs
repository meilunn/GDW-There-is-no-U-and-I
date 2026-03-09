using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract void Interact();
    
    Outline _outline;
    
    
    void Start()
    {
        _outline = GetComponent<Outline>();
        TurnInteractable(false);
    }

    public void TurnInteractable(bool toActive)
    {
        if (_outline)
        {
            _outline.enabled = toActive;
        }
        //todo: optionally dis/enable hints or so
    }

    
}
