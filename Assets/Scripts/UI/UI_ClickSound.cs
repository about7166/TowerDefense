using UnityEngine;
using UnityEngine.EventSystems; // 引入事件系統

// 這次我們繼承 IPointerDownHandler，讓滑鼠「按下去的瞬間」就發出聲音，手感最好！
public class UI_ClickSound : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // 尋找場景中的 UI 總管
        UI uiManager = FindFirstObjectByType<UI>();

        // 如果總管存在，而且有放上 onClickSFX，就播放按鈕點擊音效
        if (uiManager != null && uiManager.onClickSFX != null)
        {
            uiManager.onClickSFX.Play();
        }
    }
}