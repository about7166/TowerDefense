using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ 新增這行：為了控制 Text (TMP)

public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] uiElements;

    // ============  階段一：Logo 設定  ============
    [Header("階段 1：Logo 畫面設定")]
    [SerializeField] private GameObject logoScreenUI;
    [SerializeField] private float logoFadeInDuration = 1.5f;
    [SerializeField] private float logoStayTime = 2f;
    [SerializeField] private float logoFadeOutDuration = 1.5f;

    // ============  階段二：Loading 設定  ============
    [Header("階段 2：Loading 畫面設定")]
    [SerializeField] private GameObject loadingScreenUI;
    [SerializeField] private TextMeshProUGUI loadingTextTMP; //  用來放你的 Loading 文字
    [SerializeField] private float loadingFadeInDuration = 1.5f;
    [SerializeField] private float loadingStayTime = 3f;
    [SerializeField] private float loadingFadeOutDuration = 1f;

    [Header("黑幕淡入淡出設定")]
    [SerializeField] private float fadeImageDuration = 1.5f;

    private UI_Settings settingUI;
    private UI_MainMenu mainMenuUI;

    public UI_InGame inGameUI { get; private set; }
    public UI_Animator animatorUI { get; private set; }
    public UI_BuildButtonsHolder buildButtonsUI { get; private set; }

    [Header("UI音效")]
    public AudioSource onHoverSFX;
    public AudioSource onClickSFX;

    private void Awake()
    {
        buildButtonsUI = GetComponentInChildren<UI_BuildButtonsHolder>(true);
        settingUI = GetComponentInChildren<UI_Settings>(true);
        mainMenuUI = GetComponentInChildren<UI_MainMenu>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        animatorUI = GetComponent<UI_Animator>();

        // ============  新增這段魔法  ============
        // 遊戲一啟動，強制叫出黑幕，並把透明度 (Alpha) 設為 1 (完全不透明)
        // 它會變成一塊死黑的背景，安靜地墊在 Logo 和 Loading 後面
        if (fadeImageUI != null)
        {
            fadeImageUI.gameObject.SetActive(true);
            Color c = fadeImageUI.color;
            c.a = 1f;
            fadeImageUI.color = c;
        }
        // ============  新增這段魔法  ============

        // 確保兩個畫面都有 CanvasGroup，且一開始都是全透明的
        if (logoScreenUI != null) GetOrAddCanvasGroup(logoScreenUI).alpha = 0f;
        if (loadingScreenUI != null) GetOrAddCanvasGroup(loadingScreenUI).alpha = 0f;

        // 隱藏所有常規介面
        SwitchTo(null);

        // 啟動史詩級開場序列！
        StartCoroutine(StartupSequenceCo());
    }

    // 取得或自動加入 CanvasGroup 的小工具
    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

    private IEnumerator StartupSequenceCo()
    {
        // 凍結主場景時間
        Time.timeScale = 0f;

        //  終極解法：讓程式先空轉一影格，放掉 Unity 剛啟動時那巨大的「卡頓時間差」！
        yield return null;

        // --- 階段 1：顯示 Logo ---
        if (logoScreenUI != null)
        {
            logoScreenUI.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(logoScreenUI), 0f, 1f, logoFadeInDuration));
            yield return new WaitForSecondsRealtime(logoStayTime);
            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(logoScreenUI), 1f, 0f, logoFadeOutDuration));
            logoScreenUI.SetActive(false);
        }

        // --- 階段 2：顯示 Loading 與動態文字 ---
        if (loadingScreenUI != null)
        {
            loadingScreenUI.SetActive(true);

            // 啟動文字點點點的小動畫
            Coroutine textAnimCo = null;
            if (loadingTextTMP != null) textAnimCo = StartCoroutine(AnimateLoadingTextCo());

            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(loadingScreenUI), 0f, 1f, loadingFadeInDuration));
            yield return new WaitForSecondsRealtime(loadingStayTime);
            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(loadingScreenUI), 1f, 0f, loadingFadeOutDuration));

            // Loading 結束，關閉文字小動畫
            if (textAnimCo != null) StopCoroutine(textAnimCo);
            loadingScreenUI.SetActive(false);
        }

        // --- 開場結束，恢復遊戲 ---
        Time.timeScale = 1f;

        SwitchTo(settingUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);

        if (GameManager.instance.IsTestingLevel())
            SwitchTo(inGameUI.gameObject);

        ActivateFadeEffect(true);
    }

    // 將漸變邏輯獨立出來，利用 Mathf.Lerp 確保時間精準，保證滑順！
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float targetAlpha, float duration)
    {
        // 防護 1：物件啟用的瞬間，強制鎖定為起始透明度，絕對不准閃爍！
        cg.alpha = startAlpha;

        float time = 0;
        duration = Mathf.Max(duration, 0.01f); // 防呆：避免秒數被填 0

        while (time < duration)
        {
            // 防護 2：時間防護罩！
            // 不管 Unity 卡頓多久，我們強制規定每一次迴圈最多只能增加 0.1 秒。
            // 這樣就算系統卡了 2 秒，動畫也不會瞬間跳到結尾，保證能看到漸變過程！
            float safeDelta = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            time += safeDelta;

            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        // 結束時精準對齊目標數值
        cg.alpha = targetAlpha;
    }

    // 控制 Loading 點點點的專屬動畫
    private IEnumerator AnimateLoadingTextCo()
    {
        int dots = 0;
        while (true)
        {
            // 每 0.5 秒變換一次：Loading ➔ Loading. ➔ Loading.. ➔ Loading...
            loadingTextTMP.text = "Loading" + new string('.', dots);
            dots = (dots + 1) % 4; // 數字會在 0, 1, 2, 3 之間循環
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private void Start()
    {
        if (settingUI != null)
            settingUI.LoadSettings();
    }

    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in uiElements)
            ui.SetActive(false);

        if (uiToEnable != null)
            uiToEnable.SetActive(true);
    }

    public void EnableMainMenuUI(bool enable)
    {
        if (enable) SwitchTo(mainMenuUI.gameObject);
        else SwitchTo(null);
    }

    public void EnableInGameUI(bool enable)
    {
        if (enable) SwitchTo(inGameUI.gameObject);
        else
        {
            inGameUI.SnapTimerToDefaultPosition();
            SwitchTo(null);
        }
    }

    public void QuitButton()
    {
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        else Application.Quit();
    }

    public void ActivateFadeEffect(bool fadeIn)
    {
        // 防呆：確認你有綁定黑幕物件
        if (fadeImageUI == null) return;

        //  解放 Scene 視窗的魔法：不管你在 Inspector 有沒有打勾，程式執行到這行時，直接強制幫它打勾！
        fadeImageUI.gameObject.SetActive(true);

        // 執行漸變動畫
        if (fadeIn)
            animatorUI.ChangeColor(fadeImageUI, 0, fadeImageDuration);
        else
            animatorUI.ChangeColor(fadeImageUI, 1, fadeImageDuration);
    }
}