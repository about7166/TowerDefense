using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPortal : MonoBehaviour
{
    private ObjectPoolManager objectPool;

    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private float spawnCooldown;
    private float spawnTimer;
    private bool canCreateEnemies = true;

    [Space]

    [SerializeField] private ParticleSystem flyPortalFx;
    private Coroutine flyPortalFxCo;

    [Space]

    [SerializeField] private List<Waypoint> waypointList;
    public Vector3[] currentWaypoints {  get; private set; }

    private List<GameObject> enemiesToCreate = new List<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    [Space]
    // ★ 新增：天空航線的終點站
    [SerializeField] private Transform skyEndpoint;
    public Transform GetSkyEndpoint() => skyEndpoint;

    private void Awake()
    {
        CollectWaypoints();

        //if (myWaveManager == null)
        //    myWaveManager = FindFirstObjectByType<LevelSetup>().GetWaveManager();
    }

    private void Start()
    {
        objectPool = ObjectPoolManager.instance;
    }

    private void Update()
    {
        if (CanMakeNewEnemy())
            CreateEnemy();
    }

    public void AssignWaveManager(WaveManager newWaveManager) => myWaveManager = newWaveManager;
    private bool CanMakeNewEnemy()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0 && enemiesToCreate.Count > 0)
        {
            spawnTimer = spawnCooldown;
            return true;
        }

        return false;
    }

    private void CreateEnemy()
    {
        if (canCreateEnemies == false)
            return;

        GameObject randomEnemy = GetRandomEnemy();
        GameObject newEnemy = objectPool.Get(randomEnemy, transform.position, Quaternion.identity);

        Enemy enemyScript = newEnemy.GetComponent<Enemy>();

        // ★ 修正：先將怪物移動到空中的飛行傳送門
        PlaceEnemyAtFlyPortalIfNeeded(newEnemy, enemyScript.GetEnemyType());

        // ★ 然後才呼叫 SetupEnemy 喚醒導航大腦 (這樣牠醒來時，腳下就會是天空網格了)
        enemyScript.SetupEnemy(this);

        activeEnemies.Add(newEnemy);
    }

    private void PlaceEnemyAtFlyPortalIfNeeded(GameObject newEnemy, EnemyType enemyType)
    {
        if (enemyType != EnemyType.Flying)
            return;

        if (flyPortalFxCo != null)
            StopCoroutine(flyPortalFxCo);

        flyPortalFxCo = StartCoroutine(EnableFlyPortalFxCo());
        newEnemy.transform.position = flyPortalFx.transform.position;
    }

    private IEnumerator EnableFlyPortalFxCo()
    {
        flyPortalFx.Play();

        yield return new WaitForSeconds(2);

        flyPortalFx.Stop();
    }

    private GameObject GetRandomEnemy()
    {
        int randomIndex = Random.Range(0, enemiesToCreate.Count);
        GameObject choosenEnemy = enemiesToCreate[randomIndex];

        enemiesToCreate.Remove(choosenEnemy);

        return choosenEnemy;
    }

    public void AddEnemy(GameObject enemyToAdd) => enemiesToCreate.Add(enemyToAdd);

    public void RemoveActiveEnemy(GameObject enemyToRemove)
    {
        if(activeEnemies.Contains(enemyToRemove))
            activeEnemies.Remove(enemyToRemove);

        myWaveManager.CheckIfWaveCompleted();
    }
    public List<GameObject> GetActiveEnemies() => activeEnemies;
    public void CanCreateNewEnemies(bool canCreate) => canCreateEnemies = canCreate;

    [ContextMenu("收集怪物路徑")]
    private void CollectWaypoints()
    {
        waypointList = new List<Waypoint>();

        foreach (Transform child in transform)
        {
            Waypoint waypoint = child.GetComponent<Waypoint>();

            if (waypoint != null)
                waypointList.Add(waypoint);
        }

        currentWaypoints = new Vector3[waypointList.Count];

        for (int i = 0; i < currentWaypoints.Length; i++)
        {
            currentWaypoints[i] = waypointList[i].transform.position;
        }
    }

    // 新增這個方法：告訴外面還有多少怪在排隊
    public bool HasEnemiesToSpawn() => enemiesToCreate.Count > 0;
}
