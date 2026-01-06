using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UI_BuildButtonsHolder : MonoBehaviour
{
    private UI_Animator uiAnimator;

    [SerializeField] private float yPositionOffset;
    [SerializeField] private float openAnimationDuration = 0.1f;
    private bool isBuildMenuActive;

    private UI_BuildButtonOnHoverEffect[] buildButtonEffects;
    private UI_BuildButton[] buildButtons;

    private List<UI_BuildButton> unlockedButtons;
    private UI_BuildButton lastSelectedButton;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        buildButtonEffects = GetComponentsInChildren<UI_BuildButtonOnHoverEffect>();
        buildButtons = GetComponentsInChildren<UI_BuildButton>();
    }
    private void Update()
    {
        CheckBuildButtonsHotkeys();
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

        if (Input.GetKeyDown(KeyCode.Space) && lastSelectedButton != null)
            lastSelectedButton.BuildTower();

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

    public void SetLastSelected(UI_BuildButton newLastSelected) => lastSelectedButton = newLastSelected;
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
