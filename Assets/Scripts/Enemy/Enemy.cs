using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Basic, Fast, Heavy, Swarm, Stealth, Flying,None}

public class Enemy : MonoBehaviour , IDamagable
{
    public Enemy_Visuals visuals { get; private set; }
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected EnemyPortal myPortal;
    private GameManager gameManager;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centerPoint;
    public int healthPoint = 4;

    [Header("移動")]
    [SerializeField] private float turnSpeed = 10;
    [SerializeField] private List<Transform> myWaypoints;
    private int nextWaypointIndex;
    private int currentWaypointIndex;

    private float totalDistance;

    protected bool canBeHidden = true;
    protected bool isHidden;
    private Coroutine hideCo;
    private int originalLayerIndex;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);

        visuals = GetComponent<Enemy_Visuals>();
        originalLayerIndex = gameObject.layer;

        gameManager = FindFirstObjectByType<GameManager>();
    }

    protected virtual void Start()
    {

    }

    public void SetupEnemy(List<Waypoint> newWaypoints,EnemyPortal myNewPortal)
    {
        myWaypoints = new List<Transform>();

        foreach (var point in newWaypoints)
        {
            myWaypoints.Add(point.transform);
        }

        CollectTotalDistance();

        myPortal = myNewPortal;
    }

    protected virtual void Update()
    {        
        FaceTarget(agent.steeringTarget);

        if (ShouldChangeWaypoint())
        {
            agent.SetDestination(GetNextWaypoint());
        }
    }

    public void HideEnemy(float duration)
    {
        if (canBeHidden == false)
            return;

        if (hideCo != null)
            StopCoroutine(hideCo);

        hideCo = StartCoroutine(HideEnemyCo(duration));
    }

    private IEnumerator HideEnemyCo(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Untargetable");
        visuals.MakeTransparent(true);
        isHidden = true;

        yield return new WaitForSeconds(duration);

        gameObject.layer = originalLayerIndex;
        visuals.MakeTransparent(false);
        isHidden = false;
    }

    private bool ShouldChangeWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Count)
            return false;

        if (agent.remainingDistance < 0.5f)
            return true;

        Vector3 currentWaypoint = myWaypoints[currentWaypointIndex].position;
        Vector3 nextWaypoint = myWaypoints[nextWaypointIndex].position;

        float distanceToNextWaypoint = Vector3.Distance(transform.position, nextWaypoint);
        float distanceBetweenPoint = Vector3.Distance(currentWaypoint, nextWaypoint);

        return distanceBetweenPoint > distanceToNextWaypoint;
    }

    public float DistanceToFinishLine() => totalDistance + agent.remainingDistance;

    private void CollectTotalDistance()
    {
        for (int i = 0; i < myWaypoints.Count - 1; i++)
        {
            float distance = Vector3.Distance(myWaypoints[i].position, myWaypoints[i + 1].position);
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
    
    protected Vector3 GetFinalWaypoint()
    {
        if (myWaypoints.Count == 0)
            return transform.position;

        return myWaypoints[myWaypoints.Count - 1].position;
    }

    private Vector3 GetNextWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Count)
        {
            //waypointIndex = 0;
            return transform.position;
        }
        Vector3 targetPoint = myWaypoints[nextWaypointIndex].position;

        if (nextWaypointIndex > 0)
        {
            float distance = Vector3.Distance(myWaypoints[nextWaypointIndex].position, myWaypoints[nextWaypointIndex - 1].position);
            totalDistance -= distance;
        }

        nextWaypointIndex++;
        currentWaypointIndex = nextWaypointIndex - 1;

        return targetPoint;
    }

    public Vector3 CenterPoint() => centerPoint.position;
    public EnemyType GetEnemyType() => enemyType;
    
    public void TakeDamage(int damage)
    {
        healthPoint = healthPoint - damage;
        if (healthPoint <= 0)
            Die();
    }

    public virtual void Die()
    {
        myPortal.RemoveActiveEnemy(gameObject);
        gameManager.UpdateCurrency(1);//在GameManger裡的UpdateCurrency 用來做殺掉怪的掉落
        Destroy(gameObject);
    }

    public void DestroyEnemy()
    {
        myPortal.RemoveActiveEnemy(gameObject);
        Destroy(gameObject);
    }
}
