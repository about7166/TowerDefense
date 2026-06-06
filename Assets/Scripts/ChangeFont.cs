using UnityEngine;
using TMPro; // 記得加上這一行

public class FontChanger : MonoBehaviour
{
    public TMP_FontAsset newFont; // 把你的新字體拖進這裡

    [ContextMenu("Change All Fonts")] // 這樣你在 Inspector 可以直接按右鍵執行
    public void ChangeAllFonts()
    {
        // 找到場景中所有的 TMP 文字物件
        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>();
        foreach (TMP_Text text in allTexts)
        {
            text.font = newFont;
        }
        Debug.Log("所有字體已更換！");
    }
}