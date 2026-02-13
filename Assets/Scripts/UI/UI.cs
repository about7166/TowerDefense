using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] uiElements;

    private UI_Settings settingUI;
    private UI_MainMenu mainMenuUI;

    public UI_InGame inGameUI { get; private set; }
    public UI_Animator animatorUI { get; private set; }
    public UI_BuildButtonsHolder buildButtonsUI { get; private set; }

    [Header("UI音效")]
    public AudioSource onHoverSFX;
    public AudioSource onClickSFX;

    private void Awake()
    {
        buildButtonsUI = GetComponentInChildren<UI_BuildButtonsHolder>(true);
        settingUI = GetComponentInChildren<UI_Settings>(true);
        mainMenuUI = GetComponentInChildren<UI_MainMenu>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        animatorUI = GetComponent<UI_Animator>();

        ActivateFadeEffect(true);

        SwitchTo(settingUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);

        if (GameManager.instance.IsTestingLevel())
            SwitchTo(inGameUI.gameObject);
    }

    // ★ 新增這段：請總管 UI.cs 在遊戲啟動時，強制命令 settingUI 讀取音量！
    private void Start()
    {
        if (settingUI != null)
        {
            settingUI.LoadSettings();
        }
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
        if (fadeImageUI.gameObject.activeSelf == false)
            return;

        if (fadeIn)
            animatorUI.ChangeColor(fadeImageUI, 0, 1.5f);
        else
            animatorUI.ChangeColor(fadeImageUI, 1, 1.5f);
    }
}
