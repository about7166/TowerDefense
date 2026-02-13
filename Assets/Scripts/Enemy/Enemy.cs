using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType { Basic, Fast, Heavy, Swarm, Stealth, Flying, BossSpider,None}

public class Enemy : MonoBehaviour , IDamagable
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

    }

    public void SetupEnemy(EnemyPortal myNewPortal)
    {
        myPortal = myNewPortal;

        UpdateWaypoints(myPortal.currentWaypoints);
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

        agent.speed = originalSpeed;
        agent.enabled = true;
    }

    protected virtual void Update()
    {        
        FaceTarget(agent.steeringTarget);

        if (ShouldChangeWaypoint())
        {
            ChangeWaypoint();
        }
    }

    public void SlowEnemy(float slowMultiplier, float duration) => StartCoroutine(SlowEnemyCo(slowMultiplier, duration));

    private IEnumerator SlowEnemyCo(float slowMultiplier, float duration)
    {
        agent.speed = originalSpeed;
        agent.speed = agent.speed * slowMultiplier;

        yield return new WaitForSeconds(duration);

        agent.speed = originalSpeed;
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
        // 1. 怪物走到「目前鎖定的目標點」的距離
        float distToNextPoint = agent.remainingDistance;

        // 防呆：如果導航還沒算好路徑
        if (agent.pathPending || distToNextPoint == 0)
        {
            distToNextPoint = Vector3.Distance(transform.position, agent.destination);
        }

        // 2. ★ 核心修改：即時計算後續所有未走路段的總和 ★
        float remainingPathDistance = 0;

        // 從怪物目前正在前往的點 (currentWaypointIndex) 開始，一路加到最後一個點
        for (int i = currentWaypointIndex; i < myWaypoints.Length - 1; i++)
        {
            remainingPathDistance += Vector3.Distance(myWaypoints[i], myWaypoints[i + 1]);
        }

        // 3. 兩者相加，這就是怪物距離主堡最真實、絕對不會出錯的距離！
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
            //waypointIndex = 0;
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
        // 先檢查有沒有護盾
        if (currentShield > 0)
        {
            currentShield -= damage;

            // 播放護盾被打到的特效
            if (shieldObject != null)
                shieldObject.ActivateShieldImpact();

            // 護盾破裂
            if (currentShield <= 0 && shieldObject != null)
                shieldObject.gameObject.SetActive(false);

            return; // 護盾把這次傷害吸收了，不扣血
        }

        // 如果沒有護盾，或是護盾已經破了，就扣真實血量
        currentHp -= damage;

        if (currentHp <= 0 && isDead == false)
        {
            isDead = true;
            Die();
        }
    }

    public virtual void Die()
    {
        gameManager.UpdateCurrency(moneyReward);
        RemoveEnemy();
    }

    // 當敵人抵達終點 (或撞到主堡) 時呼叫這個方法
    public void ReachCastleAndDealDamage()
    {
        // 傳入負的 damageToCastle 來扣主堡血量
        gameManager.UpdateHp(-damageToCastle);

        // 撞到主堡後，將自己從場上移除
        RemoveEnemy();
    }

    public virtual void RemoveEnemy()
    {
        visuals.CreateOnDeathVFX();
        objectPool.Remove(gameObject);
        agent.enabled = false;

        if (myPortal != null)
        {
            myPortal.RemoveActiveEnemy(gameObject);
        }
    }

    protected virtual void OnEnable()
    {
        // 每次從物件池拿出來時，重置護盾
        currentShield = maxShield;
        EnableShieldIfNeeded();
    }

    private void EnableShieldIfNeeded()
    {
        // 如果有設定護盾模型，且最大護盾值大於0，就顯示護盾
        if (shieldObject != null && currentShield > 0)
            shieldObject.gameObject.SetActive(true);
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        CancelInvoke();
    }
}
