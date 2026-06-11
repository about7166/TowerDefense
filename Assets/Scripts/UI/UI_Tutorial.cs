using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_Tutorial : MonoBehaviour
{
    [Header("頁面設定")]
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Image[] dots;

    // 新增這兩行：用來存放「亮起」和「暗下」的圖片
    [Header("點點圖示")]
    [SerializeField] private Sprite activeDotSprite;   // 藍色發光點點
    [SerializeField] private Sprite inactiveDotSprite; // 暗色/空心點點

    [Header("控制按鈕")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject nextIcon;
    [SerializeField] private GameObject finishIcon;

    // 2. 新增這行：這會在 Inspector 產生一個專屬的事件框框
    [Header("完成教學的動作")]
    public UnityEvent onTutorialFinished;

    private int currentPageIndex = 0;

    private void OnEnable()
    {
        currentPageIndex = 0;
        UpdateTutorialUI();
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            UpdateTutorialUI();
        }
        else
        {
            FinishTutorial();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateTutorialUI();
        }
    }

    private void UpdateTutorialUI()
    {
        // 1. 切換頁面顯示
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }

        // ★ 2. 核心修改：切換點點的圖片 (Sprite)
        for (int i = 0; i < dots.Length; i++)
        {
            // 如果是當前頁面，就塞入「藍色點點圖片」，否則塞入「暗色點點圖片」
            dots[i].sprite = (i == currentPageIndex) ? activeDotSprite : inactiveDotSprite;
        }

        // 3. 按鈕邏輯：第一頁隱藏 Prev
        prevButton.gameObject.SetActive(currentPageIndex > 0);

        // 4. 按鈕邏輯：最後一頁將箭頭換成打勾
        bool isLastPage = (currentPageIndex == pages.Length - 1);
        nextIcon.SetActive(!isLastPage);
        finishIcon.SetActive(isLastPage);
    }

    private void FinishTutorial()
    {
        // ★ 3. 把原本寫死的回到主選單刪掉，改成呼叫我們在 Inspector 設定的事件！
        onTutorialFinished.Invoke();
        Debug.Log("教學完成！");
    }
}