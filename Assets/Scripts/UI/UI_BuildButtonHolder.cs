using UnityEngine;

public class UI_BuildButtonHolder : MonoBehaviour
{
    private UI_Animator uiAnimator;

    [SerializeField] private float yPositionOffset;
    [SerializeField] private float openAnimationDuration = 0.1f;
    private bool isBuildMenuActive;

    private UI_BuildButtonOnHoverEffect[] buildButtonEffects;
    private UI_BuildButton[] buildButtons;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        buildButtonEffects = GetComponentsInChildren<UI_BuildButtonOnHoverEffect>();
        buildButtons = GetComponentsInChildren<UI_BuildButton>();
    }

    public UI_BuildButton[] GetBuildButtons() => buildButtons;

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
