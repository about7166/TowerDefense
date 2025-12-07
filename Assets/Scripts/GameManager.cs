using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int currency;
    
    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;

    private UI_InGame inGameUI;

    private void Awake()
    {
        inGameUI = FindFirstObjectByType<UI_InGame>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        currentHp = maxHp;
        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
    }

    public void UpdateHp(int value)
    {
        currentHp += value;
        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
    }

    public void UpdateCurrency(int value)
    {
        currency += value;
        inGameUI.UpdateCurrencyUI(currency);
    }
}
