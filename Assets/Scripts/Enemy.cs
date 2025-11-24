using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Basic, Fast, None}

public class Enemy : MonoBehaviour , IDamagable
{
    private NavMeshAgent agent;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centerPoint;
    public int healthPoint = 4;

    [Header("²¾°Ê")]
    [SerializeField] private float turnSpeed = 10;
    [SerializeField] private Transform[] waypoints;
    private int waypointIndex;

    private float totalDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);
    }

    private void Start()
    {
        waypoints = FindFirstObjectByType<WaypointManger>().GetWayPoints();

        CollectTotalDistance();
    }

    
    private void Update()
    {        
        FaceTarget(agent.steeringTarget);

        if (agent.remainingDistance < 0.5f)
        {
            agent.SetDestination(GetNextWaypoint());
        }
    }

    public float DistanceToFinishLine() => totalDistance + agent.remainingDistance;

    private void CollectTotalDistance()
    {
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            float distance = Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            totalDistance = totalDistance + distance;
        }
    }

    private void FaceTarget(Vector3 newTarget)
    {
        Vector3 diractionToTarget = newTarget - transform.position;
        diractionToTarget.y = 0;

        Quaternion newRotation = Quaternion.LookRotation(diractionToTarget);

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, turnSpeed * Time.deltaTime);
    }
    

    private Vector3 GetNextWaypoint()
    {
        if (waypointIndex >= waypoints.Length)
        {
            //waypointIndex = 0;
            return transform.position;
        }
        Vector3 targetPoint = waypoints[waypointIndex].position;

        if (waypointIndex > 0)
        {
            float distance = Vector3.Distance(waypoints[waypointIndex].position, waypoints[waypointIndex - 1].position);
            totalDistance -= distance;
        }

        waypointIndex++;

        return targetPoint;
    }

    public Vector3 CenterPoint() => centerPoint.position;
    public EnemyType GetEnemyType() => enemyType;
    
    public void TakeDamage(int damage)
    {
        healthPoint = healthPoint - damage;
        if (healthPoint <= 0)
            Destroy(gameObject);
    }
}
