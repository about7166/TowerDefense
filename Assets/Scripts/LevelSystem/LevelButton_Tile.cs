using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButton_Tile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private LevelManager levelManager;
    private TileAnimator tileAnimator;
    private TextMeshPro myText => GetComponentInChildren<TextMeshPro>();

    // ★ 注意：這裡已經把 [SerializeField] private GameObject lockIcon 刪掉了，不需要再手動綁定了！

    [SerializeField] private int levelIndex;

    private Vector3 defaultPosition;
    private Coroutine currentMoveCo;
    private Coroutine moveToDefaultCo;

    private bool canClick;
    private bool unlocked;

    private void Awake()
    {
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        levelManager = FindAnyObjectByType<LevelManager>();
        defaultPosition = transform.position;

        // ★ 關鍵修改：延遲 0.05 秒執行，確保所有的 Clone 物件都已經複製完畢
        Invoke(nameof(CheckIfLevelUnLocked), 0.05f);
    }

    public void CheckIfLevelUnLocked()
    {
        // 第一關預設解鎖
        if (levelIndex == 1)
            PlayerPrefs.SetInt("Level_1" + "unlocked", 1);

        // 讀取解鎖狀態
        unlocked = PlayerPrefs.GetInt("Level_" + levelIndex + "unlocked", 0) == 1;
        UpdateLevelButtonText();
    }

    private void UpdateLevelButtonText()
    {
        // ★ 動態尋找名叫 "LockIcon" 的子物件，絕對不會因為 Clone 而抓錯人！
        Transform iconTransform = transform.Find("LockIcon");
        GameObject currentLockIcon = iconTransform != null ? iconTransform.gameObject : null;

        if (unlocked == false)
        {
            // 未解鎖：顯示鎖頭，並把文字變成「空字串」 (不關閉物件)
            if (currentLockIcon != null) currentLockIcon.SetActive(true);
            if (myText != null) myText.text = "";
        }
        else
        {
            // 已解鎖：隱藏鎖頭，顯示正確數字
            if (currentLockIcon != null) currentLockIcon.SetActive(false);
            if (myText != null) myText.text = "LV." + levelIndex;
        }
    }

    public void EnableClickOnButton(bool enable) => canClick = enable;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canClick == false)
            return;

        if (unlocked == false)
        {
            Debug.Log("關卡鎖住了!");
            //發出音效
            return;
        }

        transform.position = defaultPosition;
        //要跟SCENE關卡的名稱一致
        levelManager.LoadLevelFromMenu("Level_" + levelIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tileAnimator.IsGridMoving())
            return;

        if (currentMoveCo != null)
            Invoke(nameof(MoveToDefault), tileAnimator.GetTravelDuration());
        else
            MoveToDefault();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileAnimator.IsGridMoving())
            return;

        MoveTileUp();
    }

    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        currentMoveCo = StartCoroutine(tileAnimator.MoveTileCo(transform, targetPosition));
    }

    private void MoveToDefault()
    {
        moveToDefaultCo = StartCoroutine(tileAnimator.MoveTileCo(transform, defaultPosition));
    }

    private void OnValidate()
    {
        levelIndex = transform.GetSiblingIndex() + 1;

        // ★ 新增防呆：如果是「遊戲執行中(Play Mode)」，就不要讓它強制改字，才不會覆蓋掉我們設定的空字串！
        if (Application.isPlaying) return;

        if (myText != null)
            myText.text = "LV." + levelIndex;
    }
}