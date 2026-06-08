using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization.Settings;

public class UI_Settings : MonoBehaviour
{
    private CameraController camController;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25;

    [Header("左側分頁切換 (Tabs)")]
    [SerializeField] private GameObject[] tabActiveBackgrounds; // 用來放那條藍色的 BG_Active
    [SerializeField] private GameObject[] contentPanels;        // 用來放右邊的 SubPanel_...

    [Header("語言設定")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("SFX設定")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private string sfxParameter;
    [SerializeField] private TextMeshProUGUI sfxSliderText;

    [Header("BGM設定")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string bgmParameter;
    [SerializeField] private TextMeshProUGUI bgmSliderText;

    [Header("鍵盤靈敏度")]
    [SerializeField] private Slider keyboardSensitivitySlider;
    [SerializeField] private TextMeshProUGUI keyboardSensitivityText;
    [SerializeField] private string keyboardSensitivityParameter = "keyboardSensitivity";

    [SerializeField] private float minKeyboardSensitivity = 60;
    [SerializeField] private float maxKeyboardSensitivity = 240;

    [Header("滑鼠靈敏度")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private TextMeshProUGUI mouseSensitivityText;
    [SerializeField] private string mouseSensitivityParameter = "mouseSensitivity";

    [SerializeField] private float minMouseSensitivity = 1;
    [SerializeField] private float maxMouseSensitivity = 10;

    private void Start()
    {
        // ★ 剛進遊戲時，強制讀取並套用音量！
        LoadSettings();
    }

    private void OnEnable()
    {
        camController = FindFirstObjectByType<CameraController>();
        LoadSettings();

        // 每次打開設定面板，強制切回第一個分頁 (0 = Volume)
        SwitchTab(0);

        // 新增這行：開啟面板時，同步下拉選單的顯示
        StartCoroutine(SyncLanguageDropdown());
    }

    // ★ 1. 把 private 改成 public，並刪除 UI_Settings 裡的 Start() 方法 (因為用不到了)
    public void LoadSettings()
    {
        float savedBgm = PlayerPrefs.GetFloat(bgmParameter, 0.6f);
        float savedSfx = PlayerPrefs.GetFloat(sfxParameter, 0.6f);
        float savedMouse = PlayerPrefs.GetFloat(mouseSensitivityParameter, 0.6f);
        float savedKeyboard = PlayerPrefs.GetFloat(keyboardSensitivityParameter, 0.6f);

        bgmSlider.value = savedBgm;
        sfxSlider.value = savedSfx;
        mouseSensitivitySlider.value = savedMouse;
        keyboardSensitivitySlider.value = savedKeyboard;

        BGMSliderValue(savedBgm);
        SFXSliderValue(savedSfx);

        // ★ 2. 新增這行防呆：如果在主選單剛啟動時還沒抓到相機，就先試著抓抓看
        if (camController == null)
            camController = FindFirstObjectByType<CameraController>();

        if (camController != null)
        {
            MouseSensitivity(savedMouse);
            KeyboardSensitivity(savedKeyboard);
        }
    }

    public void SFXSliderValue(float value)
    {
        float safeValue = Mathf.Max(0.0001f, value);
        float newValue = Mathf.Log10(safeValue) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);
        sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void BGMSliderValue(float value)
    {
        float safeValue = Mathf.Max(0.0001f, value);
        float newValue = Mathf.Log10(safeValue) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);
        bgmSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void KeyboardSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minKeyboardSensitivity, maxKeyboardSensitivity, value);
        if (camController != null)
            camController.AdjustKeyboardSensitivity(newSensitivity);

        keyboardSensitivityText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void MouseSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minMouseSensitivity, maxMouseSensitivity, value);
        if (camController != null)
            camController.AdjustMouseSensitivity(newSensitivity);

        mouseSensitivityText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    // 切換分頁的核心邏輯
    public void SwitchTab(int tabIndex)
    {
        // 1. 先把所有的「藍色高光背景」和「右側面板」通通關閉
        for (int i = 0; i < contentPanels.Length; i++)
        {
            if (tabActiveBackgrounds[i] != null) tabActiveBackgrounds[i].SetActive(false);
            if (contentPanels[i] != null) contentPanels[i].SetActive(false);
        }

        // 2. 只把「被點擊的那個 (tabIndex)」打開
        if (tabIndex < tabActiveBackgrounds.Length && tabActiveBackgrounds[tabIndex] != null)
            tabActiveBackgrounds[tabIndex].SetActive(true);

        if (tabIndex < contentPanels.Length && contentPanels[tabIndex] != null)
            contentPanels[tabIndex].SetActive(true);
    }

    // 給 Dropdown 呼叫的公開方法
    public void ChangeLanguage(int localeID)
    {
        StartCoroutine(SetLocale(localeID));
    }

    // 實際執行切換語言的協程 (確保 Localization 系統準備好才切換)
    private IEnumerator SetLocale(int localeID)
    {
        // 等待 Localization 系統初始化完成
        yield return LocalizationSettings.InitializationOperation;

        // 根據 Dropdown 傳來的數字 (0, 1, 2) 切換到對應的語言
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
    }

    // ★ 負責同步下拉選單與目前語言的方法
    private System.Collections.IEnumerator SyncLanguageDropdown()
    {
        // 1. 等待多國語言系統準備好
        yield return LocalizationSettings.InitializationOperation;

        // 2. 取得目前系統記憶的語言，以及所有的語言清單
        var currentLocale = LocalizationSettings.SelectedLocale;
        var availableLocales = LocalizationSettings.AvailableLocales.Locales;

        // 3. 找出目前語言在清單裡是第幾個 (0=英文, 1=繁中, 2=簡中)
        int index = availableLocales.IndexOf(currentLocale);

        // 4. 把下拉選單切換到對應的數字。
        // 使用 SetValueWithoutNotify 是為了「只改變畫面顯示」，不觸發 OnValueChanged 去重新載入一次語言
        if (languageDropdown != null && index >= 0)
        {
            languageDropdown.SetValueWithoutNotify(index);
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(keyboardSensitivityParameter, keyboardSensitivitySlider.value);
        PlayerPrefs.SetFloat(mouseSensitivityParameter, mouseSensitivitySlider.value);
        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, bgmSlider.value);

        // ★ 確保存檔確實寫入硬碟
        PlayerPrefs.Save();
    }
}