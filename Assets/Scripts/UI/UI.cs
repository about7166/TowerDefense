using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] uiElements;

    [Header("階段 1：Logo 畫面設定")]
    [SerializeField] private GameObject logoScreenUI;
    [SerializeField] private float logoFadeInDuration = 1.5f;
    [SerializeField] private float logoStayTime = 2f;
    [SerializeField] private float logoFadeOutDuration = 1.5f;

    [Header("階段 2：Loading 畫面設定")]
    [SerializeField] private GameObject loadingScreenUI;
    [SerializeField] private TextMeshProUGUI loadingTextTMP;
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

    // 跨場景記憶卡，防止進入關卡時重複播放動畫
    private static bool hasPlayedStartup = false;

    // 用來追蹤與中斷協程的變數
    private Coroutine startupCo;
    private Coroutine textAnimCo;

    private void Awake()
    {
        buildButtonsUI = GetComponentInChildren<UI_BuildButtonsHolder>(true);
        settingUI = GetComponentInChildren<UI_Settings>(true);
        mainMenuUI = GetComponentInChildren<UI_MainMenu>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        animatorUI = GetComponent<UI_Animator>();

        if (!hasPlayedStartup)
        {
            hasPlayedStartup = true;

            if (fadeImageUI != null)
            {
                fadeImageUI.gameObject.SetActive(true);
                Color c = fadeImageUI.color;
                c.a = 1f;
                fadeImageUI.color = c;
            }

            if (logoScreenUI != null) GetOrAddCanvasGroup(logoScreenUI).alpha = 0f;
            if (loadingScreenUI != null) GetOrAddCanvasGroup(loadingScreenUI).alpha = 0f;

            SwitchTo(null);

            // 將協程存入變數，方便我們隨時砍掉它
            startupCo = StartCoroutine(StartupSequenceCo());
        }
        else
        {
            if (logoScreenUI != null) logoScreenUI.SetActive(false);
            if (loadingScreenUI != null) loadingScreenUI.SetActive(false);
            if (fadeImageUI != null) fadeImageUI.gameObject.SetActive(false);
        }
    }

    // 修改：現在只偵測「Z」鍵來跳過開場
    private void Update()
    {
        /* (startupCo != null)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                SkipStartupSequence();
            }
        }
        */
    }

    // 強制中斷所有等待，瞬間進入主介面
    private void SkipStartupSequence()
    {
        // 1. 停止所有的等待協程
        if (startupCo != null) StopCoroutine(startupCo);
        if (textAnimCo != null) StopCoroutine(textAnimCo);
        startupCo = null;

        // 2. 強制關閉遮擋畫面的 UI
        if (logoScreenUI != null)
        {
            GetOrAddCanvasGroup(logoScreenUI).alpha = 0f;
            logoScreenUI.SetActive(false);
        }
        if (loadingScreenUI != null)
        {
            GetOrAddCanvasGroup(loadingScreenUI).alpha = 0f;
            loadingScreenUI.SetActive(false);
        }

        // 3. 恢復時間與正常流程
        Time.timeScale = 1f;

        SwitchTo(settingUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);

        if (GameManager.instance.IsTestingLevel())
            SwitchTo(inGameUI.gameObject);

        ActivateFadeEffect(true);
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

    private IEnumerator StartupSequenceCo()
    {
        Time.timeScale = 0f;
        yield return null;

        if (logoScreenUI != null)
        {
            logoScreenUI.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(logoScreenUI), 0f, 1f, logoFadeInDuration));
            yield return new WaitForSecondsRealtime(logoStayTime);
            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(logoScreenUI), 1f, 0f, logoFadeOutDuration));
            logoScreenUI.SetActive(false);
        }

        if (loadingScreenUI != null)
        {
            loadingScreenUI.SetActive(true);

            // 將文字動畫存入變數，以便跳過時能一起停掉
            if (loadingTextTMP != null) textAnimCo = StartCoroutine(AnimateLoadingTextCo());

            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(loadingScreenUI), 0f, 1f, loadingFadeInDuration));
            yield return new WaitForSecondsRealtime(loadingStayTime);
            yield return StartCoroutine(FadeCanvasGroup(GetOrAddCanvasGroup(loadingScreenUI), 1f, 0f, loadingFadeOutDuration));

            if (textAnimCo != null) StopCoroutine(textAnimCo);
            loadingScreenUI.SetActive(false);
        }

        // 如果順利播完沒有被玩家跳過，在這裡清空變數
        startupCo = null;
        Time.timeScale = 1f;

        SwitchTo(settingUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);

        if (GameManager.instance.IsTestingLevel())
            SwitchTo(inGameUI.gameObject);

        ActivateFadeEffect(true);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float targetAlpha, float duration)
    {
        cg.alpha = startAlpha;
        float time = 0;
        duration = Mathf.Max(duration, 0.01f);

        while (time < duration)
        {
            float safeDelta = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            time += safeDelta;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        cg.alpha = targetAlpha;
    }

    private IEnumerator AnimateLoadingTextCo()
    {
        int dots = 0;
        while (true)
        {
            loadingTextTMP.text = "Loading" + new string('.', dots);
            dots = (dots + 1) % 4;
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
#if UNITY_EDITOR
        // 在 Unity 編輯器中按下離開時執行這個
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 在真正打包出來的遊戲中按下離開時執行這個
        Application.Quit();
#endif
    }

    public void ActivateFadeEffect(bool fadeIn)
    {
        if (fadeImageUI == null) return;
        fadeImageUI.gameObject.SetActive(true);
        if (fadeIn) animatorUI.ChangeColor(fadeImageUI, 0, fadeImageDuration);
        else animatorUI.ChangeColor(fadeImageUI, 1, fadeImageDuration);
    }
}