using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TowerInfoPanel : MonoBehaviour
{
    [Header("面板排版設定")]
    [SerializeField] private RectTransform myRectTransform;
    [Tooltip("面板距離驚嘆號的 Y 軸高度")]
    [SerializeField] private float yOffset = 150f;

    [Header("圖片素材")]
    [SerializeField] private Sprite haveSkillSprite;
    [SerializeField] private Sprite noSkillSprite;

    [Header("=== 攻擊類型 (Attack Type) ===")]
    [SerializeField] private Image bg_Single;
    [SerializeField] private TextMeshProUGUI text_Single;
    [SerializeField] private Image icon_Single;

    [SerializeField] private Image bg_AoE;
    [SerializeField] private TextMeshProUGUI text_AoE;
    [SerializeField] private Image icon_AoE;

    [SerializeField] private Image bg_Slow;
    [SerializeField] private TextMeshProUGUI text_Slow;
    [SerializeField] private Image icon_Slow;

    [SerializeField] private Image bg_Ground;
    [SerializeField] private TextMeshProUGUI text_Ground;
    [SerializeField] private Image icon_Ground;

    [SerializeField] private Image bg_Air;
    [SerializeField] private TextMeshProUGUI text_Air;
    [SerializeField] private Image icon_Air;

    [SerializeField] private Image bg_DoT;
    [SerializeField] private TextMeshProUGUI text_DoT;
    [SerializeField] private Image icon_DoT;


    [Header("=== 屬性數值 (Attributes) ===")]
    [SerializeField] private Image icon_Damage;
    [SerializeField] private TextMeshProUGUI label_Damage;
    [SerializeField] private TextMeshProUGUI value_Damage;

    [SerializeField] private Image icon_Range;
    [SerializeField] private TextMeshProUGUI label_Range;
    [SerializeField] private TextMeshProUGUI value_Range;

    [SerializeField] private Image icon_CD;
    [SerializeField] private TextMeshProUGUI label_CD;
    [SerializeField] private TextMeshProUGUI value_CD;

    // ★ 新增：Slow 屬性的三個元件
    [SerializeField] private Image icon_SlowAttr;
    [SerializeField] private TextMeshProUGUI label_SlowAttr;
    [SerializeField] private TextMeshProUGUI value_SlowAttr;

    [SerializeField] private Image icon_DoTAttr;
    [SerializeField] private TextMeshProUGUI label_DoTAttr;
    [SerializeField] private TextMeshProUGUI value_DoTAttr;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel(RectTransform buttonRect, Tower towerData)
    {
        gameObject.SetActive(true);

        Vector3 newPosition = buttonRect.position;
        newPosition.y += yOffset;
        myRectTransform.position = newPosition;

        if (towerData != null)
        {
            // 1. 更新上方的攻擊類型
            UpdateSkillVisual(bg_Single, text_Single, icon_Single, towerData.isSingleTarget);
            UpdateSkillVisual(bg_AoE, text_AoE, icon_AoE, towerData.isAoE);
            UpdateSkillVisual(bg_Slow, text_Slow, icon_Slow, towerData.isSlow);
            UpdateSkillVisual(bg_Ground, text_Ground, icon_Ground, towerData.canAttackGround);
            UpdateSkillVisual(bg_Air, text_Air, icon_Air, towerData.canAttackAir);
            UpdateSkillVisual(bg_DoT, text_DoT, icon_DoT, towerData.hasDoT);

            // 2. 更新下方的屬性數值
            if (value_Damage != null) value_Damage.text = towerData.GetAttackDamage().ToString();
            if (value_Range != null) value_Range.text = towerData.GetAttackRange().ToString();
            if (value_CD != null) value_CD.text = towerData.GetAttackCooldown().ToString() + "s";
            if (value_DoTAttr != null) value_DoTAttr.text = towerData.ui_dotText;

            // ★ 新增：讀取緩速 % 數並加上 "%" 符號
            if (value_SlowAttr != null) value_SlowAttr.text = towerData.GetSlowPercentage().ToString("F0") + "%";

            // 讓下方的屬性圖示與文字也具備智慧半透明功能
            UpdateAttributeVisual(icon_Damage, label_Damage, value_Damage, towerData.GetAttackDamage() > 0);
            UpdateAttributeVisual(icon_Range, label_Range, value_Range, towerData.GetAttackRange() > 0);
            UpdateAttributeVisual(icon_CD, label_CD, value_CD, towerData.GetAttackCooldown() > 0);
            UpdateAttributeVisual(icon_DoTAttr, label_DoTAttr, value_DoTAttr, towerData.hasDoT);

            // ★ 新增：如果緩速 % 數大於 0，這個欄位才會亮起
            UpdateAttributeVisual(icon_SlowAttr, label_SlowAttr, value_SlowAttr, towerData.GetSlowPercentage() > 0);
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    // ================= 以下為核心魔法 =================

    private void UpdateSkillVisual(Image bgImage, TextMeshProUGUI textComponent, Image iconImage, bool hasSkill)
    {
        if (bgImage != null)
            bgImage.sprite = hasSkill ? haveSkillSprite : noSkillSprite;

        float targetAlpha = hasSkill ? 1f : (80f / 255f);

        SetAlpha(textComponent, targetAlpha);
        SetAlpha(iconImage, targetAlpha);
    }

    private void UpdateAttributeVisual(Image iconImage, TextMeshProUGUI labelText, TextMeshProUGUI valueText, bool isActive)
    {
        float targetAlpha = isActive ? 1f : (80f / 255f);

        SetAlpha(iconImage, targetAlpha);
        SetAlpha(labelText, targetAlpha);
        SetAlpha(valueText, targetAlpha);
    }

    private void SetAlpha(Graphic graphicElement, float alpha)
    {
        if (graphicElement != null)
        {
            Color c = graphicElement.color;
            c.a = alpha;
            graphicElement.color = c;
        }
    }
}