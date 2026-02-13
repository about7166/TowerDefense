using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
    private CameraController camController;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25;

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