using UnityEngine;

public class Medicine : MovableInteractable
{
    
    public ExtraIngredientData medicineData;

    protected override void OnStart()
    {
        isSuspicious = true;
    }
}
