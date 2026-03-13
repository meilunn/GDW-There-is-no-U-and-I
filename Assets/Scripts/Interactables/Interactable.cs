using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract bool Interact();
    
    [SerializeField]
    protected Outline outline;
    
    void Start()
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }
        TurnInteractable(false);
        OnStart();
    }

	public virtual void Enable() {
		enabled = true;
	}

	public virtual void Disable() {
		enabled = false;
		outline.enabled = false;
	}
    
    protected virtual void OnStart() {}
    public void TurnInteractable(bool toActive)
    {
		if(!enabled) return;
        if (outline)
        {
            outline.enabled = toActive;
        }
        //todo: optionally dis/enable hints or so
    }

    
}
