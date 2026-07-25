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
    [SerializeField] private Material buildPreviewMaterial;

    // ==========================================
    // 預覽範圍圈設定
    // ==========================================
    [Header("預覽範圍圈設定")]
    public Sprite rangeGradientSprite;
    public Material rangeLineMaterial;
    public Color rangeFillColor = new Color(0f, 1f, 0.5f, 0.3f);
    [ColorUsage(true, true)] public Color rangeBorderColor = new Color(0f, 1f, 0.5f, 1f);
    public float rangeBorderThickness = 0.1f;
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
            if (CheckIfPointerOverUI())
                return;

            RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity);
            bool hitValidSlot = false;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.GetComponent<BuildSlot>() != null)
                {
                    hitValidSlot = true;
                    break;
                }
            }

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
        //  買塔防連點鎖：在扣除金錢之前，先檢查地塊是不是已經被第一下點擊清空了
        BuildSlot slotToUse = GetSelectedSlot();
        if (slotToUse == null)
        {
            // 如果是手速過快的第二下連點，到這裡就會被擋下，絕對不會往下執行扣錢！
            return;
        }

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

        // 這裡會把 selectedBuildSlot 設為 null，所以第二下連點會死在最上面的防呆機制
        CancelBuildAction();

        slotToUse.SnapToDefaultPositionImmidiatly();
        slotToUse.SetSlotAvailableTo(false);

        ui.buildButtonsUI.SetLastSelected(null, null);

        cameraEffects.ScreenShake(camShakeDuration, camShakeMagnitude);

        GameObject newTower = Instantiate(towerToBuild, slotToUse.GetBuildPosition(towerCenterY), Quaternion.identity);
        newTower.transform.rotation = newPreviewTower.rotation;

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

    public Material GetBuildPreviewMaterial() => buildPreviewMaterial;
}