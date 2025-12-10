using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UI ui;
    private BuildManager buildManager;
    private CameraEffects cameraEffects;
    private GameManager gameManager;
    private TowerAttackRadiusDisplay towerAttackRadiusDisplay;


    [SerializeField] private string towerName;
    [SerializeField] private int towerPrice = 50;
    [Space]
    [SerializeField] private GameObject towerToBuild;
    [SerializeField] private float towerCenterY = 0.5f;
    [Header("文字元件")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;

    [Header("塔的設定")]
    [SerializeField] private float towerAttackRadius = 3;


    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        buildManager = FindFirstObjectByType<BuildManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        gameManager = FindFirstObjectByType<GameManager>();

        towerAttackRadiusDisplay = FindFirstObjectByType<TowerAttackRadiusDisplay>(FindObjectsInactive.Include);

        if (towerToBuild != null)
            towerAttackRadius = towerToBuild.GetComponent<Tower>().GetAttackRange();
    }

    public void UnlockTowerIfNeeded(string towerNameToCheck, bool unlockStatus)
    {
        if (towerNameToCheck != towerName)
            return;

        gameObject.SetActive(unlockStatus);
    }

    public void BuildTower()
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

        BuildSlot slotToUse = buildManager.GetSelectedSlot();
        buildManager.CancelBuildAction();

        slotToUse.SnapToDefaultPositionImmidiatly();
        slotToUse.SetSlotAvailableTo(false);

        cameraEffects.ScreenShake(0.15f, 0.02f);

        GameObject newtower = Instantiate(towerToBuild, slotToUse.GetBuildPosition(towerCenterY), Quaternion.identity);
    }

    private void OnValidate()
    {
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "BuildButton_UI_" + towerName;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //BuildSlot slotToUse = buildManager.GetSelectedSlot();
        //towerAttackRadiusDisplay.ShowAttackRadius(true, towerAttackRadius, slotToUse.GetBuildPosition(0.5f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //towerAttackRadiusDisplay.ShowAttackRadius(false, towerAttackRadius, Vector3.zero);
    }
}
