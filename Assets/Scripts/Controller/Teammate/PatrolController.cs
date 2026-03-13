using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TeammateController))]
public class PatrolController : MonoBehaviour
{
    public GameObject[] patrolWaypoints;
    private int index = -1;

    private NavMeshAgent agent;
    private TeammateController teammateController;

    public float waitTimeLongWaypoint;
    public float waitTimeShortWaypoint;

    private float waitTimeCurWaypoint;
    private bool curWaypointIsLong;

    private bool lastDest;

    private float stayTime;
    public bool IsWaitingAtWaypoint { get; private set; }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        teammateController = GetComponent<TeammateController>();
        
    }

    private void Update()
    {
        // Agent arrived at cur dest
        if (teammateController.curTeammateState == TeammateController.TeammateState.Patrolling 
            && agent.remainingDistance <= 0.1f)
        {
            IsWaitingAtWaypoint = true;
            stayTime += Time.deltaTime;
            if (stayTime >= waitTimeCurWaypoint)
            {
                stayTime = 0f;
                if (lastDest)
                    EndPatrol();
                else 
                    SetNextDestination();
            }
        }
        else
        {
            IsWaitingAtWaypoint = false;
        }
    }
    
    public bool IsPatrolInProgress()
    {
        return index >= 0 && index < patrolWaypoints.Length;
    }

    public void StartPatrol()
    {
        Debug.Log($"{gameObject.name} starting patrol");
        teammateController.curTeammateState = TeammateController.TeammateState.Patrolling;
        teammateController.curDestination = TeammateController.Place.None;

        SetNextDestination();
        agent.speed = teammateController.GetWalkSpeed();
    }

    public void EndPatrol()
    {
        Debug.Log($"{gameObject.name} ending patrol");

        index = -1;
        lastDest = false;

        teammateController.GoToDestination(TeammateController.Place.Workplace);
    }


    private void SetNextDestination()
    {
        // get next waypoint
        index++;
        if (index == patrolWaypoints.Length - 1)
            lastDest = true;
    
        agent.destination = patrolWaypoints[index].transform.position;
        
        // set wait time
        curWaypointIsLong = patrolWaypoints[index].GetComponent<PatrolWaypoint>().longStay;
        if (curWaypointIsLong)
            waitTimeCurWaypoint = waitTimeLongWaypoint;
        else 
            waitTimeCurWaypoint = waitTimeShortWaypoint;
    }

    private void OnDrawGizmos()
    {
        if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < patrolWaypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(patrolWaypoints[i].transform.position, patrolWaypoints[i + 1].transform.position);
        }

        foreach (GameObject waypoint in patrolWaypoints)
        {
            if (waypoint.GetComponent<PatrolWaypoint>().longStay)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawSphere(waypoint.transform.position, 0.2f);
        }
    }
}
