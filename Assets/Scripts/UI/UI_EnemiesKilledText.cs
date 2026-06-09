using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class UI_EnemiesKilledText : MonoBehaviour
{
    private TextMeshProUGUI myText;
    private GameManager gameManager;

    [Header("多國語言設定")]
    [SerializeField] private LocalizedString enemyKilledPrefix;
    [SerializeField] private LocalizedString healthPrefix;

    private void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        UpdateVictoryStats();
    }

    private void UpdateVictoryStats()
    {
        // 1. 取得系統翻譯好的文字 (例如 "擊殺敵人：" 和 "剩餘血量：")
        string translatedEnemy = enemyKilledPrefix.GetLocalizedString();
        string translatedHealth = healthPrefix.GetLocalizedString();

        // 2. 取得實際的殺敵數
        int kills = gameManager.enemiesKilled;

        // 3. 透過剛剛開啟的通道 (大寫開頭的屬性)，安全取得真實血量！
        int currentHp = gameManager.CurrentHp;
        int maxHp = gameManager.MaxHp;

        // 4. 將翻譯文字與數字組合，並使用 \n 換行顯示
        myText.text = $"{translatedEnemy} {kills}\n{translatedHealth} {currentHp}/{maxHp}";
    }
}