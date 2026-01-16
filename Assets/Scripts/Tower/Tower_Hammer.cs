using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using RangeAttribute = UnityEngine.RangeAttribute;

public class Tower_Hammer : Tower
{
    private Hammer_Visuals hammerVisuals;

    [Header("槌子設定")]
    [Range(0, 1)]
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private float slowDuration;

    protected override void Awake()
    {
        base.Awake();
        hammerVisuals = GetComponent<Hammer_Visuals>();
    }

    protected override void Update()
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
            enemy.SlowEnemy(slowMultiplier, slowDuration);
        }
    }

    private List<Enemy> ValidEnemyTargets()
    {
        List<Enemy> targets = new List<Enemy>();
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsTargetable);

        foreach (Collider enemy in enemiesAround)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();

            if (newEnemy != null)
                targets.Add(newEnemy);
        }

        return targets;
    }

    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && AtLeastOneEnemyAround();
    }

    private bool AtLeastOneEnemyAround()
    {
        Collider[] enemyColliders = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);
        return enemyColliders.Length > 0;
    }
}
