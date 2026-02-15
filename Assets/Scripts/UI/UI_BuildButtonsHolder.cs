using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UI_BuildButtonsHolder : MonoBehaviour
{
    private UI_Animator uiAnimator;

    // ★ 1. 新增：用來抓取 BuildManager，讓我們可以隨時查勤
    private BuildManager buildManager;

    [SerializeField] private float yPositionOffset;
    [SerializeField] private float openAnimationDuration = 0.1f;
    private bool isBuildMenuActive;

    private UI_BuildButtonOnHoverEffect[] buildButtonEffects;
    private UI_BuildButton[] buildButtons;

    private List<UI_BuildButton> unlockedButtons;
    private UI_BuildButton lastSelectedButton;
    private Transform previewTower;

    // ★ 2. 新增：用來記錄「上一次選到的是哪一塊地」
    private BuildSlot trackedSlot;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        buildButtonEffects = GetComponentsInChildren<UI_BuildButtonOnHoverEffect>();
        buildButtons = GetComponentsInChildren<UI_BuildButton>();

        // ★ 3. 取得 BuildManager
        buildManager = FindFirstObjectByType<BuildManager>();
    }

    private void Update()
    {
        CheckBuildButtonsHotkeys();
        CheckIfSlotChanged(); // ★ 4. 每一幀檢查地塊有沒有被切換
    }

    // ★ 5. 終極殺招：只要發現換地塊了，立刻清除所有殘留的預覽！
    private void CheckIfSlotChanged()
    {
        if (buildManager == null) return;

        BuildSlot currentSlot = buildManager.GetSelectedSlot();

        // 如果現在選擇的地塊，跟我們記錄的不同 (代表玩家點了新地塊，或是按右鍵取消選擇了)
        if (currentSlot != trackedSlot)
        {
            trackedSlot = currentSlot; // 更新記錄

            // 強制清除所有按鈕的預覽狀態
            foreach (var button in buildButtons)
            {
                if (button != null)
                    button.SelectButton(false);
            }

            lastSelectedButton = null;
            previewTower = null;
        }
    }

    private void CheckBuildButtonsHotkeys()
    {
        if (isBuildMenuActive == false)
            return;

        for (int i = 0; i < unlockedButtons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectNewButton(i);
                break;
            }
        }

        if (lastSelectedButton != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                lastSelectedButton.ConfirmTowerBuild();
                previewTower = null;
            }

            if (Input.GetKeyDown(KeyCode.Q))
                RotateTarget(previewTower, -90);

            if (Input.GetKeyDown(KeyCode.E))
                RotateTarget(previewTower, 90);
        }
    }

    private void RotateTarget(Transform target, float angel)
    {
        if (target == null)
            return;

        target.Rotate(0, angel, 0);
        target.GetComponent<ForwardAttackDisplay>()?.UpdateLines();
    }

    public void SelectNewButton(int buttonIndex)
    {
        if (buttonIndex >= unlockedButtons.Count)
            return;

        foreach (var button in unlockedButtons)
        {
            button.SelectButton(false);
        }

        UI_BuildButton selectedButton = unlockedButtons[buttonIndex];

        selectedButton.SelectButton(true);
    }

    public UI_BuildButton[] GetBuildButtons() => buildButtons;
    public List<UI_BuildButton> GetUnlockedButtons() => unlockedButtons;
    public UI_BuildButton GetLastSelectedButton() => lastSelectedButton;

    public void SetLastSelected(UI_BuildButton newLastSelected, Transform newPreview)
    {
        lastSelectedButton = newLastSelected;
        previewTower = newPreview;
    }

    public void UpdateUnlockedButtons()
    {
        unlockedButtons = new List<UI_BuildButton>();

        foreach (var button in buildButtons)
        {
            if (button.buttonUnlocked)
                unlockedButtons.Add(button);
        }
    }

    public void ShowBuildButtons(bool showButtons)
    {
        isBuildMenuActive = showButtons;

        // 原本的強制清除保留著也沒關係，多一層防護
        foreach (var button in buildButtons)
        {
            if (button != null)
                button.SelectButton(false);
        }
        lastSelectedButton = null;
        previewTower = null;

        float yOffset = isBuildMenuActive ? yPositionOffset : -yPositionOffset;
        float methodDelay = isBuildMenuActive ? openAnimationDuration : 0;

        uiAnimator.ChangePosition(transform, new Vector3(0, yOffset), openAnimationDuration);
        Invoke(nameof(ToggleButtonMovement), methodDelay);
    }

    private void ToggleButtonMovement()
    {
        foreach (var button in buildButtonEffects)
        {
            button.ToggleMovement(isBuildMenuActive);
        }
    }
}