using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private UI ui;
    private TileAnimator tileAnimator;
    private CameraEffects cameraEffects;

    private GridBuilder currentActiveGrid;
    public string currentLevelName { get; private set; }

    [Header("更換背景顏色設定")]
    [SerializeField] private MeshRenderer groundMesh;
    private Color defaultColor;

    private void Awake()
    {
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        ui = FindFirstObjectByType<UI>();

        defaultColor = groundMesh.material.color;
        groundMesh.material = new Material(groundMesh.material);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
            LoadLevelFromMenu("Level_1");

        if (Input.GetKeyDown(KeyCode.K))
            LoadMainMenu();

        if (Input.GetKeyDown(KeyCode.R))
            RestarCurrentLevel();

    }

    public void RestarCurrentLevel() => StartCoroutine(LoadLevelCo(currentLevelName));
    public void LoadLevel(string levelName) => StartCoroutine(LoadLevelCo(levelName));
    public void LoadNextLevel() => LoadLevel(GetNextLevelName());
    public void LoadLevelFromMenu(string levelName) => StartCoroutine(LoadLevelFromMenuCo(levelName));
    public void LoadMainMenu() => StartCoroutine(LoadMainMenuCo());

    private IEnumerator LoadLevelCo(string levelName)
    {
        CleanUpScene();
        ui.EnableInGameUI(false);

        cameraEffects.SwitchToGameView();
        yield return tileAnimator.GetCurrentActiveCo();

        UnloadCurrentScene();
        LoadScene(levelName);
    }

    private IEnumerator LoadLevelFromMenuCo(string levelName)
    {
        tileAnimator.ShowMainGrid(false);
        ui.EnableMainMenuUI(false);

        cameraEffects.SwitchToGameView();

        yield return tileAnimator.GetCurrentActiveCo();

        tileAnimator.EnableMainSceneObjects(false);

        LoadScene(levelName);
    }

    private IEnumerator LoadMainMenuCo()
    {
        CleanUpScene();
        ui.EnableInGameUI(false);

        cameraEffects.SwitchToMenuView();

        yield return tileAnimator.GetCurrentActiveCo();

        UpdateBackgroundColor(defaultColor);
        UnloadCurrentScene();

        tileAnimator.EnableMainSceneObjects(true);

        // 需求 1：將開啟 UI 的指令移到這裡，達成同步顯示
        ui.EnableMainMenuUI(true);
        tileAnimator.ShowMainGrid(true);

        // 等待主場景地塊升起動畫完全結束
        yield return tileAnimator.GetCurrentActiveCo();

        // 需求 2：精準抓取主場景專屬的 Spawner，並叫它重新開始生怪！
        MainScene_Spawner mainSpawner = FindFirstObjectByType<MainScene_Spawner>();
        if (mainSpawner != null)
        {
            // 直接呼叫 BeginSpawning，跳過 Start 的等待，因為我們已經等完動畫了
            mainSpawner.BeginSpawning();
        }
    }

    private void LoadScene(string sceneNameToLoad)
    {
        currentLevelName = sceneNameToLoad;
        SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);
    }

    private void UnloadCurrentScene() => SceneManager.UnloadSceneAsync(currentLevelName);

    private void CleanUpScene()
    {
        // 如果是從主選單離開，也要確保主選單的生怪器停止並下沉
        MainScene_Spawner mainSpawner = FindFirstObjectByType<MainScene_Spawner>();
        if (mainSpawner != null)
        {
            mainSpawner.StopSpawningAndSink();
        }

        GameManager.instance.StopMakingEnemies();
        EliminateAllEnemies();
        EliminateAllTowers();

        if (currentActiveGrid != null)
            tileAnimator.ShowGrid(currentActiveGrid, false);
    }

    private void EliminateAllEnemies()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.RemoveEnemy();
        }
    }

    private void EliminateAllTowers()
    {
        Tower[] towers = FindObjectsOfType<Tower>();

        foreach (Tower tower in towers)
        {
            Destroy(tower.gameObject);
        }
    }

    public void UpdateBackgroundColor(Color targetColor)
    {
        StartCoroutine(UpdateBackgroundColorCo(targetColor, 1.5f));
    }

    private IEnumerator UpdateBackgroundColorCo(Color targetColor, float duration)
    {
        float time = 0;
        Color startColor = groundMesh.material.color;

        while (time < duration)
        {
            Color currentColor = Color.Lerp(startColor, targetColor, time / duration);
            groundMesh.material.color = currentColor;

            time += Time.deltaTime;
            yield return null;
        }

        groundMesh.material.color = targetColor;
    }

    public void UpdateCurrentGrid(GridBuilder newGrid) => currentActiveGrid = newGrid;
    public int GetNextLevelIndex() => SceneUtility.GetBuildIndexByScenePath(currentLevelName) + 1;
    public string GetNextLevelName() => "Level_" + GetNextLevelIndex();
    public bool HasNoMoreLevels() => GetNextLevelIndex() >= SceneManager.sceneCountInBuildSettings;
}