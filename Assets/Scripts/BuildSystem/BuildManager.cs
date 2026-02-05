using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Material attackRadiusMaterial;
    [SerializeField] private Material buildPreviewMaterial;

    [Header("建造設定")]
    [SerializeField] private float towerCenterY = 0.5f;
    [SerializeField] private float camShakeDuration = 0.15f;
    [SerializeField] private float camShakeMagnitude = 0.02f; //晃動幅度

    private bool isMouseOverUI;

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();

        //MakeBuildSlotNotAvalibleIfNeeded(waveManger, currentGrid);
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
            if (isMouseOverUI)
                return;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, ~whatToIgnore))
            {
                bool clickedNotOnBuildSlot = hit.collider.GetComponent<BuildSlot>() == null;

                if (clickedNotOnBuildSlot)
                    CancelBuildAction();
            }
        }
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
    public Material GetAttackRadiusMaterial() => attackRadiusMaterial;
    public Material GetBuildPreviewMaterial() => buildPreviewMaterial;
}
