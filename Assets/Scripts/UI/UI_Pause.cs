using UnityEngine;

public class UI_Pause : MonoBehaviour
{
    private UI ui;
    private UI_InGame inGameUI;

    [SerializeField] private GameObject[] pauseUiElements;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        inGameUI = ui.GetComponentInChildren<UI_InGame>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
            ui.SwitchTo(inGameUI.gameObject);
    }

    public void SwitchPauseUIElements(GameObject elementToEnable)
    {
        foreach (GameObject obj in pauseUiElements)
        {
            obj.SetActive(false);
        }

        elementToEnable.SetActive(true);
    }

    // ==========================================
    // 新增：當玩家在暫停選單按下「回到主選單」或「重選關卡」時，
    // 請讓按鈕的 OnClick 事件呼叫這個方法，而不是直接切換 UI！
    // ==========================================
    public void GoToMainMenuOrLevelSelect()
    {
        // 1. 強制清空 3D 場景上的預覽圈跟建造選單
        if (GameManager.instance != null)
        {
            GameManager.instance.ForceClearBuildPreview();
        }

        // 2. 恢復時間 (非常重要，不然退回主選單會被鎖死在 timeScale = 0)
        Time.timeScale = 1f;

        // 3. 呼叫你的 UI 總管，切換回你想顯示的介面 (這裡以MainMenu為例，你需要自己補上 UI_MainMenu 的參照或呼叫方式)
        // 例如： ui.EnableMainMenuUI(true);
    }
    // ==========================================

    private void OnEnable()
    {
        // 1. 暫停時間
        Time.timeScale = 0f;

        // 2. 【新增這段】：在暫停畫面彈出的瞬間，強制沒收場景上的預覽跟蓋塔選單
        if (GameManager.instance != null)
        {
            GameManager.instance.ForceClearBuildPreview();
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }
}