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
    [SerializeField] private Image icon_Single; // ★ 新增：箭矢圖案

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
    [SerializeField] private Image icon_Damage;       // ★ 新增：劍圖案
    [SerializeField] private TextMeshProUGUI label_Damage; // ★ 新增："Damage" 文字
    [SerializeField] private TextMeshProUGUI value_Damage;

    [SerializeField] private Image icon_Range;
    [SerializeField] private TextMeshProUGUI label_Range;
    [SerializeField] private TextMeshProUGUI value_Range;

    [SerializeField] private Image icon_CD;
    [SerializeField] private TextMeshProUGUI label_CD;
    [SerializeField] private TextMeshProUGUI value_CD;

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
            // 1. 更新上方的攻擊類型 (背景換圖 + 文字與圖示透明度)
            UpdateSkillVisual(bg_Single, text_Single, icon_Single, towerData.isSingleTarget);
            UpdateSkillVisual(bg_AoE, text_AoE, icon_AoE, towerData.isAoE);
            UpdateSkillVisual(bg_Slow, text_Slow, icon_Slow, towerData.isSlow);
            UpdateSkillVisual(bg_Ground, text_Ground, icon_Ground, towerData.canAttackGround);
            UpdateSkillVisual(bg_Air, text_Air, icon_Air, towerData.canAttackAir);
            UpdateSkillVisual(bg_DoT, text_DoT, icon_DoT, towerData.hasDoT);

            // 2. 更新下方的屬性數值 (填入數字，並根據數值決定要不要變半透明)
            if (value_Damage != null) value_Damage.text = towerData.ui_damage.ToString();
            if (value_Range != null) value_Range.text = towerData.GetAttackRange().ToString();
            if (value_CD != null) value_CD.text = towerData.GetAttackCooldown().ToString() + "s";
            if (value_DoTAttr != null) value_DoTAttr.text = towerData.ui_dotText;

            // 讓下方的屬性圖示與文字也具備智慧半透明功能 (例如沒有 DoT 時，整行變暗)
            UpdateAttributeVisual(icon_Damage, label_Damage, value_Damage, towerData.ui_damage > 0);
            UpdateAttributeVisual(icon_Range, label_Range, value_Range, towerData.GetAttackRange() > 0);
            UpdateAttributeVisual(icon_CD, label_CD, value_CD, towerData.GetAttackCooldown() > 0);
            UpdateAttributeVisual(icon_DoTAttr, label_DoTAttr, value_DoTAttr, towerData.hasDoT);
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

        // 計算透明度：有技能為 1 (完全不透明)，沒技能為 80/255 (約 31%)
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

    // 這個魔法可以同時吃下 Image 和 TextMeshProUGUI，幫它們改透明度！
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