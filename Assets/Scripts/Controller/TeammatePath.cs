using System;
using UnityEngine;

public class TeammatePath : MonoBehaviour
{
    public GameObject[] teammatePathWaypoints;
    private int index = -1;

    public GameObject GetNextWaypoint()
    {
        index++;
        index %= teammatePathWaypoints.Length;

        return teammatePathWaypoints[index];
    }

    private void OnDrawGizmos()
    {
        if (teammatePathWaypoints == null || teammatePathWaypoints.Length == 0) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < teammatePathWaypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(teammatePathWaypoints[i].transform.position, teammatePathWaypoints[i + 1].transform.position);
        }

        Gizmos.DrawLine(teammatePathWaypoints[^1].transform.position, teammatePathWaypoints[0].transform.position);

        foreach (GameObject waypoint in teammatePathWaypoints)
        {
            if (waypoint.GetComponent<TeammateWaypoint>().isTable)
                Gizmos.color = Color.blue;
            else
                Gizmos.color = Color.green;

            Gizmos.DrawSphere(waypoint.transform.position, 0.2f);

        }
    }
}
