using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] uiElements;

    private UI_Animator animatorUI;
    private UI_Setting settingUI;
    private UI_MainMenu mainMenuUI;
    private UI_InGame inGameUI;

    private void Awake()
    {
        settingUI = GetComponentInChildren<UI_Setting>(true);
        mainMenuUI = GetComponentInChildren<UI_MainMenu>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        animatorUI = GetComponentInChildren<UI_Animator>();

        //ActivateFadeEffect(true);

        SwitchTo(settingUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);
        SwitchTo(inGameUI.gameObject);
    }

    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in uiElements)
        {
            ui.SetActive(false);
        }

        uiToEnable.SetActive(true);
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
