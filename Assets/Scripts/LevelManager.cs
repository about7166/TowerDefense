using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public List<TowerUnlockData> towerUnlocks;

    private void Start()
    {
        UnlockAvailableTowers();
    }

    private void UnlockAvailableTowers()
    {
        UI ui = FindFirstObjectByType<UI>();

        foreach (var unlockDate in towerUnlocks)//找每座塔的資料
        {
            foreach (var buildButton in ui.buildButtonsUI.GetBuildButtons())//找按鈕
            {
                buildButton.UnlockTowerIfNeeded(unlockDate.towerName, unlockDate.unlocked);
            }
        }

        ui.buildButtonsUI.UpdateUnlockedButtons();
    }

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