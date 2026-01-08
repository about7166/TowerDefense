using UnityEngine;

public class Enemy_Heavy : Enemy
{
    [Header("¼Ä¤H³]©w")]
    [SerializeField] private int shieldAmount = 50;
    [SerializeField] private Enemy_Shield shieldObject;

    protected override void Awake()
    {
        base.Awake();

        if (shieldObject != null)
        {
            shieldObject.gameObject.SetActive(true);
            shieldObject.SetupShield(shieldAmount);
        }
    }
}
