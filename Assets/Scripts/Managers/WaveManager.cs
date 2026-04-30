using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public class WaveDetails
{
    public GridBuilder nextGrid;
    public EnemyPortal[] newPortals;
    public int basicEnemy;
    public int basicEnemy_2;
    public int basicEnemy_3;
    public int fastEnemy;
    public int fastEnemy_2;
    public int fastEnemy_3;
    public int heavyEnemy;
    public int heavyEnemy_2;
    public int heavyEnemy_3;
    public int swarmEnemy;
    public int stealthEnemy;
    public int flyingEnemy;
    public int flyingEnemy_2;
    public int flyingEnemy_3;
    public int flyingBossEnemy;
    public int spiderBossEnemy;
}

public class WaveManager : MonoBehaviour
{
    private GameManager gameManager;
    private TileAnimator tileAnimator;
    private UI_InGame inGameUI;
    [SerializeField] private GridBuilder currentGrid;
    [SerializeField] private NavMeshSurface droneNavSurface;
    [SerializeField] private NavMeshSurface flyingNavSurface;
    [SerializeField] private MeshCollider[] flyingNavMeshColliders;


    [Header("波次設定")]
    [SerializeField] private float timeBetweenWaves = 10;
    [SerializeField] private float waveTimer;
    [SerializeField] private WaveDetails[] levelWaves;
    [SerializeField] private int waveIndex;

    [Header("提升階段設定")]
    [SerializeField] private float yOffset = 5;
    [SerializeField] private float tileDelay = 0.1f;

    [Header("怪物的Prefabs")]
    [SerializeField] private GameObject basicEnemy;
    [SerializeField] private GameObject basicEnemy_2;
    [SerializeField] private GameObject basicEnemy_3;
    [SerializeField] private GameObject fastEnemy;
    [SerializeField] private GameObject fastEnemy_2;
    [SerializeField] private GameObject fastEnemy_3;
    [SerializeField] private GameObject heavyEnemy;
    [SerializeField] private GameObject heavyEnemy_2;
    [SerializeField] private GameObject heavyEnemy_3;
    [SerializeField] private GameObject swarmEnemy;
    [SerializeField] private GameObject stealthEnemy;
    [SerializeField] private GameObject flyingEnemy;
    [SerializeField] private GameObject flyingEnemy_2;
    [SerializeField] private GameObject flyingEnemy_3;
    [SerializeField] private GameObject flyingBossEnemy;
    [SerializeField] private GameObject spiderBossEnemy;

    private List<EnemyPortal> enemyPortals;
    private bool waveTimerEnabled;
    private bool makingNextWave;
    public bool gameBegun;

    private void Awake()
    {
        enemyPortals = new List<EnemyPortal>(FindObjectsOfType<EnemyPortal>());

        gameManager = FindFirstObjectByType<GameManager>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        inGameUI = FindFirstObjectByType<UI_InGame>(FindObjectsInactive.Include);

        flyingNavMeshColliders = GetComponentsInChildren<MeshCollider>();
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            ActivateWaveManager();

        if (gameBegun == false)
            return;

        HandleWaveTimer();
    }

    [ContextMenu("啟動波次管理")]
    public void ActivateWaveManager()
    {
        gameBegun = true;
        inGameUI = gameManager.inGameUI;
        EnableWaveTimer(true);
    }

    public void DeactivateWaveManager() => gameBegun = false;

    public void CheckIfWaveCompleted()
    {
        if (gameBegun == false)
            return;

        if (AllEnemiesDefeated() == false || makingNextWave)
            return;

        makingNextWave = true;
        waveIndex++;

        if (HasNoMoreWaves())
        {
            gameManager.LevelCompleted();
            return;
        }

        if (HasNewLayout())
            AttemptToUpdateLayout();
        else
            EnableWaveTimer(true);
    }

    public void StartNewWave()
    {
        UpdateNavMeshes();
        //currentGrid.DisableShadowsIfNeeded();
        GiveEnemiesToPortals();
        EnableWaveTimer(false);
        makingNextWave = false;
    }
    private void HandleWaveTimer()
    {
        if (waveTimerEnabled == false)
            return;

        waveTimer -= Time.deltaTime;
        inGameUI.UpdateWaveTimerUI(waveTimer);

        if (waveTimer <= 0)
            StartNewWave();
    }
    private void GiveEnemiesToPortals()
    {
        List<GameObject> newEnemies = GetNewEnemies();
        int portalIndex = 0;

        if (newEnemies == null)
        {
            Debug.LogWarning("沒有下一波");
            return;
        }

        for (int i = 0; i < newEnemies.Count; i++)
        {
            GameObject enemyToAdd = newEnemies[i];
            EnemyPortal portalToReciveEnemy = enemyPortals[portalIndex];

            portalToReciveEnemy.AddEnemy(enemyToAdd);

            portalIndex++;

            if (portalIndex >= enemyPortals.Count)
                portalIndex = 0;
        }
    }

    private void AttemptToUpdateLayout() => UpdateLevelLayout(levelWaves[waveIndex]);
    private void UpdateLevelLayout(WaveDetails nextWave)
    {
        GridBuilder nextGrid = nextWave.nextGrid;
        List<GameObject> grid = currentGrid.GetTileSetup();
        List<GameObject> newGrid = nextGrid.GetTileSetup();

        if (grid.Count != newGrid.Count)
        {
            Debug.LogWarning("當前地塊和新地塊尺寸不一");
            return;
        }

        List<TileSlot> tilesToRemove = new List<TileSlot>();
        List<TileSlot> tilesToAdd = new List<TileSlot>();

        for (int i = 0; i < grid.Count; i++)
        {
            TileSlot currentTile = grid[i].GetComponent<TileSlot>();
            TileSlot referenceNewTile = newGrid[i].GetComponent<TileSlot>();

            bool shouldBeUpdated = currentTile.GetMesh() != referenceNewTile.GetMesh() ||
                                   currentTile.GetOriginalMaterial() != referenceNewTile.GetOriginalMaterial() ||
                                   currentTile.GetAllChildren().Count != referenceNewTile.GetAllChildren().Count ||
                                   currentTile.transform.rotation != referenceNewTile.transform.rotation;

            if (shouldBeUpdated)
            {
                tilesToRemove.Add(currentTile);

                // ============ 👇 修改這段 👇 ============

                // 1. 先抓取舊地塊的位置 (保留 X 和 Z)
                Vector3 correctPosition = currentTile.transform.position;

                // 2. 🚀 關鍵：把 Y 軸高度替換成「新地塊」原本設定好的高度！
                correctPosition.y = referenceNewTile.transform.position.y;

                // 3. 在這個融合了新舊座標的精準位置上生成地塊
                GameObject spawnedNewTile = Instantiate(referenceNewTile.gameObject, correctPosition, referenceNewTile.transform.rotation);

                // ============ 👆 修改結束 👆 ============

                TileSlot actualNewTile = spawnedNewTile.GetComponent<TileSlot>();

                // 讓新地塊一開始是隱藏的
                spawnedNewTile.SetActive(false);

                tilesToAdd.Add(actualNewTile);

                // 將新地塊更新到目前的陣列中
                grid[i] = spawnedNewTile;
            }
        }

        StartCoroutine(RebuildLevelCo(tilesToRemove, tilesToAdd, nextWave, tileDelay));
    }

    private IEnumerator RebuildLevelCo(List<TileSlot> tilesToRemove, List<TileSlot> tilesToAdd, WaveDetails waveDetails, float delay)
    {
        for (int i = 0; i < tilesToRemove.Count; i++)
        {
            yield return new WaitForSeconds(delay);
            RemoveTile(tilesToRemove[i]);
        }

        for (int i = 0; i < tilesToAdd.Count; i++)
        {
            yield return new WaitForSeconds(delay);
            AddTile(tilesToAdd[i]);
        }

        EnableNewPortals(waveDetails.newPortals);
        EnableWaveTimer(true);
    }

    private void AddTile(TileSlot newTile)
    {
        newTile.transform.parent = currentGrid.transform;
        Vector3 targetPosition = newTile.transform.position;
        newTile.transform.position = targetPosition + new Vector3(0, -yOffset, 0);
        newTile.gameObject.SetActive(true);

        // 🚀 新增這段：如果這個新地塊是 Can Build，告訴它正確的預設位置是 targetPosition！
        BuildSlot buildSlot = newTile.GetComponent<BuildSlot>();
        if (buildSlot != null)
        {
            buildSlot.UpdateDefaultPosition(targetPosition);
        }

        tileAnimator.DissolveTile(true, newTile.transform);
        tileAnimator.MoveTile(newTile.transform, targetPosition, true);
    }

    private void RemoveTile(TileSlot tileToRemove)
    {
        Vector3 targetPosition = tileToRemove.transform.position + new Vector3(0, -yOffset, 0);

        tileAnimator.DissolveTile(false, tileToRemove.transform);
        tileAnimator.MoveTile(tileToRemove.transform, targetPosition, false);

        Destroy(tileToRemove.gameObject, 3);
    }

    private void EnableWaveTimer(bool enable)
    {
        if (waveTimerEnabled == enable)
            return;

        waveTimer = timeBetweenWaves;
        waveTimerEnabled = enable;
        inGameUI.EnableWaveTimer(enable);
    }

    private void EnableNewPortals(EnemyPortal[] newPortals)
    {
        foreach (EnemyPortal portal in newPortals)
        {
            portal.CanCreateNewEnemies(true);
            portal.AssignWaveManager(this);
            portal.gameObject.SetActive(true);
            enemyPortals.Add(portal);
        }
    }

    private void UpdateNavMeshes()
    {
        foreach (var collider in flyingNavMeshColliders)
        {
            collider.enabled = true;
        }

        flyingNavSurface.BuildNavMesh();

        foreach (var collider in flyingNavMeshColliders)
        {
            collider.enabled = false;
        }

        currentGrid.UpdateNavMesh();
        droneNavSurface.BuildNavMesh();
    }

    public void UpdateDroneNavMesh() => droneNavSurface.BuildNavMesh();

    private List<GameObject> GetNewEnemies()
    {
        if (waveIndex >= levelWaves.Length)
        {
            return null;
        }

        List<GameObject> newEnemyList = new List<GameObject>();

        for (int i = 0; i < levelWaves[waveIndex].basicEnemy; i++)
        {
            newEnemyList.Add(basicEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].basicEnemy_2; i++)
        {
            newEnemyList.Add(basicEnemy_2);
        }

        for (int i = 0; i < levelWaves[waveIndex].basicEnemy_3; i++)
        {
            newEnemyList.Add(basicEnemy_3);
        }

        for (int i = 0; i < levelWaves[waveIndex].fastEnemy; i++)
        {
            newEnemyList.Add(fastEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].fastEnemy_2; i++)
        {
            newEnemyList.Add(fastEnemy_2);
        }

        for (int i = 0; i < levelWaves[waveIndex].fastEnemy_3; i++)
        {
            newEnemyList.Add(fastEnemy_3);
        }

        for (int i = 0; i < levelWaves[waveIndex].heavyEnemy; i++)
        {
            newEnemyList.Add(heavyEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].heavyEnemy_2; i++)
        {
            newEnemyList.Add(heavyEnemy_2);
        }

        for (int i = 0; i < levelWaves[waveIndex].heavyEnemy_3; i++)
        {
            newEnemyList.Add(heavyEnemy_3);
        }

        for (int i = 0; i < levelWaves[waveIndex].swarmEnemy; i++)
        {
            newEnemyList.Add(swarmEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].stealthEnemy; i++)
        {
            newEnemyList.Add(stealthEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].flyingEnemy; i++)
        {
            newEnemyList.Add(flyingEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].flyingEnemy_2; i++)
        {
            newEnemyList.Add(flyingEnemy_2);
        }

        for (int i = 0; i < levelWaves[waveIndex].flyingEnemy_3; i++)
        {
            newEnemyList.Add(flyingEnemy_3);
        }

        for (int i = 0; i < levelWaves[waveIndex].flyingBossEnemy; i++)
        {
            newEnemyList.Add(flyingBossEnemy);
        }

        for (int i = 0; i < levelWaves[waveIndex].spiderBossEnemy; i++)
        {
            newEnemyList.Add(spiderBossEnemy);
        }

        return newEnemyList;
    }
    public WaveDetails[] GetLevelWaves() => levelWaves;
    private bool AllEnemiesDefeated()
    {
        foreach (EnemyPortal portal in enemyPortals)
        {
            // 修改重點：
            // 如果場上還有活著的怪 (GetActiveEnemies().Count > 0)
            // 或者 (||)
            // 傳送門還有怪沒生出來 (portal.HasEnemiesToSpawn())
            // 就不算贏！
            if (portal.GetActiveEnemies().Count > 0 || portal.HasEnemiesToSpawn())
                return false;
        }

        return true;
    }

    private bool HasNewLayout() => waveIndex < levelWaves.Length && levelWaves[waveIndex].nextGrid != null;
    private bool HasNoMoreWaves() => waveIndex >= levelWaves.Length;

    // =========================================================
    // ★ 編輯器自動化工具：一鍵將未來會變動的地塊轉為 NoBuild ★
    // =========================================================
    [ContextMenu("一鍵標記！預計變動的地塊自動轉為 NoBuild")]
    private void AutoMarkChangingTilesAsNoBuild()
    {
        if (currentGrid == null || levelWaves == null || levelWaves.Length == 0)
        {
            Debug.LogWarning("請先設定好 Current Grid 和 Level Waves！");
            return;
        }

        TileSetHolder tileSet = FindFirstObjectByType<TileSetHolder>();
        if (tileSet == null || tileSet.tileNoBuild == null)
        {
            Debug.LogWarning("找不到 TileSetHolder 或尚未設定 Tile No Build！");
            return;
        }

        List<GameObject> currentTiles = currentGrid.GetTileSetup();
        int changedCount = 0;

        // 檢查每一波的未來地圖
        foreach (WaveDetails wave in levelWaves)
        {
            if (wave.nextGrid == null) continue;

            List<GameObject> nextTiles = wave.nextGrid.GetTileSetup();

            if (currentTiles.Count != nextTiles.Count)
            {
                Debug.LogWarning("警告：未來的地塊數量與現在的地塊數量不一致！");
                continue;
            }

            // 逐一比對每個地塊
            for (int i = 0; i < currentTiles.Count; i++)
            {
                TileSlot currentTile = currentTiles[i].GetComponent<TileSlot>();
                TileSlot newTile = nextTiles[i].GetComponent<TileSlot>();

                // 判斷是否和未來的地塊不一樣
                bool willChange = currentTile.GetMesh() != newTile.GetMesh() ||
                                  currentTile.GetOriginalMaterial() != newTile.GetOriginalMaterial() ||
                                  currentTile.GetAllChildren().Count != newTile.GetAllChildren().Count ||
                                  currentTile.transform.rotation != newTile.transform.rotation;

                // 如果未來會變動，且現在還不是 NoBuild 地塊，就自動幫它換成 NoBuild
                if (willChange && currentTile.gameObject.name != tileSet.tileNoBuild.name)
                {
                    currentTile.SwitchTile(tileSet.tileNoBuild);
                    changedCount++;
                }
            }
        }

        Debug.Log($"✅ 自動標記完成！共將 {changedCount} 個預計變動的地塊轉換為 NoBuild。");
    }
}
