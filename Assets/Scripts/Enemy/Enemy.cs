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

    // ★ 新增：用來記錄目前的緩速狀態
    private Coroutine activeSlowCoroutine;
    private float currentSlowMultiplier = 1f; // 1 代表沒有被緩速

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

        // ★ 修正：判斷怪物類型，給予不同的導航目標
        if (enemyType == EnemyType.Flying)
        {
            Transform skyEnd = myPortal.GetSkyEndpoint();
            if (skyEnd != null)
            {
                // 如果是飛行怪，只給牠一個目標：天空跑道的終點！NavMesh 會自動幫牠沿著跑道走。
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
            // 如果是地面怪物，照常讀取地面的多個彎曲 Waypoints
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

        // ============ 👇 修改重點 1：精準尋找專屬網格 👇 ============
        // 建立過濾器，告訴系統：「我是誰(飛行或地面)，就只找我能走的路！」
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        filter.agentTypeID = agent.agentTypeID; // ★ 關鍵：綁定怪物的專屬 Agent Type
        filter.areaMask = NavMesh.AllAreas;

        // 把搜尋半徑加大到 5.0f，容錯率更高
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 5.0f, filter))
        {
            agent.enabled = false;
            transform.position = hit.position; // 吸附到精準座標
            agent.enabled = true;
            agent.Warp(hit.position); // 雙重保險：強制更新內部座標
        }
        else
        {
            Debug.LogError($"嚴重錯誤：怪物 {name} 在 {transform.position} 附近找不到任何專屬的 NavMesh！");
            agent.enabled = true;
        }
        // ============ 👆 修改結束 👆 ============
    }
    protected virtual void Update()
    {
        // ============ 👇 修改重點 2：保險絲 👇 ============
        // 如果大腦還沒開啟，或是沒有踩在網格上，就絕對不要往下執行 (根絕紅字)
        if (agent.enabled == false || agent.isOnNavMesh == false)
            return;
        // ===============================================

        FaceTarget(agent.steeringTarget);

        if (ShouldChangeWaypoint())
        {
            ChangeWaypoint();
        }
    }

    public void SlowEnemy(float newSlowMultiplier, float duration)
    {
        // 💡 邏輯判斷：數字越小代表緩速越強 (例如 0.2 的緩速效果大於 0.8)
        // 只有在「目前沒被緩速(1f)」或「新緩速比舊緩速強(或一樣強)」時，才套用新緩速
        if (currentSlowMultiplier == 1f || newSlowMultiplier <= currentSlowMultiplier)
        {
            // 如果身上已經有舊的緩速協程，先把它停掉，確保不會疊加衝突
            if (activeSlowCoroutine != null)
            {
                StopCoroutine(activeSlowCoroutine);
            }

            // 啟動新的、最強的緩速
            activeSlowCoroutine = StartCoroutine(SlowEnemyRoutine(newSlowMultiplier, duration));
        }
    }

    private IEnumerator SlowEnemyRoutine(float multiplier, float duration)
    {
        // 紀錄目前正在運作的緩速強度
        currentSlowMultiplier = multiplier;

        // 套用速度
        agent.speed = originalSpeed * multiplier;

        // 等待持續時間結束
        yield return new WaitForSeconds(duration);

        // 緩速結束，恢復正常狀態
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
        // ★ 加入保護：如果沒在網格上，直接回傳最大值，當作牠還很遠
        if (agent.enabled == false || agent.isOnNavMesh == false)
            return 9999f;

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
