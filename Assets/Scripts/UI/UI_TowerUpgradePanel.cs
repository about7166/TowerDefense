using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UI_TowerUpgradePanel : MonoBehaviour
{
    public static UI_TowerUpgradePanel instance;
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

    [Header("LV.1 (目前數值)")]
    [SerializeField] private TextMeshProUGUI current_Damage;
    [SerializeField] private TextMeshProUGUI current_Range;
    [SerializeField] private TextMeshProUGUI current_CD;
    [SerializeField] private TextMeshProUGUI current_Slow;
    [SerializeField] private TextMeshProUGUI current_DoT;

    [Header("LV.2 (升級後數值)")]
    [SerializeField] private GameObject nextStatsGroup;
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
    [SerializeField] private GameObject upgradeFX;
    [SerializeField] private GameObject sellFX;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip sellSound;

    private void Awake()
    {
        instance = this;
        panelObject.SetActive(false);

        if (upgradeButton != null) upgradeButton.onClick.AddListener(UpgradeSelectedTower);
        if (sellButton != null) sellButton.onClick.AddListener(SellSelectedTower);
    }

    private void Update() => DetectClickOutside();

    public void SelectTower(Tower tower)
    {
        if (selectedTower != null && selectedTower != tower)
            selectedTower.ToggleHighlight(false);

        selectedTower = tower;
        selectedTower.ToggleHighlight(true);

        panelObject.SetActive(true);
        UpdateUI();

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

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(SlidePanel(hiddenPosY, true));
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

        // ★ 自動抓取塔的名字和大圖示
        if (towerNameText != null) towerNameText.text = selectedTower.towerName;
        if (towerIconImage != null && selectedTower.towerIcon != null) towerIconImage.sprite = selectedTower.towerIcon;

        // ★ 自動抓取塔的兩個屬性小圖示
        if (attributeIcon1_Image != null && selectedTower.attributeIcon1 != null) attributeIcon1_Image.sprite = selectedTower.attributeIcon1;
        if (attributeIcon2_Image != null && selectedTower.attributeIcon2 != null) attributeIcon2_Image.sprite = selectedTower.attributeIcon2;

        // ★ 自動抓取目前的屬性和賣出的價錢
        if (current_Damage != null) current_Damage.text = selectedTower.GetAttackDamage().ToString();
        if (current_Range != null) current_Range.text = selectedTower.GetAttackRange().ToString();
        if (current_CD != null) current_CD.text = selectedTower.GetAttackCooldown().ToString() + "s";
        if (current_Slow != null) current_Slow.text = selectedTower.GetSlowPercentage().ToString("F0") + "%";
        if (current_DoT != null) current_DoT.text = selectedTower.ui_dotText;

        if (sellRewardText != null) sellRewardText.text = selectedTower.sellReward.ToString();

        Tower nextPrefab = selectedTower.nextUpgradePrefab;

        if (nextPrefab != null)
        {
            // ★ 如果有下一等：標題顯示目前的等級
            if (currentLevelText != null) currentLevelText.text = "LV." + selectedTower.towerLevel;

            if (nextStatsGroup != null) nextStatsGroup.SetActive(true);
            if (upgradeButton != null) upgradeButton.interactable = true;
            if (upgradeCostText != null) upgradeCostText.text = selectedTower.upgradeCost.ToString();

            if (next_Damage != null) next_Damage.text = nextPrefab.GetAttackDamage().ToString();
            if (next_Range != null) next_Range.text = nextPrefab.GetAttackRange().ToString();
            if (next_CD != null) next_CD.text = nextPrefab.GetAttackCooldown().ToString() + "s";
            if (next_Slow != null) next_Slow.text = nextPrefab.GetSlowPercentage().ToString("F0") + "%";
            if (next_DoT != null) next_DoT.text = nextPrefab.ui_dotText;
        }
        else
        {
            // ★ 如果滿等了：標題自動加上 (MAX)！
            if (currentLevelText != null) currentLevelText.text = "LV." + selectedTower.towerLevel + " (MAX)";

            if (nextStatsGroup != null) nextStatsGroup.SetActive(false);
            if (upgradeButton != null) upgradeButton.interactable = false;
            if (upgradeCostText != null) upgradeCostText.text = "MAX";
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

    private void PlayEffects(GameObject fxPrefab, AudioClip clip, Vector3 position)
    {
        if (fxPrefab != null) Instantiate(fxPrefab, position + Vector3.up * 0.1f, Quaternion.identity);
        if (clip != null) AudioSource.PlayClipAtPoint(clip, position);
    }

    private void DetectClickOutside()
    {
        if (!panelObject.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (selectedTower != null && hit.collider.gameObject == selectedTower.gameObject) return;
            }

            Hide();
        }
    }
}