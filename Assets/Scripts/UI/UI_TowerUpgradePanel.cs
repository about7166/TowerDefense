using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Localization.Settings; // ★ 多國語系

public class UI_TowerUpgradePanel : MonoBehaviour
{
    private static UI_TowerUpgradePanel _instance;
    public static UI_TowerUpgradePanel instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<UI_TowerUpgradePanel>(FindObjectsInactive.Include);
                if (_instance != null) _instance.ForceInit();
            }
            return _instance;
        }
    }

    private bool isInitialized = false;

    // ==========================================
    // ★ 攻擊範圍顯示系統變數 (英雄聯盟風格)
    // ==========================================
    private LineRenderer rangeBorder;
    private SpriteRenderer rangeFill;

    [Header("攻擊範圍顯示設定")]
    [Tooltip("請放入那張『外圍白、內心透明』的漸層圓形 Sprite")]
    public Sprite gradientFillSprite;

    [Tooltip("請放入你截圖裡的那個 Circle 材質球，或是任何 Unlit 材質球")]
    public Material lineMaterial;

    [SerializeField] private Color fillColor = new Color(0f, 1f, 0.5f, 0.3f); // 內圈漸層顏色

    [ColorUsage(true, true)] // 開啟 HDR，讓這個顏色可以發光！
    [SerializeField] private Color borderColor = new Color(0f, 1f, 0.5f, 1f); // 外框實線顏色

    [SerializeField] private float borderThickness = 0.1f; // 外框粗細
    // ==========================================


    private void Awake()
    {
        ForceInit();
    }

    public void ForceInit()
    {
        if (isInitialized) return;
        isInitialized = true;

        _instance = this;

        if (upgradeButton != null) upgradeButton.onClick.AddListener(UpgradeSelectedTower);
        if (sellButton != null) sellButton.onClick.AddListener(SellSelectedTower);

        if (panelObject != null) panelObject.SetActive(false);

        // 初始化範圍指示器
        CreateRangeIndicator();
    }

    private void CreateRangeIndicator()
    {
        if (rangeBorder != null && rangeFill != null) return;

        // 1. 建立總開關 (父物件)
        GameObject indicatorObj = new GameObject("DynamicRangeIndicator");

        // 2. 製作內圈光暈 (SpriteRenderer)
        GameObject fillObj = new GameObject("Fill_Gradient");
        fillObj.transform.SetParent(indicatorObj.transform);
        fillObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        rangeFill = fillObj.AddComponent<SpriteRenderer>();
        rangeFill.sprite = gradientFillSprite;
        rangeFill.color = fillColor;
        rangeFill.sortingOrder = 4;

        // 3. 製作發光外框 (LineRenderer)
        GameObject borderObj = new GameObject("Border_Line");
        borderObj.transform.SetParent(indicatorObj.transform);

        rangeBorder = borderObj.AddComponent<LineRenderer>();
        rangeBorder.useWorldSpace = true;
        rangeBorder.loop = true;
        rangeBorder.positionCount = 60;

        rangeBorder.startWidth = borderThickness;
        rangeBorder.endWidth = borderThickness;

        if (lineMaterial != null) rangeBorder.material = lineMaterial;
        rangeBorder.startColor = borderColor;
        rangeBorder.endColor = borderColor;
        rangeBorder.sortingOrder = 5;

        indicatorObj.SetActive(false);
    }

    private Tower selectedTower;

    [Header("介面主體")]
    [SerializeField] private GameObject panelObject;

    [Header("動畫設定")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float hiddenPosY = -500f;
    [SerializeField] private float shownPosY = 50f;
    [SerializeField] private float animDuration = 0.25f;
    private Coroutine animCoroutine;

    [Header("左側資訊與等級標題")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerIconImage;
    [SerializeField] private Image attributeIcon1_Image;
    [SerializeField] private Image attributeIcon2_Image;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;

    [Header("排版自動置中系統")]
    [SerializeField] private HorizontalLayoutGroup[] statRows;
    [SerializeField] private int maxPadding = 120;

    [Header("LV.1 (目前數值)")]
    [SerializeField] private TextMeshProUGUI current_Damage;
    [SerializeField] private TextMeshProUGUI current_Range;
    [SerializeField] private TextMeshProUGUI current_CD;
    [SerializeField] private TextMeshProUGUI current_Slow;
    [SerializeField] private TextMeshProUGUI current_DoT;

    [Header("LV.2 (升級後數值)")]
    [SerializeField] private GameObject nextStatsGroup;
    [SerializeField] private GameObject[] upgradeArrows;
    [SerializeField] private TextMeshProUGUI next_Damage;
    [SerializeField] private TextMeshProUGUI next_Range;
    [SerializeField] private TextMeshProUGUI next_CD;
    [SerializeField] private TextMeshProUGUI next_Slow;
    [SerializeField] private TextMeshProUGUI next_DoT;

    [Header("按鈕與價格")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellRewardText;

    [Header("特效與音效")]
    public GameObject upgradeFX;
    public GameObject sellFX;
    // ★ 這裡已經改成你要求的 AudioSource 播放器了
    public AudioSource upgradeSound;
    public AudioSource sellSound;

    private void Update() => DetectClickOutside();

    public void SelectTower(Tower tower)
    {
        if (selectedTower != null && selectedTower != tower)
            selectedTower.ToggleHighlight(false);

        selectedTower = tower;
        selectedTower.ToggleHighlight(true);

        panelObject.SetActive(true);
        UpdateUI();

        ShowRangeIndicator();

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(SlidePanel(shownPosY));
    }

    public void Hide()
    {
        if (selectedTower != null)
        {
            selectedTower.ToggleHighlight(false);
            selectedTower = null;
        }

        // ★ 關閉面板時，把範圍圈父物件藏起來
        if (rangeFill != null) rangeFill.transform.parent.gameObject.SetActive(false);

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(SlidePanel(hiddenPosY, true));
    }

    private void ShowRangeIndicator()
    {
        if (selectedTower == null || rangeFill == null || rangeBorder == null) return;

        rangeFill.transform.parent.gameObject.SetActive(true);

        float radius = selectedTower.GetAttackRange();
        Vector3 center = selectedTower.transform.position + Vector3.up * 0.15f;

        rangeFill.transform.position = center;
        float spriteSize = rangeFill.sprite != null ? rangeFill.sprite.bounds.size.x : 1f;
        float targetScale = (radius * 2f) / spriteSize;
        rangeFill.transform.localScale = new Vector3(targetScale, targetScale, 1f);

        for (int i = 0; i < 60; i++)
        {
            float angle = ((float)i / 60) * Mathf.PI * 2f;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            rangeBorder.SetPosition(i, center + new Vector3(x, 0, z));
        }
    }

    private System.Collections.IEnumerator SlidePanel(float targetY, bool disableAfter = false)
    {
        if (panelRect == null) yield break;

        float time = 0;
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x, targetY);

        while (time < animDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, time / animDuration);
            panelRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panelRect.anchoredPosition = targetPos;
        if (disableAfter) panelObject.SetActive(false);
    }

    private void UpdateUI()
    {
        if (selectedTower == null) return;

        if (towerNameText != null)
        {
            string myKey = "Tower_" + selectedTower.towerName;
            towerNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UI_Text", myKey);
        }

        if (towerIconImage != null && selectedTower.towerIcon != null) towerIconImage.sprite = selectedTower.towerIcon;

        if (attributeIcon1_Image != null && selectedTower.attributeIcon1 != null) attributeIcon1_Image.sprite = selectedTower.attributeIcon1;
        if (attributeIcon2_Image != null && selectedTower.attributeIcon2 != null) attributeIcon2_Image.sprite = selectedTower.attributeIcon2;

        if (current_Damage != null) current_Damage.text = selectedTower.GetAttackDamage().ToString();
        if (current_Range != null) current_Range.text = selectedTower.GetAttackRange().ToString();
        if (current_CD != null) current_CD.text = selectedTower.GetAttackCooldown().ToString() + "s";
        if (current_Slow != null) current_Slow.text = selectedTower.GetSlowPercentage().ToString("F0") + "%";
        if (current_DoT != null) current_DoT.text = selectedTower.ui_dotText;

        if (sellRewardText != null) sellRewardText.text = selectedTower.sellReward.ToString();

        Tower nextPrefab = selectedTower.nextUpgradePrefab;

        if (nextPrefab != null)
        {
            if (statRows != null)
            {
                foreach (var row in statRows)
                {
                    if (row != null) row.padding = new RectOffset(0, 0, 0, 0);
                }
            }

            if (currentLevelText != null) currentLevelText.text = "LV." + selectedTower.towerLevel;
            if (nextLevelText != null) nextLevelText.text = "LV." + (selectedTower.towerLevel + 1);

            if (nextStatsGroup != null) nextStatsGroup.SetActive(true);
            if (upgradeButton != null) upgradeButton.interactable = true;
            if (upgradeCostText != null) upgradeCostText.text = selectedTower.upgradeCost.ToString();

            if (next_Damage != null) next_Damage.gameObject.SetActive(true);
            if (next_Range != null) next_Range.gameObject.SetActive(true);
            if (next_CD != null) next_CD.gameObject.SetActive(true);
            if (next_Slow != null) next_Slow.gameObject.SetActive(true);
            if (next_DoT != null) next_DoT.gameObject.SetActive(true);

            if (upgradeArrows != null)
            {
                foreach (var arrow in upgradeArrows)
                {
                    if (arrow != null) arrow.SetActive(true);
                }
            }

            if (next_Damage != null) next_Damage.text = nextPrefab.GetAttackDamage().ToString();
            if (next_Range != null) next_Range.text = nextPrefab.GetAttackRange().ToString();
            if (next_CD != null) next_CD.text = nextPrefab.GetAttackCooldown().ToString() + "s";
            if (next_Slow != null) next_Slow.text = nextPrefab.GetSlowPercentage().ToString("F0") + "%";
            if (next_DoT != null) next_DoT.text = nextPrefab.ui_dotText;
        }
        else
        {
            if (statRows != null)
            {
                foreach (var row in statRows)
                {
                    if (row != null) row.padding = new RectOffset(maxPadding, maxPadding, 0, 0);
                }
            }

            if (currentLevelText != null) currentLevelText.text = "LV." + selectedTower.towerLevel + " (MAX)";
            if (nextLevelText != null) nextLevelText.text = "";

            if (nextStatsGroup != null) nextStatsGroup.SetActive(false);
            if (upgradeButton != null) upgradeButton.interactable = false;
            if (upgradeCostText != null) upgradeCostText.text = "MAX";

            if (next_Damage != null) next_Damage.gameObject.SetActive(false);
            if (next_Range != null) next_Range.gameObject.SetActive(false);
            if (next_CD != null) next_CD.gameObject.SetActive(false);
            if (next_Slow != null) next_Slow.gameObject.SetActive(false);
            if (next_DoT != null) next_DoT.gameObject.SetActive(false);

            if (upgradeArrows != null)
            {
                foreach (var arrow in upgradeArrows)
                {
                    if (arrow != null) arrow.SetActive(false);
                }
            }
        }
    }

    public void UpgradeSelectedTower()
    {
        if (selectedTower == null || selectedTower.nextUpgradePrefab == null) return;

        if (GameManager.instance.HasEnoughCurrency(selectedTower.upgradeCost))
        {
            GameManager.instance.UpdateCurrency(-selectedTower.upgradeCost);
            PlayEffects(upgradeFX, upgradeSound, selectedTower.transform.position);

            Tower oldTower = selectedTower;
            Instantiate(selectedTower.nextUpgradePrefab.gameObject, selectedTower.transform.position, selectedTower.transform.rotation);

            Hide();
            Destroy(oldTower.gameObject);
        }
        else
        {
            if (GameManager.instance.inGameUI != null) GameManager.instance.inGameUI.ShakeCurrencyUI();
        }
    }

    public void SellSelectedTower()
    {
        if (selectedTower == null) return;

        GameManager.instance.UpdateCurrency(selectedTower.sellReward);
        PlayEffects(sellFX, sellSound, selectedTower.transform.position);

        RaycastHit[] hits = Physics.RaycastAll(selectedTower.transform.position + Vector3.up, Vector3.down, 5f);
        foreach (RaycastHit hit in hits)
        {
            BuildSlot slot = hit.collider.GetComponent<BuildSlot>();
            if (slot != null)
            {
                slot.SetSlotAvailableTo(true);
                slot.UnSelectTile();
                break;
            }
        }

        Tower towerToDestroy = selectedTower;
        Hide();
        Destroy(towerToDestroy.gameObject);
    }

    // ★ 幫你把這個小幫手升級了！現在它吃的是 AudioSource，並且使用 .Play()
    private void PlayEffects(GameObject fxPrefab, AudioSource soundSource, Vector3 position)
    {
        if (fxPrefab != null) Instantiate(fxPrefab, position + Vector3.up * 0.1f, Quaternion.identity);
        if (soundSource != null) soundSource.Play();
    }

    private void DetectClickOutside()
    {
        if (!panelObject.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            List<RaycastResult> uiResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, uiResults);

            foreach (RaycastResult result in uiResults)
            {
                if (result.gameObject.transform.IsChildOf(panelObject.transform))
                {
                    return;
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (selectedTower != null && hit.collider.gameObject == selectedTower.gameObject)
                {
                    return;
                }
            }

            Hide();
        }
    }
}