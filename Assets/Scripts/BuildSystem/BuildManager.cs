using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    private UI ui;
    public BuildSlot selectedBuildSlot;

    public WaveManager waveManger;
    public GridBuilder currentGrid;
    private GameManager gameManager;
    private CameraEffects cameraEffects;

    [SerializeField] private LayerMask whatToIgnore;

    [Header("塔的預覽材質")]
    // ★ 舊的 attackRadiusMaterial 已經徹底移除，只留下模型半透明預覽用的材質！
    [SerializeField] private Material buildPreviewMaterial;

    // ==========================================
    // 預覽範圍圈設定
    // ==========================================
    [Header("預覽範圍圈設定")]
    public Sprite rangeGradientSprite; // 內圈漸層圖片
    public Material rangeLineMaterial; // 外圈線條材質
    public Color rangeFillColor = new Color(0f, 1f, 0.5f, 0.3f); //內圈填充顏色
    [ColorUsage(true, true)] public Color rangeBorderColor = new Color(0f, 1f, 0.5f, 1f); //外圈顏色
    public float rangeBorderThickness = 0.1f; //外圈粗細
    // ==========================================

    [Header("建造設定")]
    [SerializeField] private float towerCenterY = 0.5f;
    [SerializeField] private float camShakeDuration = 0.15f;
    [SerializeField] private float camShakeMagnitude = 0.02f;

    [Header("特效與音效設定")]
    public GameObject buildFX;
    public AudioSource buildSound;

    public bool isMouseOverUI;

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
    }

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            CancelBuildAction();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // 1. 如果點到 UI (例如按鈕)，直接忽略
            if (CheckIfPointerOverUI())
                return;

            // 2. 發射「穿透射線」，取得滑鼠點下去那一條線上撞到的【所有東西】
            RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity);

            bool hitValidSlot = false;

            // 3. 檢查這條線上所有的碰撞體，只要裡面有一個是 BuildSlot，就算點擊成功！
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.GetComponent<BuildSlot>() != null)
                {
                    hitValidSlot = true;
                    break; // 找到了！立刻停止搜尋
                }
            }

            // 4. 只有在「確定滑鼠底下完全沒有任何 BuildSlot」的時候，才執行取消
            if (hitValidSlot == false)
            {
                CancelBuildAction();
            }
        }
    }

    public bool CheckIfPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateBuildManager(WaveManager newWaveManager, GridBuilder newCurrentGrid)
    {
        currentGrid = newCurrentGrid;
        MakeBuildSlotNotAvalibleIfNeeded(newWaveManager, currentGrid);
    }

    public void BuildTower(GameObject towerToBuild, int towerPrice, Transform newPreviewTower)
    {
        if (gameManager.HasEnoughCurrency(towerPrice) == false)
        {
            ui.inGameUI.ShakeCurrencyUI();
            return;
        }

        if (towerToBuild == null)
        {
            Debug.LogWarning("還沒有這座塔");
            return;
        }

        if (ui.buildButtonsUI.GetLastSelectedButton() == null)
            return;

        Transform previewTower = newPreviewTower;
        BuildSlot slotToUse = GetSelectedSlot();
        CancelBuildAction();

        slotToUse.SnapToDefaultPositionImmidiatly();
        slotToUse.SetSlotAvailableTo(false);

        ui.buildButtonsUI.SetLastSelected(null, null);

        cameraEffects.ScreenShake(camShakeDuration, camShakeMagnitude);

        GameObject newTower = Instantiate(towerToBuild, slotToUse.GetBuildPosition(towerCenterY), Quaternion.identity);
        newTower.transform.rotation = newPreviewTower.rotation;

        //  修改這裡：除了播音效，也順便把特效 Instantiate (生成) 出來！
        if (buildFX != null)
        {
            Instantiate(buildFX, newTower.transform.position + Vector3.up * 0.1f, Quaternion.identity);
        }

        if (buildSound != null)
        {
            buildSound.Play();
        }
    }

    public void MouseOverUI(bool IsOverUI) => isMouseOverUI = IsOverUI;

    public void MakeBuildSlotNotAvalibleIfNeeded(WaveManager waveManager, GridBuilder currentGrid)
    {
        if (waveManager == null)
        {
            Debug.Log("沒有下一波");
            return;
        }

        foreach (var wave in waveManager.GetLevelWaves())
        {
            if (wave.nextGrid == null)
                continue;

            List<GameObject> grid = currentGrid.GetTileSetup();
            List<GameObject> nextWaveGrid = wave.nextGrid.GetTileSetup();

            for (int i = 0; i < grid.Count; i++)
            {
                TileSlot currentTile = grid[i].GetComponent<TileSlot>();
                TileSlot nextTile = nextWaveGrid[i].GetComponent<TileSlot>();

                bool tileNotTheSame = currentTile.GetMesh() != nextTile.GetMesh() ||
                                      currentTile.GetOriginalMaterial() != nextTile.GetOriginalMaterial() ||
                                      currentTile.GetAllChildren().Count != nextTile.GetAllChildren().Count;

                if (tileNotTheSame == false)
                    continue;

                BuildSlot buildSlot = grid[i].GetComponent<BuildSlot>();

                if (buildSlot != null)
                    buildSlot.SetSlotAvailableTo(false);
            }

        }
    }

    public void CancelBuildAction()
    {
        if (selectedBuildSlot == null)
            return;

        ui.buildButtonsUI.GetLastSelectedButton()?.SelectButton(false);

        selectedBuildSlot.UnSelectTile();
        selectedBuildSlot = null;
        DisableBuildMenu();
    }

    public void SelectBuildSlot(BuildSlot newSlot)
    {
        if (selectedBuildSlot != null)
            selectedBuildSlot.UnSelectTile();

        selectedBuildSlot = newSlot;
    }

    public void EnableBuildMenu()
    {
        if (selectedBuildSlot != null)
            return;

        ui.buildButtonsUI.ShowBuildButtons(true);
    }

    private void DisableBuildMenu()
    {
        ui.buildButtonsUI.ShowBuildButtons(false);
    }

    public BuildSlot GetSelectedSlot() => selectedBuildSlot;

    // ★ 舊的 GetAttackRadiusMaterial() 已經刪除
    public Material GetBuildPreviewMaterial() => buildPreviewMaterial;
}