using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// 改成這樣：
// ★ 補上 IPointerDownHandler
public class UI_BuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private BuildManager buildManager;
    private CameraEffects cameraEffects;
    private GameManager gameManager;
    private UI_BuildButtonsHolder buildButtonHolder;
    private UI_BuildButtonOnHoverEffect onHoverEffect;
    private UI_TowerInfoPanel infoPanel;


    [SerializeField] private string towerName;
    [SerializeField] private int towerPrice = 50;
    [Space]
    [SerializeField] private GameObject towerToBuild;
    [Header("文字元件")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerPriceText;

    public TowerPreview towerPreview;
    public bool buttonUnlocked {  get; private set; }

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        onHoverEffect = GetComponent<UI_BuildButtonOnHoverEffect>();
        buildButtonHolder = GetComponentInParent<UI_BuildButtonsHolder>();

        buildManager = FindFirstObjectByType<BuildManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void Start()
    {
        CreateTowerPreview();
        // 加入這行：遊戲開始時，自動去畫面上把那個大面板找出來！
        infoPanel = FindFirstObjectByType<UI_TowerInfoPanel>(FindObjectsInactive.Include);
    }
    private void CreateTowerPreview()
    {
        GameObject newPreview = Instantiate(towerToBuild, Vector3.zero, Quaternion.identity);

        towerPreview = newPreview.AddComponent<TowerPreview>();
        towerPreview.SetupTowerPreview(newPreview);
        towerPreview.transform.parent = buildManager.transform;
    }

    public void SelectButton(bool select)
    {
        // 修正：如果是要「取消選取 (select == false)」，我們不需要檢查地塊，直接關閉預覽就好！
        if (select == false)
        {
            if (towerPreview != null) towerPreview.gameObject.SetActive(false);
            //if (onHoverEffect != null) onHoverEffect.ShowcaseButton(false);
            return; // 關閉完就直接結束
        }

        BuildSlot slotToUse = buildManager.GetSelectedSlot();

        // 只有在「開啟 (select == true)」時，才需要檢查地塊存不存在
        if (slotToUse == null)
            return;

        Vector3 previewPosition = slotToUse.GetBuildPosition(1f);

        towerPreview.gameObject.SetActive(true);
        towerPreview.ShowPreview(true, previewPosition);
        //onHoverEffect.ShowcaseButton(true);
        buildButtonHolder.SetLastSelected(this, towerPreview.transform);
    }

    public void UnlockTowerIfNeeded(string towerNameToCheck, bool unlockStatus)
    {
        if (towerNameToCheck != towerName)
            return;

        buttonUnlocked = unlockStatus;
        gameObject.SetActive(unlockStatus);
    }

    public void ConfirmTowerBuild()
    {
        // 修正 1：在送出建造指令前，強制重新喚醒預覽圖並確認地塊位置
        SelectButton(true);

        // 修正 2：增加一道安全鎖。如果真的找不到地塊，就直接退出，絕對不執行後續的扣錢與建造
        if (buildManager.GetSelectedSlot() == null)
        {
            return;
        }

        // 原本的建造與扣款邏輯
        buildManager.BuildTower(towerToBuild, towerPrice, towerPreview.transform);

        // 買塔的時候，把面板順便關閉
        if (infoPanel != null)
        {
            infoPanel.ClosePanel();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buildManager.MouseOverUI(true);

        foreach (var button in buildButtonHolder.GetBuildButtons())
        {
            if (button.gameObject.activeSelf)
                button.SelectButton(false);
        }

        SelectButton(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buildManager.MouseOverUI(false);

    }
    private void OnValidate()
    {
        towerNameText.text = towerName;
        towerPriceText.text = towerPrice + "";
        gameObject.name = "BuildButton_UI_" + towerName;
    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    ConfirmTowerBuild();
    //}
    // ★ 當玩家點擊這張卡片的「任何空白處」時，關閉詳細資訊面板
    public void OnPointerDown(PointerEventData eventData)
    {
        if (infoPanel != null)
            infoPanel.ClosePanel();
    }
    // 新增這個方法：用來打開詳細資訊介面
    public void OpenTowerDetails()
    {
        RectTransform myRect = GetComponent<RectTransform>();

        // ★ 智慧判斷：如果面板已經開著，且玩家點擊的是「同一個」驚嘆號
        if (infoPanel != null && infoPanel.gameObject.activeSelf)
        {
            // 比較面板和按鈕的 X 座標來確認是不是同一個
            if (Mathf.Abs(infoPanel.GetComponent<RectTransform>().position.x - myRect.position.x) < 0.1f)
            {
                infoPanel.ClosePanel(); // 關掉它
                return; // 提早結束，就不會往下執行重新開啟了！
            }
        }

        // --- 以下是你原本的開啟邏輯 ---
        if (buildButtonHolder != null && towerPreview != null)
        {
            towerPreview.gameObject.SetActive(false);
            buildButtonHolder.SetLastSelected(null, null);
        }

        if (infoPanel != null)
        {
            Tower myTowerData = towerToBuild.GetComponent<Tower>();
            infoPanel.OpenPanel(myRect, myTowerData);
        }
    }
}
