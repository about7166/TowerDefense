using TMPro;
using UnityEngine;
using UnityEngine.Localization; // 1. 引入 Localization 命名空間

public class UI_AdviceText : MonoBehaviour
{
    private TextMeshProUGUI myText;

    [Header("多國語言隨機提示詞")]
    // 2. 將普通的 string 改成 LocalizedString 陣列
    [SerializeField] private LocalizedString[] localizedAdvices;

    private LocalizedString currentAdvice; // 用來記住這次抽到哪一句

    private void OnEnable()
    {
        if (myText == null)
            myText = GetComponent<TextMeshProUGUI>();

        if (localizedAdvices.Length == 0) return;

        // 3. 隨機抽出一句
        int randomIndex = Random.Range(0, localizedAdvices.Length);
        currentAdvice = localizedAdvices[randomIndex];

        // 4. 綁定事件：當字串準備好，或是語言切換時，自動執行 UpdateText 來換字
        currentAdvice.StringChanged += UpdateText;
    }

    private void OnDisable()
    {
        // 5. 關閉介面時，務必解除綁定，避免產生記憶體錯誤 (Memory Leak)
        if (currentAdvice != null)
        {
            currentAdvice.StringChanged -= UpdateText;
        }
    }

    // 這是一個專門用來替換文字的方法，交給 StringChanged 自動呼叫
    private void UpdateText(string translatedText)
    {
        myText.text = translatedText;
    }
}