using UnityEngine;

public class UI_Hyperlink : MonoBehaviour
{
    [SerializeField] private string url;

    public void OpenURL()
    {
        // 防呆機制升級版：判斷 null、空字串，或是只有空白鍵
        if (string.IsNullOrEmpty(url) || url.Trim() == "")
        {
            // 印出這句話，讓我們確認程式有成功擋下它！
            Debug.Log("成功攔截！網址是空的，沒有執行開啟網頁。");
            return;
        }

        Application.OpenURL(url);
    }
}