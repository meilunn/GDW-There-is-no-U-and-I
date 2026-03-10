using UnityEngine;
using UnityEngine.Events;

public class TriggerInteractable : Interactable
{
    public UnityEvent onInteract;
    
    //assign in inspector
    public override bool Interact()
    {
        onInteract.Invoke();
        return true;
    }
}
