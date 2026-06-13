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
    // 1. 去掉 "Health : "，只保留純數字顯示 (例如 20/20)
    public void UpdateHealthPointsUI(int currentHp, int maxHp)
    {
        healthPointText.text = currentHp + "/" + maxHp;
    }

    // 2. 去掉 "$ "，只保留純金錢數字 (例如 1000)
    public void UpdateCurrencyUI(int value)
    {
        currencyText.text = value.ToString();
    }

    // 3. 將原本的 "Seconds : 20" 升級成精緻的倒數計時格式 (例如 00:20)
    public void UpdateWaveTimerUI(float value)
    {
        int minutes = Mathf.FloorToInt(value / 60f);
        int seconds = Mathf.FloorToInt(value % 60f);
        waveTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    public void EnableWaveTimer(bool enable)
    {
        // 關鍵防護：如果 UI 本身已經關閉了，或者物件正在被毀滅中，就直接跳過
        if (this == null || !gameObject.activeInHierarchy)
            return;

        if (waveTimer == null) return;

        RectTransform rect = waveTimer.GetComponent<RectTransform>();
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;
        Vector3 offset = new Vector3(0, yOffset);

        // 防護：確保 uiAnimator 還活著
        if (uiAnimator == null) uiAnimator = GetComponentInParent<UI_Animator>();
        if (uiAnimator == null) return;

        if (waveTimerMoveCo != null) StopCoroutine(waveTimerMoveCo);

        waveTimerMoveCo = StartCoroutine(uiAnimator.ChangePositionCo(rect, offset));

        if (waveTimerTextBlinkEffect != null)
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
