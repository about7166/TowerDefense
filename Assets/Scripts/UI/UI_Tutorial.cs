using UnityEngine;
using UnityEngine.UI;

public class UI_Tutorial : MonoBehaviour
{
    [Header("頁面設定")]
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Image[] dots;

    // ★ 新增這兩行：用來存放「亮起」和「暗下」的圖片
    [Header("點點圖示")]
    [SerializeField] private Sprite activeDotSprite;   // 藍色發光點點
    [SerializeField] private Sprite inactiveDotSprite; // 暗色/空心點點

    [Header("控制按鈕")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject nextIcon;
    [SerializeField] private GameObject finishIcon;

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
        // 呼叫 UI 總管，請它幫我們切換回主選單！
        FindFirstObjectByType<UI>().EnableMainMenuUI(true);
        Debug.Log("教學完成，回到主選單！");
    }
}