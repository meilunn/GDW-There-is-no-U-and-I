using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PatrolController))]
[RequireComponent(typeof(NavMeshAgent))]
public class TeammateController : MonoBehaviour
{
    public enum TeammateState
    {
        AtTable,
        Sleeping,
        Shitting,
        Patrolling,
        GoingToDestination
    }

    [Header("States")]
    public TeammateState initialTeammateState = TeammateState.AtTable;
    public TeammateState curTeammateState;

    [Header("Stats")]
    public float drowsiness = 0f;
    public float drowsinessIncrease;  // const increase, should be npc dependant
    public float bladder = 0f;
    public float bladderIncrease;

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

    public enum Place
    {
        None,
        Workplace, 
        Toilet
        // TODO: 
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
    }

    private void Update()
    {
        teammateStateText.text = $"Teammate state: {curTeammateState}";

        switch (curTeammateState)
        {
            case TeammateState.AtTable: 
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
                        curTeammateState = TeammateState.AtTable;
                        curDestination = Place.None;
                        break;
                    // TODO: 
                }

                break;
        }


        
    }

    public float GetWalkSpeed()
    {
        // TODO: scale with drowsiness & bladder
        return walkSpeed;
    }

    public void GoingToDestination(Place place)
    {
        curTeammateState = TeammateState.GoingToDestination;

        agent.speed = GetWalkSpeed();

        curDestination = place;

        switch(place)
        {
            case Place.Workplace: 
                Debug.Log("Returning to workplace");

                agent.SetDestination(workplace.transform.position);
                break; 
            
            case Place.Toilet: 
                Debug.Log("Returning to workplace");

                agent.SetDestination(toilet.transform.position);
                break; 
        }
        
    }

    // Draw places
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        Gizmos.DrawCube(workplace.transform.position, new Vector3(0.4f, 0.4f, 0.4f));
    }
}
