using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI instance;

    [Header("UI 設定")]
    [SerializeField] private GameObject uiPrefab;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 3f, 0);

    [Header("特效與音效")]
    [Tooltip("升級時的粒子特效")]
    [SerializeField] private GameObject upgradeFX;
    [Tooltip("賣出時的粒子特效")]
    [SerializeField] private GameObject sellFX;
    [Tooltip("升級音效")]
    [SerializeField] private AudioClip upgradeSound;
    [Tooltip("賣出音效")]
    [SerializeField] private AudioClip sellSound;

    private GameObject currentUIInstance;
    private Tower selectedTower;

    // UI 元件
    private Button upgradeButton;
    private Button closeButton;
    private Button sellButton; // 新增
    private TextMeshProUGUI priceText;
    private TextMeshProUGUI sellPriceText; // 新增

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Update()
    {
        if (currentUIInstance != null && currentUIInstance.activeSelf)
        {
            if (Camera.main != null)
                currentUIInstance.transform.forward = Camera.main.transform.forward;
        }

        DetectClickOutside();
    }

    public void SelectTower(Tower tower)
    {
        if (selectedTower != null && selectedTower != tower)
        {
            Hide();
        }

        selectedTower = tower;

        if (currentUIInstance == null)
        {
            CreateUI();
        }

        currentUIInstance.transform.position = tower.transform.position + uiOffset;
        UpdateUIState();
        currentUIInstance.SetActive(true);
    }

    private void CreateUI()
    {
        currentUIInstance = Instantiate(uiPrefab);

        Canvas canvas = currentUIInstance.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = Camera.main;
        }

        // --- 綁定 UI 元件 (使用深度搜尋) ---
        Transform btnUpgradeTrans = FindDeepChild(currentUIInstance.transform, "Btn_Upgrade");
        if (btnUpgradeTrans != null) upgradeButton = btnUpgradeTrans.GetComponent<Button>();

        Transform btnCloseTrans = FindDeepChild(currentUIInstance.transform, "Btn_Close");
        if (btnCloseTrans != null) closeButton = btnCloseTrans.GetComponent<Button>();

        // ★ 新增：綁定賣出按鈕
        Transform btnSellTrans = FindDeepChild(currentUIInstance.transform, "Btn_Sell");
        if (btnSellTrans != null) sellButton = btnSellTrans.GetComponent<Button>();

        // 文字
        Transform txtPriceTrans = FindDeepChild(currentUIInstance.transform, "Txt_Price");
        if (txtPriceTrans != null) priceText = txtPriceTrans.GetComponent<TextMeshProUGUI>();

        // ★ 新增：綁定賣出價格文字
        Transform txtSellPriceTrans = FindDeepChild(currentUIInstance.transform, "Txt_SellPrice");
        if (txtSellPriceTrans != null) sellPriceText = txtSellPriceTrans.GetComponent<TextMeshProUGUI>();


        // --- 綁定事件 ---
        if (upgradeButton != null) upgradeButton.onClick.AddListener(UpgradeSelectedTower);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (sellButton != null) sellButton.onClick.AddListener(SellSelectedTower); // ★ 新增事件
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void UpdateUIState()
    {
        if (selectedTower == null)
        {
            Hide();
            return;
        }

        // 1. 更新升級按鈕狀態
        if (upgradeButton != null)
        {
            if (selectedTower.nextUpgradePrefab != null)
            {
                upgradeButton.interactable = true;
                if (priceText != null) priceText.text = "$ " + selectedTower.upgradeCost;
            }
            else
            {
                upgradeButton.interactable = false;
                if (priceText != null) priceText.text = "MAX";
            }
        }

        // 2. 更新賣出價格文字
        if (sellPriceText != null)
        {
            sellPriceText.text = "$ " + selectedTower.sellReward;
        }
    }

    private void DetectClickOutside()
    {
        if (currentUIInstance == null || !currentUIInstance.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;

            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (selectedTower != null && hit.collider.gameObject == selectedTower.gameObject) return;
                }
            }
            Hide();
        }
    }

    private bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            int layer = result.gameObject.layer;
            if (layer == LayerMask.NameToLayer("TowerUpgrade") || layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }

    public void UpgradeSelectedTower()
    {
        if (selectedTower == null || selectedTower.nextUpgradePrefab == null) return;

        if (GameManager.instance.HasEnoughCurrency(selectedTower.upgradeCost))
        {
            // 1. 播放特效與音效
            PlayEffects(upgradeFX, upgradeSound, selectedTower.transform.position);

            // 2. 生成新塔
            Tower oldTower = selectedTower;
            Instantiate(selectedTower.nextUpgradePrefab.gameObject, selectedTower.transform.position, selectedTower.transform.rotation);

            // 3. 處理後續
            Hide();
            Destroy(oldTower.gameObject);
        }
        else
        {
            if (GameManager.instance.inGameUI != null)
                GameManager.instance.inGameUI.ShakeCurrencyUI();
        }
    }

    public void SellSelectedTower()
    {
        if (selectedTower == null) return;

        // 1. 加錢
        GameManager.instance.UpdateCurrency(selectedTower.sellReward);

        // 2. 播放特效與音效
        // (這裡沿用你原本的 PlayEffects 邏輯，如果沒有該方法請保留原本寫法)
        // PlayEffects(sellFX, sellSound, selectedTower.transform.position); 

        // 3. ★ 關鍵修復：使用 RaycastAll 來穿透塔身找到地板 ★
        // 我們抓取射線路徑上打到的 "所有東西"
        RaycastHit[] hits = Physics.RaycastAll(selectedTower.transform.position + Vector3.up, Vector3.down, 5f);

        foreach (RaycastHit hit in hits)
        {
            // 檢查打到的這個東西有沒有 BuildSlot
            BuildSlot slot = hit.collider.GetComponent<BuildSlot>();

            if (slot != null)
            {
                // 找到了！告訴地板：「你是自由的了！」
                slot.SetSlotAvailableTo(true);

                // 保險起見，重置一下地板的狀態 (避免它還以為自己被選取中)
                slot.UnSelectTile();

                // 找到地板就可以跳出迴圈了，不用再找了
                break;
            }
        }

        // 4. 刪除塔
        Destroy(selectedTower.gameObject);
        Hide();
    }

    // 播放特效的小幫手
    private void PlayEffects(GameObject fxPrefab, AudioClip clip, Vector3 position)
    {
        if (fxPrefab != null)
        {
            Instantiate(fxPrefab, position + Vector3.up * 0.1f, Quaternion.identity);
        }

        if (clip != null)
        {
            // 在該位置播放 3D 音效
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }

    public void Hide()
    {
        if (currentUIInstance != null)
        {
            currentUIInstance.SetActive(false);
        }
        selectedTower = null;
    }
}