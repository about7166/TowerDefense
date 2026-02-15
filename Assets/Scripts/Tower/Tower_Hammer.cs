using System.Collections.Generic;
using UnityEngine;

public class Tower_Hammer : Tower
{
    private Hammer_Visuals hammerVisuals;

    [Header("槌子設定")]
    [SerializeField] private float damage = 15f;

    [Range(0, 1)]
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private float slowDuration;

    // ★ 優化 1：宣告在類別層級，重複使用這些容器，不再產生記憶體垃圾
    private Collider[] collidersBuffer = new Collider[50]; // 假設範圍內最多同時有 50 個敵人
    private List<Enemy> validTargets = new List<Enemy>();

    protected override void Awake()
    {
        base.Awake();
        hammerVisuals = GetComponent<Hammer_Visuals>();
    }

    protected override void FixedUpdate()
    {
        if (towerActive == false)
            return;

        if (CanAttack())
            Attack();
    }

    protected override void Attack()
    {
        base.Attack();
        hammerVisuals.PlayAttackAnimation();

        foreach (var enemy in ValidEnemyTargets())
        {
            enemy.TakeDamage(damage);

            if (enemy.gameObject.activeSelf)
            {
                enemy.SlowEnemy(slowMultiplier, slowDuration);
            }
        }
    }

    private List<Enemy> ValidEnemyTargets()
    {
        // ★ 優化 2：清空舊名單，重複使用
        validTargets.Clear();

        // ★ 優化 3：使用 NonAlloc 方法，將掃描到的敵人填入我們預先準備好的 collidersBuffer
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, attackRange, collidersBuffer, whatIsTargetable);

        // 只讀取實際抓到的數量 (hitCount)
        for (int i = 0; i < hitCount; i++)
        {
            Enemy newEnemy = collidersBuffer[i].GetComponent<Enemy>();

            if (newEnemy != null)
                validTargets.Add(newEnemy);
        }

        return validTargets;
    }

    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && AtLeastOneEnemyAround();
    }
}