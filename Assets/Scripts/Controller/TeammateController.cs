using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TeammateController : MonoBehaviour
{
    public enum TeammateState
    {
        Meeting,
        AtTable,
        Sleeping,
        Shitting,
        Patrolling
    }
    public TeammateState initialTeammateState;
    public TeammateState curTeammateState;

    public float drowsiness;

    public TeammatePath teammatePath;
    private NavMeshAgent agent;
    public float walkSpeed;  // TODO: scale with drowsiness & set in code
    private float time = 0f;
    
    public float waitTimeTableWaypoint = 10f;
    public float waitTimeWalkingWaypoint = 3f;
    private float waitTimeCurWaypoint;
    private bool curWaypointIsTable;

    public GameObject player;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        curTeammateState = initialTeammateState;

        SetDestination(teammatePath.GetNextWaypoint());
    }

    private void Update()
    {
        // Agent arrived at cur dest
        if (agent.remainingDistance <= 0.1f)
        {
            if (curWaypointIsTable && curTeammateState != TeammateState.AtTable)
                curTeammateState = TeammateState.AtTable;
            else if (!curWaypointIsTable && curTeammateState != TeammateState.Patrolling)
                curTeammateState = TeammateState.Patrolling;

            time += Time.deltaTime;
            if (time >= waitTimeCurWaypoint)
            {
                time = 0f;
                SetDestination(teammatePath.GetNextWaypoint());
            }
        }
    }

    private void SetDestination(GameObject dest)
    {
        agent.destination = dest.transform.position;
        
        curWaypointIsTable = dest.GetComponent<TeammateWaypoint>().isTable;
        if (curWaypointIsTable)
            waitTimeCurWaypoint = waitTimeTableWaypoint;
        else 
            waitTimeCurWaypoint = waitTimeWalkingWaypoint;
    }
}
