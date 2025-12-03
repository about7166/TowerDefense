using UnityEditor;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject[] uiElements;

    private UI_Setting uiSetting;
    private UI_MainMenu uiMainMenu;
    private void Awake()
    {
        uiSetting = GetComponentInChildren<UI_Setting>(true);
        uiMainMenu = GetComponentInChildren<UI_MainMenu>(true);

        SwitchTo(uiSetting.gameObject);
        SwitchTo(uiMainMenu.gameObject);
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
}
