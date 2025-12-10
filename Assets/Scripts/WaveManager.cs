using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveDetails
{
    public GridBuilder nextGrid;
    public EnemyPortal[] newPortals;
    public int basicEnemy;
    public int fastEnemy;
}

public class WaveManager : MonoBehaviour
{
    private UI_InGame inGameUI;

    [SerializeField] private GridBuilder currentGrid;
    public bool waveCompleted;

    [Header("波次設定")]
    public float timeBetweenWaves = 10;
    public float waveTimer;

    [SerializeField] private WaveDetails[] levelWaves;
    private int waveIndex;

    private float checkTinterval = 0.5f;
    private float nextCheckTime;

    [Header("怪物的Prefabs")]
    [SerializeField] private GameObject basicEnemy;
    [SerializeField] private GameObject fastEnemy;

    private List<EnemyPortal> enemyPortals;

    private void Awake()
    {
        enemyPortals = new List<EnemyPortal>(FindObjectsOfType<EnemyPortal>());
        inGameUI = FindFirstObjectByType<UI_InGame>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        SetupNextWave();
    }

    private void Update()
    {
        HandleWaveCompletion();
        HandleWaveTiming();
    }

    public WaveDetails[] GetLevelWaves() => levelWaves;

    private void HandleWaveCompletion()
    {
        if (ReadyToCheck() == false)
            return;

        if (waveCompleted == false && AllEnemiesDefeatrd())
        {
            CheckForNewLevelLayout();

            waveCompleted = true;
            waveTimer = timeBetweenWaves;
            inGameUI.EnableWaveTimer(true);
        }
    }

    private void HandleWaveTiming()
    {
        if (waveCompleted)
        {
            waveTimer -= Time.deltaTime;
            inGameUI.UpdateWaveTimerUI(waveTimer);

            if (waveTimer <= 0)
            {
                inGameUI.EnableWaveTimer(false);
                SetupNextWave();
            }
        }
    }

    public void ForceNextWave()
    {
        if (AllEnemiesDefeatrd() == false)
        {
            return;
        }

        inGameUI.EnableWaveTimer(false);
        SetupNextWave();
    }

    [ContextMenu("下一波設定")]

    private void SetupNextWave()
    {
        List<GameObject> newEnemies = NewEnemyWave();
        int portalIndwx = 0;

        if (newEnemies == null)
            return;

        for (int i = 0; i < newEnemies.Count; i++)
        {
            GameObject enemyToAdd = newEnemies[i];
            EnemyPortal portalToReciveEnemy = enemyPortals[portalIndwx];

            portalToReciveEnemy.AddEnemy(enemyToAdd);

            portalIndwx++;

            if (portalIndwx >= enemyPortals.Count)
                portalIndwx = 0;
        }

        waveCompleted = false;
    }

    private List<GameObject> NewEnemyWave()
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

        for (int i = 0; i < levelWaves[waveIndex].fastEnemy; i++)
        {
            newEnemyList.Add(fastEnemy);
        }

        waveIndex++;

        return newEnemyList;
    }

    private void CheckForNewLevelLayout()
    {
        if (waveIndex >= levelWaves.Length)
            return;

        WaveDetails nextWave = levelWaves[waveIndex];

        if (nextWave.nextGrid != null)
        {
            UpdateLevelTiles(nextWave.nextGrid);
            EnableNewPortals(nextWave.newPortals);
        }

        currentGrid.UpdateNavMesh();
    }

    private void UpdateLevelTiles(GridBuilder nextGrid)
    {
        List<GameObject> grid = currentGrid.GetTileSetup();
        List<GameObject> newGrid = nextGrid.GetTileSetup();

        for (int i = 0; i < grid.Count; i++)
        {
            TileSlot currentTile = grid[i].GetComponent<TileSlot>();
            TileSlot newTile = newGrid[i].GetComponent<TileSlot>();

            bool shouldBeUpdated = currentTile.GetMesh() != newTile.GetMesh() ||
                                   currentTile.GetMaterial() != newTile.GetMaterial() ||
                                   currentTile.GetAllChildren().Count != newTile.GetAllChildren().Count ||
                                   currentTile.transform.rotation != newTile.transform.rotation;

            if (shouldBeUpdated)
            {
                currentTile.gameObject.SetActive(false);

                newTile.gameObject.SetActive(true);
                newTile.transform.parent = currentGrid.transform;

                grid[i] = newTile.gameObject;
                Destroy(currentTile.gameObject);
            }
        }
    }

    private void EnableNewPortals(EnemyPortal[] newPortals)
    {
        foreach (EnemyPortal portal in newPortals)
        {
            portal.gameObject.SetActive(true);
            enemyPortals.Add(portal);
        }
    }

    private bool AllEnemiesDefeatrd()
    {
        foreach (EnemyPortal portal in enemyPortals)
        {
            if (portal.GetActiveEnemies().Count > 0)
                return false;
        }

        return true;
    }

    private bool ReadyToCheck()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkTinterval;
            return true;
        }

        return false;
    }
}
