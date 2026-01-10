using UnityEngine;

public class Enemy_Heavy : Enemy
{
    [Header("敵人設定")]
    [SerializeField] private int shieldAmount = 50;
    [SerializeField] private Enemy_Shield shieldObject;

    protected override void Start()
    {
        base.Start();
        EnableShieldIfNeeded();
    }

    private void EnableShieldIfNeeded() //護盾跟著隱形
    {
        if (shieldObject != null)
        {
            shieldObject.gameObject.SetActive(true);
            shieldObject.SetupShield(shieldAmount);
        }
    }
}
