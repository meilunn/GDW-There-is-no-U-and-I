using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PatrolController))]
[RequireComponent(typeof(NavMeshAgent))]
public class TeammateController : MonoBehaviour
{
    public enum TeammateState
    {
        AtWorkplace,
        Sleeping,
        Shitting,
        Patrolling,
        GoingToDestination
    }

    [Header("States")]
    public TeammateState initialTeammateState = TeammateState.AtWorkplace;
    public TeammateState curTeammateState;

    [Header("Stats")]
    public float drowsiness = 0f;
    public float drowsinessIncrease;  // const increase per sec, should be npc dependant
    public float drowsinessDecrease;  // when napping
    public float bladder = 0f;
    public float bladderIncrease;
    public float hunger = 0f;
    public float hungerIncrease;

    [Header("Walking")]
    public float baseWalkSpeed;
    public float walkSpeedDrowsinessScale;
    public float walkSpeedBladderScale;
    private float walkSpeed;  

    [Header("Places")]
    public Place curDestination = Place.None;
    [Space(10)]
    public GameObject workplace;
    public GameObject toilet;
    public GameObject exit;
    
    [Header("Field of View params")] 
    public float angle = 90f;
    public float radius = 10f;
    
    [Header("Detection params")] 
    public int detectionThreshold = 100;
    public static int currentPoints;
    [SerializeField]
    private int pointsPerCheck;
    [SerializeField]
    private LayerMask traceAgainst;
    private bool susDetected;
    public Transform rayCastOrigin;

    public enum Place
    {
        None,
        Workplace, 
        Toilet,
        Exit
    }

    public TMP_Text teammateStateText;

    private float time = 0f;

    private NavMeshAgent agent;
    private PatrolController patrolController;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolController = GetComponent<PatrolController>();
        
        curTeammateState = initialTeammateState;
        walkSpeed = baseWalkSpeed;

        GoToDestination(Place.Workplace);
    }

    private void Update()
    {
        drowsiness += drowsinessIncrease * Time.deltaTime;
        bladder += bladderIncrease * Time.deltaTime;
        hunger += hungerIncrease * Time.deltaTime;
        
        teammateStateText.text = $"Teammate state: {curTeammateState}";

        if (drowsiness >= 100 && curTeammateState != TeammateState.Sleeping)
        {
            if (curTeammateState != TeammateState.AtWorkplace)
                            GoToDestination(Place.Workplace);
        }
        else if (bladder >= 100 && curTeammateState != TeammateState.Shitting)
                GoToDestination(Place.Toilet);
            

        switch (curTeammateState)
        {
            case TeammateState.AtWorkplace: 
                if (drowsiness >= 100)
                {
                    curTeammateState = TeammateState.Sleeping;
                    break;
                }
                DetectHand();
                // for testing: start patrol after 5 secs of working
                time += Time.deltaTime;  // TODO: randomise going to patrol

                if (time >= 5f)
                {
                    time = 0f;
                    patrolController.StartPatrol();    
                }
                break;
            case TeammateState.GoingToDestination:
                // if agent hasn't arrived at dest
                DetectHand();
                if (agent.remainingDistance > 0.1f) break; 

                // else
                Debug.Log($"Arrived at {curDestination}");

                switch (curDestination)
                {
                    case Place.Workplace: 
                        curTeammateState = TeammateState.AtWorkplace;
                        curDestination = Place.None;

                        break;
                    case Place.Toilet: 
                        CheckToiletPaper();

                        break;
                    case Place.Exit: 
                        

                        break;
                }

                break;

            case TeammateState.Sleeping: 
                if (drowsiness <= 0)  // wake up
                {
                    curTeammateState = TeammateState.AtWorkplace;
                    break;
                }

                drowsiness -= drowsinessDecrease * Time.deltaTime;

                break;
            default:
                DetectHand();
                break;
        } 
    }
     private void DetectHand()
    {   //TODO: optionally, change amount of current/needed detect-points depending on sleepiness
        susDetected = false;
        Vector3 handPosition = Player.Instance.PlayerHand.position;
        Vector3 distanceToHand = handPosition - transform.position;
        if (distanceToHand.magnitude <= radius)
        {
            if (Vector3.Dot(distanceToHand.normalized, transform.forward) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad))
            {
                RaycastHit hit;
                
                Vector3 direction = handPosition - rayCastOrigin.position;
               
                if (Physics.Raycast(rayCastOrigin.position, direction, out hit, traceAgainst))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.TryGetComponent<MovableInteractable>(out MovableInteractable detectedItem) && detectedItem == Player.Instance.ItemInHand && detectedItem.isSuspicious)
                    {
                        Debug.DrawRay(rayCastOrigin.position, direction, Color.green);
                        susDetected = true;
                        currentPoints += (int)Math.Ceiling(pointsPerCheck * Time.deltaTime * 4);
                        if (currentPoints >= detectionThreshold)
                        {
                            //TODO: Loose condition
                            Debug.Log("Player would loose here because sus item was detected");
                            currentPoints = 0;
                        }
                    }
                    else
                    {
                        susDetected = false;
                        Debug.DrawRay(rayCastOrigin.position, direction, Color.red);
                        if (currentPoints > 0) currentPoints -= (int)Math.Ceiling(1 * Time.deltaTime);
                    }
                }
            }
            else
            {
                if (currentPoints > 0) currentPoints -= (int)Math.Ceiling(1 * Time.deltaTime);
            }
        }
    }
    

    public float GetWalkSpeed()
    {
        // TODO: scale with drowsiness & bladder
        return walkSpeed;
    }

    public void GoToDestination(Place place)
    {
        curTeammateState = TeammateState.GoingToDestination;

        agent.speed = GetWalkSpeed();

        curDestination = place;

        switch(place)
        {
            case Place.Workplace: 
                Debug.Log("Going to workplace");

                agent.SetDestination(workplace.transform.position);
                break; 
            
            case Place.Toilet: 
                Debug.Log("Going to toilet");

                agent.SetDestination(toilet.transform.position);
                break;

            case Place.Exit: 
                Debug.Log("Going to Exit");

                agent.SetDestination(exit.transform.position);
                break; 
        }        
    }
    
    private void CheckToiletPaper()
    {
        // TODO: 
        throw new NotImplementedException();
    }

    // Draw places
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        Gizmos.DrawCube(workplace.transform.position, new Vector3(0.4f, 0.4f, 0.4f));
        
        //FoV
        Color c = new Color(0, 0, 0.6f, 0.4f);
        UnityEditor.Handles.color = c;
        Vector3 rotatedForward = Quaternion.Euler(0, -angle * 0.5f, 0) * transform.forward;
        UnityEditor.Handles.DrawSolidArc(rayCastOrigin.position, Vector3.up,rotatedForward, angle, radius);
    }
}
