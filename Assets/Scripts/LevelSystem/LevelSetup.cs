using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSetup : MonoBehaviour
{
    private UI ui;
    private TileAnimator tileAnimator;
    private LevelManager levelManager;
    private GameManager gameManager;
    private BuildManager buildManager;

    [Header("關卡詳情")]
    [SerializeField] private int levelCurrency = 1000;
    [SerializeField] private List<TowerUnlockData> towerUnlocks;

    [Header("關卡設定")]
    [SerializeField] private GridBuilder myMainGrid;
    [SerializeField] private WaveManager myWaveManager;
    [SerializeField] private List<GameObject> extraObjectsToDelete = new List<GameObject>();
    [SerializeField] private Material groundMaterial;

    private IEnumerator Start()
    {
        if (LevelWasLoadedToMainScene())
        {
            DeleteExtraObjects();

            buildManager = FindFirstObjectByType<BuildManager>();
            buildManager.UpdateBuildManager(myWaveManager, myMainGrid);

            levelManager.UpdateCurrentGrid(myMainGrid);
            levelManager.UpdateBackgroundColor(groundMaterial.color);

            tileAnimator = FindFirstObjectByType<TileAnimator>();
            tileAnimator.ShowGrid(myMainGrid, true);

            yield return tileAnimator.GetCurrentActiveCo();

            ui = FindFirstObjectByType<UI>();
            ui.EnableInGameUI(true);

            gameManager = FindFirstObjectByType<GameManager>();
            gameManager.PrepareLevel(levelCurrency, myWaveManager);
        }

        UnlockAvailableTowers();
    }

    private bool LevelWasLoadedToMainScene()
    {
        levelManager = FindFirstObjectByType<LevelManager>();

        return levelManager != null;
    }

    private void DeleteExtraObjects()
    {
        foreach (var obj in extraObjectsToDelete)
        {
            if (obj != null)
            {
                // 解除卡死 Bug 的核心魔法：在銷毀前先強制隱藏，避免動畫系統抓錯人！
                obj.SetActive(false);
                Destroy(obj);
            }
        }
    }

    private void UnlockAvailableTowers()
    {
        UI ui = FindFirstObjectByType<UI>();

        if (ui == null || ui.buildButtonsUI == null)
        {
            Debug.LogWarning("找不到 UI 總管，跳過解鎖防禦塔！(可能是因為你正在單獨測試關卡場景)");
            return;
        }

        foreach (var unlockData in towerUnlocks)
        {
            foreach (var buildButton in ui.buildButtonsUI.GetBuildButtons())
            {
                buildButton.UnlockTowerIfNeeded(unlockData.towerName, unlockData.unlocked);
            }
        }

        ui.buildButtonsUI.UpdateUnlockedButtons();
    }

    public WaveManager GetWaveManager() => myWaveManager;

    [ContextMenu("初始化塔的資料")]
    private void InitializeTowerData()
    {
        towerUnlocks.Clear();

        towerUnlocks.Add(new TowerUnlockData("Crossbow", false));
        towerUnlocks.Add(new TowerUnlockData("Cannon", false));
        towerUnlocks.Add(new TowerUnlockData("Rapid Fire Gun", false));
        towerUnlocks.Add(new TowerUnlockData("Hammer", false));
        towerUnlocks.Add(new TowerUnlockData("Spider Nest", false));
        towerUnlocks.Add(new TowerUnlockData("Anti-air Harpon", false));
        towerUnlocks.Add(new TowerUnlockData("Just Fan", false));
    }
}

[System.Serializable]
public class TowerUnlockData
{
    public string towerName;
    public bool unlocked;

    public TowerUnlockData(string newTowerName, bool newUnlockedStatus)
    {
        towerName = newTowerName;
        unlocked = newUnlockedStatus;
    }
}