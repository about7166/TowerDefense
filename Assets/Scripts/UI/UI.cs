using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] uiElements;

    private UI_Setting settingUI;
    private UI_MainMenu mainMenuUI;

    public UI_InGame inGameUI { get; private set; }
    public UI_Animator animatorUI { get; private set; }
    public UI_BuildButtonHolder buildButtonsUI { get; private set; }

    private void Awake()
    {
        buildButtonsUI = GetComponentInChildren<UI_BuildButtonHolder>(true);
        settingUI = GetComponentInChildren<UI_Setting>(true);
        mainMenuUI = GetComponentInChildren<UI_MainMenu>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        animatorUI = GetComponent<UI_Animator>();

        //ActivateFadeEffect(true);

        SwitchTo(settingUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);
        //SwitchTo(inGameUI.gameObject);
    }

    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in uiElements)
        {
            ui.SetActive(false);
        }

        if (uiToEnable != null)
            uiToEnable.SetActive(true);
    }

    public void EnableMainMenuUI(bool enable)
    {
        if (enable)
            SwitchTo(mainMenuUI.gameObject);
        else
            SwitchTo(null);
    }

    public void EnableInGameUI(bool enable)
    {
        if (enable)
            SwitchTo(inGameUI.gameObject);
        else
        {
            inGameUI.SnapTimerToDefaultPosition();
            SwitchTo(null);
        }
    }

    public void QuitButton()
    {
        if (EditorApplication.isPlaying)
            EditorApplication.isPlaying = false;
        else
            Application.Quit();
    }

    public void ActivateFadeEffect(bool fadeIn)
    {
        if (fadeIn)
            animatorUI.ChangeColor(fadeImageUI, 0, 2);
        else
            animatorUI.ChangeColor(fadeImageUI, 1, 2);
    }
}
