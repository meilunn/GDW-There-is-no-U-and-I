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


    public GameObject player;

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
            if (curTeammateState != TeammateState.AtWorkplace)
                GoToDestination(Place.Workplace);
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
    }
}
