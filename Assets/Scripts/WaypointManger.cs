using UnityEngine;

public class WaypointManger : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;

    public Transform[] GetWayPoints() => waypoints;
}
