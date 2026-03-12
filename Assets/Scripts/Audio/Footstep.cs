using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Footstep : MonoBehaviour
{
    private int MaterialValue;
    private RaycastHit rh;
    private float distance = 0.3f;
    private string EventPath = "event:/character/footsteps";
    private PARAMETER_ID ParamID;
    private PARAMETER_ID ParamID2;
    private LayerMask lm;
    
    private EventDescription eventDescription;
    private PARAMETER_DESCRIPTION ParamDescription;
    
    void Start()
    {
        // Herausfinden der IDs
        /*
         eventDescription = RuntimeManager.GetEventDescription(EventPath);
        eventDescription.getParameterDescriptionByName("Terrain", out ParamDescription);
        ParamID = ParamDescription.id;
        Debug.Log(ParamID.data1 + " " + ParamID.data2);
        */
        
        ParamID.data1 = 2986495963;
        ParamID.data2 = 2320075354;
        
        lm = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * distance, Color.red);
    }

    void PlayWalkEvent()
    {
        MaterialCheck();
        EventInstance Walk = RuntimeManager.CreateInstance(EventPath);
        RuntimeManager.AttachInstanceToGameObject(Walk, transform, GetComponent<Rigidbody>());
        
        Walk.setParameterByID(ParamID, MaterialValue, false);

        Walk.start();
    }

    void MaterialCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out rh, distance, lm))
        {
            switch (rh.collider.tag)
            {
                case "Wood":
                    MaterialValue = 0;
                    break;
                case "Tile":
                    MaterialValue = 1;
                    break;
                default:
                    MaterialValue = 0;
                    break;
            }
        }
    }
}
