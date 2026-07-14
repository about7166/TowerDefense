using UnityEngine;
using UnityEngine.EventSystems;

public class UI_HoverSound : MonoBehaviour, IPointerEnterHandler
{
    [Header("專屬音效 (若不填，則使用 UI.cs 的全域預設音效)")]
    public AudioSource customHoverSFX;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. 如果這個按鈕有設定「專屬音效」，就優先播放專屬的！
        if (customHoverSFX != null)
        {
            customHoverSFX.Play();
        }
        // 2. 如果沒有設定專屬音效，就去找 UI 總管播預設的聲音
        else
        {
            UI uiManager = FindFirstObjectByType<UI>();
            if (uiManager != null && uiManager.onHoverSFX != null)
            {
                uiManager.onHoverSFX.Play();
            }
        }
    }
}