using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelButton_Tile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private LevelManager levelManager;
    private TileAnimator tileAnimator;

    //  1. 已經刪除 GetComponentInChildren 的舊寫法

    [SerializeField] private int levelIndex;

    //  2. 新增：統一用這兩個變數來進行「手動綁定」
    [SerializeField] private GameObject lockIconObject;
    [SerializeField] private TextMeshPro myText;

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

        Invoke(nameof(CheckIfLevelUnLocked), 0.05f);
    }

    public void CheckIfLevelUnLocked()
    {
        if (levelIndex == 1)
            PlayerPrefs.SetInt("Level_1" + "unlocked", 1);

        unlocked = PlayerPrefs.GetInt("Level_" + levelIndex + "unlocked", 0) == 1;
        UpdateLevelButtonText();
    }

    private void UpdateLevelButtonText()
    {
        //  3. 徹底刪除 transform.Find，直接對 lockIconObject 和 myText 下指令！

        if (unlocked == false)
        {
            // 未解鎖：開啟鎖頭、關閉文字物件
            if (lockIconObject != null) lockIconObject.SetActive(true);
            if (myText != null) myText.gameObject.SetActive(false);
        }
        else
        {
            // 已解鎖：關閉鎖頭、開啟文字物件並更新數字
            if (lockIconObject != null) lockIconObject.SetActive(false);

            if (myText != null)
            {
                myText.gameObject.SetActive(true);
                myText.text = "LV." + levelIndex;
            }
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

        // =================  新增這行  =================
        // 點擊成功的瞬間，通知生怪器停止生怪，並關閉場上怪物的 NavMeshAgent
        FindFirstObjectByType<MainScene_Spawner>()?.StopSpawningAndSink();
        // =================  新增結束  =================

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
        if (Application.isPlaying) return;
    }
}