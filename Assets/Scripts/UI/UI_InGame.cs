using TMPro;
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

    [SerializeField] private Transform waveTimer;
    private Coroutine waveTimerMoveCo;
    private Vector3 waveTimerDefaultPosition;

    [Header("勝利或失敗")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject levelCompletedUI;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        ui = GetComponentInParent<UI>();
        uiPause = ui.GetComponentInChildren<UI_Pause>(true);

        if (waveTimer != null)
        {
            waveTimerDefaultPosition = waveTimer.localPosition;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            ui.SwitchTo(uiPause.gameObject);
    }

    public void EnableGameOverUI(bool enable)
    {
        if (gameOverUI != null)
            gameOverUI.SetActive(enable);
    }
    public void EnableVictoryUI(bool enable)
    {
        if (victoryUI != null)
            victoryUI.SetActive(enable);
    }

    public void EnableLevelCompletedUI(bool enable)
    {
        if (levelCompletedUI != null)
            levelCompletedUI.SetActive(enable);
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
        RectTransform rect = waveTimer.GetComponent<RectTransform>();
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;

        Vector3 offset = new Vector3(0, yOffset);


        waveTimerMoveCo = StartCoroutine(uiAnimator.ChangePositionCo(rect, offset));
        waveTimerTextBlinkEffect.EnableBlink(enable);
    }

    public void SnapTimerToDefaultPosition()
    {
        if (waveTimer == null)
            return;

        if (waveTimerMoveCo != null)
            StopCoroutine(waveTimerMoveCo);

        waveTimer.localPosition = waveTimerDefaultPosition;
    }

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.StartNewWave();
    }
}
