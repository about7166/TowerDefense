using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class UI_InGame : MonoBehaviour
{
    private UI ui;
    private UI_Pause uiPause;
    private UI_Animator uiAnimator;

    [SerializeField] private TextMeshProUGUI healthPointText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [Space]
    [SerializeField] private TextMeshProUGUI waveTimerText;
    [SerializeField] private float waveTimerOffset;
    [SerializeField] UI_TextBlinkEffect waveTimerTextBlinkEffect;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        ui = GetComponentInParent<UI>();
        uiPause = ui.GetComponentInChildren<UI_Pause>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            ui.SwitchTo(uiPause.gameObject);
    }

    public void ShakeCurrencyUI() => ui.animatorUI.Shake(currencyText.transform.parent);
    public void ShakeHealthUI() => ui.animatorUI.Shake(healthPointText.transform.parent);
    //這裡是老師寫的"威脅值"
    //public void UpdateHealthPointsUI(int value, int maxValue)
    //{
    //int newValue = maxValue - value;
    //healthPointText.text = "Threat :" + newValue + "/" + maxValue;
    //}

    //"威脅值"改成"血量
    public void UpdateHealthPointsUI(int currentHp, int maxHp) => healthPointText.text = "Health : " + currentHp + "/" + maxHp;

    public void UpdateCurrencyUI(int value) => currencyText.text = "Resources : " + value;

    public void UpdateWaveTimerUI(float value) => waveTimerText.text = "Seconds : " + value.ToString("00");
    public void EnableWaveTimer(bool enable)
    {
        Transform waveTimerTransform = waveTimerText.transform.parent;
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;

        Vector3 offset = new Vector3(0, yOffset);


        uiAnimator.ChangePosition(waveTimerTransform, offset);
        waveTimerTextBlinkEffect.EnableBlink(enable);
    }

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.StartNewWave();
    }
}
