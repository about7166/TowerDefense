using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Basic, Fast, Heavy, Swarm, Stealth, Flying, BossSpider, None }

public class Enemy : MonoBehaviour, IDamagable
{
    public Enemy_Visuals visuals { get; private set; }

    protected ObjectPoolManager objectPool;
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected EnemyPortal myPortal;
    protected GameManager gameManager;

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private Transform centerPoint;
    public float maxHp = 100;
    public float currentHp = 4;
    protected bool isDead;

    [Header("掉落與傷害設定")]
    [Tooltip("死亡時掉落的金錢")]
    [SerializeField] private int moneyReward = 15;

    [Tooltip("碰到主堡時扣除的血量")]
    [SerializeField] private int damageToCastle = 1;

    [Header("主堡受擊音效設定")]
    [SerializeField] private AudioClip castleDamageSound;
    [Range(0f, 1f)]
    [SerializeField] private float castleDamageVolume = 1f;

    [Header("移動")]
    [SerializeField] private float turnSpeed = 10;
    [SerializeField] protected Vector3[] myWaypoints;

    [Header("護盾設定")]
    [Tooltip("設為 0 代表沒有護盾")]
    public float maxShield = 0;
    public float currentShield = 0;
    public Enemy_Shield shieldObject;

    protected int nextWaypointIndex;
    protected int currentWaypointIndex;
    protected float totalDistance;
    protected float originalSpeed;

    protected bool canBeHidden = true;
    protected bool isHidden;
    private Coroutine hideCo;
    private Coroutine disableHideCo;
    private int originalLayerIndex;

    private Coroutine activeSlowCoroutine;
    private float currentSlowMultiplier = 1f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.avoidancePriority = Mathf.RoundToInt(agent.speed * 10);

        visuals = GetComponent<Enemy_Visuals>();
        originalLayerIndex = gameObject.layer;

        gameManager = FindFirstObjectByType<GameManager>();
        originalSpeed = agent.speed;

        objectPool = ObjectPoolManager.instance;
    }

    protected virtual void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    public void SetupEnemy(EnemyPortal myNewPortal)
    {
        myPortal = myNewPortal;

        if (enemyType == EnemyType.Flying)
        {
            Transform skyEnd = myPortal.GetSkyEndpoint();
            if (skyEnd != null)
            {
                Vector3[] flyWaypoints = new Vector3[1];
                flyWaypoints[0] = skyEnd.position;
                UpdateWaypoints(flyWaypoints);
            }
            else
            {
                Debug.LogWarning("警告：忘記把 Sky_Endpoint 放進 EnemyPortal 裡了！");
            }
        }
        else
        {
            UpdateWaypoints(myPortal.currentWaypoints);
        }

        CollectTotalDistance();
        ResetEnemy();
        BeginMovement();
    }

    private void UpdateWaypoints(Vector3[] newWaypoints)
    {
        myWaypoints = new Vector3[newWaypoints.Length];

        for (int i = 0; i < myWaypoints.Length; i++)
            myWaypoints[i] = newWaypoints[i];
    }

    private void BeginMovement()
    {
        currentWaypointIndex = 0;
        nextWaypointIndex = 0;
        ChangeWaypoint();
    }

    protected void ResetEnemy()
    {
        gameObject.layer = originalLayerIndex;
        visuals.MakeTransparent(false);
        currentHp = maxHp;
        isDead = false;

        currentSlowMultiplier = 1f;
        if (activeSlowCoroutine != null)
        {
            StopCoroutine(activeSlowCoroutine);
            activeSlowCoroutine = null;
        }

        agent.speed = originalSpeed;

        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        filter.agentTypeID = agent.agentTypeID;
        filter.areaMask = NavMesh.AllAreas;

        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 5.0f, filter))
        {
            agent.enabled = false;
            transform.position = hit.position;
            agent.enabled = true;
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogError($"嚴重錯誤：怪物 {name} 在 {transform.position} 附近找不到任何專屬的 NavMesh！");
            agent.enabled = true;
        }
    }
    protected virtual void Update()
    {
        if (agent.enabled == false || agent.isOnNavMesh == false)
            return;

        FaceTarget(agent.steeringTarget);

        if (ShouldChangeWaypoint())
        {
            ChangeWaypoint();
        }
    }

    public void SlowEnemy(float newSlowMultiplier, float duration)
    {
        if (currentSlowMultiplier == 1f || newSlowMultiplier <= currentSlowMultiplier)
        {
            if (activeSlowCoroutine != null)
            {
                StopCoroutine(activeSlowCoroutine);
            }

            activeSlowCoroutine = StartCoroutine(SlowEnemyRoutine(newSlowMultiplier, duration));
        }
    }

    private IEnumerator SlowEnemyRoutine(float multiplier, float duration)
    {
        currentSlowMultiplier = multiplier;
        agent.speed = originalSpeed * multiplier;
        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
        currentSlowMultiplier = 1f;
        activeSlowCoroutine = null;
    }

    public void DisableHide(float duration)
    {
        if (disableHideCo != null)
            StopCoroutine(disableHideCo);

        disableHideCo = StartCoroutine(DisableHideCo(duration));
    }
    protected virtual IEnumerator DisableHideCo(float duration)
    {
        canBeHidden = false;

        yield return new WaitForSeconds(duration);
        canBeHidden = true;
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

    protected virtual void ChangeWaypoint()
    {
        agent.SetDestination(GetNextWaypoint());
    }

    protected virtual bool ShouldChangeWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Length)
            return false;

        if (agent.remainingDistance < 0.5f)
            return true;

        Vector3 currentWaypoint = myWaypoints[currentWaypointIndex];
        Vector3 nextWaypoint = myWaypoints[nextWaypointIndex];

        float distanceToNextWaypoint = Vector3.Distance(transform.position, nextWaypoint);
        float distanceBetweenPoint = Vector3.Distance(currentWaypoint, nextWaypoint);

        return distanceBetweenPoint > distanceToNextWaypoint;
    }

    public virtual float DistanceToFinishLine()
    {
        if (agent.enabled == false || agent.isOnNavMesh == false)
            return 9999f;

        float distToNextPoint = agent.remainingDistance;

        if (agent.pathPending || distToNextPoint == 0)
        {
            distToNextPoint = Vector3.Distance(transform.position, agent.destination);
        }

        float remainingPathDistance = 0;

        for (int i = currentWaypointIndex; i < myWaypoints.Length - 1; i++)
        {
            remainingPathDistance += Vector3.Distance(myWaypoints[i], myWaypoints[i + 1]);
        }

        return distToNextPoint + remainingPathDistance;
    }

    private void CollectTotalDistance()
    {
        for (int i = 0; i < myWaypoints.Length - 1; i++)
        {
            float distance = Vector3.Distance(myWaypoints[i], myWaypoints[i + 1]);
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
        if (myWaypoints.Length == 0)
            return transform.position;

        return myWaypoints[myWaypoints.Length - 1];
    }

    private Vector3 GetNextWaypoint()
    {
        if (nextWaypointIndex >= myWaypoints.Length)
        {
            return transform.position;
        }
        Vector3 targetPoint = myWaypoints[nextWaypointIndex];

        if (nextWaypointIndex > 0)
        {
            float distance = Vector3.Distance(myWaypoints[nextWaypointIndex], myWaypoints[nextWaypointIndex - 1]);
            totalDistance -= distance;
        }

        nextWaypointIndex++;
        currentWaypointIndex = nextWaypointIndex - 1;

        return targetPoint;
    }

    public Vector3 CenterPoint() => centerPoint.position;
    public EnemyType GetEnemyType() => enemyType;

    public virtual void TakeDamage(float damage)
    {
        if (currentShield > 0)
        {
            currentShield -= damage;

            if (shieldObject != null)
                shieldObject.ActivateShieldImpact();

            if (currentShield <= 0 && shieldObject != null)
                shieldObject.gameObject.SetActive(false);

            return;
        }

        currentHp -= damage;

        if (currentHp <= 0 && isDead == false)
        {
            isDead = true;
            Die();
        }
    }

    public virtual void Die()
    {
        if (gameManager != null)
            gameManager.UpdateCurrency(moneyReward);

        visuals.CreateOnDeathVFX();

        RemoveEnemy();
    }

    public void ReachCastleAndDealDamage()
    {
        // 防呆保護：確定有 GameManager 才扣血
        if (gameManager != null)
            gameManager.UpdateHp(-damageToCastle);
        else
            Debug.LogWarning($"警告：場景中找不到 GameManager！怪物 {gameObject.name} 無法對主堡造成傷害！");

        if (castleDamageSound != null)
        {
            AudioSource.PlayClipAtPoint(castleDamageSound, Camera.main.transform.position, castleDamageVolume);
        }

        visuals.CreateOnDeathVFX();

        // 撞到主堡後，將自己從場上移除
        RemoveEnemy();
    }

    public virtual void RemoveEnemy()
    {
        objectPool.Remove(gameObject);
        agent.enabled = false;

        if (myPortal != null)
        {
            myPortal.RemoveActiveEnemy(gameObject);
        }
    }

    protected virtual void OnEnable()
    {
        currentShield = maxShield;
        EnableShieldIfNeeded();
    }

    private void EnableShieldIfNeeded()
    {
        if (shieldObject != null && currentShield > 0)
            shieldObject.gameObject.SetActive(true);
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        CancelInvoke();
    }
}