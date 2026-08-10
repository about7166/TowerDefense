using TMPro;
using UnityEngine;

public class UI_InGame : MonoBehaviour
{
    private UI ui;
    private UI_Pause uiPause;
    private UI_Animator uiAnimator;

    [SerializeField] private TextMeshProUGUI healthPointText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [Header("波次進度 (如 1/6)")]
    [SerializeField] private TextMeshProUGUI waveProgressText;
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

    // 用來鎖死原始座標的變數
    private Vector3 currencyDefaultPos;
    private Vector3 healthDefaultPos;
    private Transform currencyParent;
    private Transform healthParent;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UI_Animator>();
        ui = GetComponentInParent<UI>();
        uiPause = ui.GetComponentInChildren<UI_Pause>(true);

        if (waveTimer != null)
        {
            waveTimerDefaultPosition = waveTimer.localPosition;
        }

        if (currencyText != null) currencyParent = currencyText.transform.parent;
        if (healthPointText != null) healthParent = healthPointText.transform.parent;

        if (currencyParent != null) currencyDefaultPos = currencyParent.localPosition;
        if (healthParent != null) healthDefaultPos = healthParent.localPosition;
    }

    // ★ 新增修復：每次這個 UI 介面被打開時，強制把所有結算畫面隱藏！
    private void OnEnable()
    {
        EnableGameOverUI(false);
        EnableVictoryUI(false);
        EnableLevelCompletedUI(false);
    }

    private void Update()
    {
        // 判斷：如果遊戲已經進入結算畫面，就直接返回，禁止使用 ESC 叫出暫停選單
        if (IsGameEnded()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            ui.SwitchTo(uiPause.gameObject);
    }

    // 判斷現在是不是處於「結算狀態」
    private bool IsGameEnded()
    {
        bool isGameOver = gameOverUI != null && gameOverUI.activeSelf;
        bool isVictory = victoryUI != null && victoryUI.activeSelf;
        bool isLevelCompleted = levelCompletedUI != null && levelCompletedUI.activeSelf;

        return isGameOver || isVictory || isLevelCompleted;
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

    public void ShakeCurrencyUI()
    {
        if (currencyParent != null) currencyParent.localPosition = currencyDefaultPos;
        ui.animatorUI.Shake(currencyParent);
    }

    public void ShakeHealthUI()
    {
        if (healthParent != null) healthParent.localPosition = healthDefaultPos;
        ui.animatorUI.Shake(healthParent);
    }

    public void UpdateHealthPointsUI(int currentHp, int maxHp)
    {
        healthPointText.text = currentHp + "/" + maxHp;
    }

    public void UpdateCurrencyUI(int value)
    {
        currencyText.text = value.ToString();
    }

    public void UpdateWaveProgressUI(int currentWave, int maxWaves)
    {
        if (waveProgressText != null)
        {
            // 將傳進來的數字組合成 "1/6" 的格式
            waveProgressText.text = currentWave + "/" + maxWaves;
        }
    }
    public void UpdateWaveTimerUI(float value)
    {
        int minutes = Mathf.FloorToInt(value / 60f);
        int seconds = Mathf.FloorToInt(value % 60f);
        waveTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void EnableWaveTimer(bool enable)
    {
        if (this == null || !gameObject.activeInHierarchy)
            return;

        if (waveTimer == null) return;

        RectTransform rect = waveTimer.GetComponent<RectTransform>();
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;
        Vector3 offset = new Vector3(0, yOffset);

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