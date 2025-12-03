using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UI_Setting : MonoBehaviour
{
    private CameraController camController;

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
    }

    private void OnEnable()
    {
        keyboardSensitivitySlider.value = PlayerPrefs.GetFloat(keyboardSensitivityParameter, 0.6f);
        mouseSensitivitySlider.value = PlayerPrefs.GetFloat(mouseSensitivityParameter, 0.6f);
    }
}
