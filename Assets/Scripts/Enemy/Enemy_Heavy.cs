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
        if (shieldObject != null & shieldAmount > 0)
            shieldObject.gameObject.SetActive(true);
    }

    public override void TakeDamage(int damage)
    {
        if (shieldAmount > 0)
        {
            shieldAmount -= damage;
            shieldObject.ActivateShieldImpact();

            if (shieldAmount <= 0)
                shieldObject.gameObject.SetActive(false);
        }
        else
            base.TakeDamage(damage);
    }
}
