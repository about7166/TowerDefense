using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public WaveManager currentActiveWaveManager;// {  get; private set; }
    public UI_InGame inGameUI {  get; private set; }
    private LevelManager levelManager;
    private CameraEffects cameraEffects;


    [SerializeField] private int currency;
    
    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;

    public int enemiesKilled {  get; private set; }

    private bool gameLost;

    private void Awake()
    {
        // --- 單例模式保護機制 ---
        if (instance != null && instance != this)
        {
            // 如果已經有老大了，我這個新來的(分身)就自我毀滅
            Destroy(gameObject);
            return;
        }

        instance = this;
        // ----------------------

        // 加上這行：因為場景切換時我們不希望這個唯一的管理者被刪掉
        // 但因為你的主場景通常不會被卸載，這行看情況加，不加也沒關係
        // DontDestroyOnLoad(gameObject); 

        inGameUI = FindFirstObjectByType<UI_InGame>(FindObjectsInactive.Include);
        levelManager = FindFirstObjectByType<LevelManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
    }

    private void Start()
    {
        currentHp = maxHp;

        if (IsTestingLevel())
        {
            currency += 9999;
            currentHp += 9999;
        }

        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.UpdateCurrencyUI(currency);
    }

    public void StopMakingEnemies()
    {
        EnemyPortal[] portals = FindObjectsOfType<EnemyPortal>();

        foreach (var portal in portals)
            portal.CanCreateNewEnemies(false);
    }

    public bool IsTestingLevel() => levelManager == null;

    public IEnumerator LevelFailedCo()
    {
        gameLost = true;
        currentActiveWaveManager.DeactivateWaveManager();
        cameraEffects.FocusOnCastle();

        yield return cameraEffects.GetActiveCamCo();

        inGameUI.EnableGameOverUI(true);
    }

    public void LevelCompleted() => StartCoroutine(LevelCompletedCo());
    private IEnumerator LevelCompletedCo()
    {
        cameraEffects.FocusOnCastle();

        yield return cameraEffects.GetActiveCamCo();

        if (levelManager.HasNoMoreLevels())
        {
            inGameUI.EnableVictoryUI(true);
        }
        else
        {
            PlayerPrefs.SetInt(levelManager.GetNextLevelName() + "unlocked", 1);//1是true 0是false
            inGameUI.EnableLevelCompletedUI(true);
        }
    }

    public void PrepareLevel(int levelCurrency, WaveManager newWaveManager)
    {
        gameLost = false;
        enemiesKilled = 0;

        currentActiveWaveManager = newWaveManager;
        currency = levelCurrency;
        currentHp = maxHp;

        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.UpdateCurrencyUI(currency);

        newWaveManager.ActivateWaveManager();
    }

    public void UpdateHp(int value)
    {
        currentHp += value;
        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.ShakeHealthUI();

        if (currentHp <= 0 && gameLost == false)
            StartCoroutine(LevelFailedCo());
    }

    public void UpdateCurrency(int value)
    {
        enemiesKilled++;
        currency += value;
        inGameUI.UpdateCurrencyUI(currency);
    }

    public bool HasEnoughCurrency(int price)
    {
        if (price <= currency)
        {
            currency = currency - price;
            inGameUI.UpdateCurrencyUI(currency);

            return true;
        }
        return false;
    }
}
