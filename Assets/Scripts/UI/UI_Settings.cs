using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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


    private void Awake()
    {
        camController = FindFirstObjectByType<CameraController>();
    }

    public void SFXSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);

        sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void BGMSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);

        bgmSliderText.text = Mathf.Round(value * 100) + "%";
    }

    public void KeyboardSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minKeyboardSensitivity,maxKeyboardSensitivity, value);
        camController.AdjustKeyboardSensitivity(newSensitivity);

        //拉桿的值的顯示方式
        keyboardSensitivityText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void MouseSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minMouseSensitivity,maxMouseSensitivity, value);
        camController.AdjustMouseSensitivity(newSensitivity);

        //拉桿的值的顯示方式
        mouseSensitivityText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(keyboardSensitivityParameter, keyboardSensitivitySlider.value);
        PlayerPrefs.SetFloat(mouseSensitivityParameter, mouseSensitivitySlider.value);
        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, bgmSlider.value);
    }

    private void OnEnable()
    {
        keyboardSensitivitySlider.value = PlayerPrefs.GetFloat(keyboardSensitivityParameter, 0.6f);
        mouseSensitivitySlider.value = PlayerPrefs.GetFloat(mouseSensitivityParameter, 0.6f);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, 0.6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, 0.6f);
    }
}
