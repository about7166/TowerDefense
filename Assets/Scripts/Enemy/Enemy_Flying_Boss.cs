using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Flying_Boss : Enemy_Flying
{
    [Header("Boss設定")]
    [SerializeField] private GameObject bossUnitPrefab;
    [SerializeField] private int amountToCreate = 150;
    [SerializeField] private float cooldown = 0.05f;
    private float creationTimer;

    private List<Enemy> createdEnemies = new List<Enemy>();

    protected override void Start()
    {
        
    }

    protected override void Update()
    {
        base.Update();

        creationTimer -= Time.deltaTime;

        if (creationTimer < 0 && amountToCreate > 0)
        {
            creationTimer = cooldown;
            CreateNewBossUnit();
        }
    }

    private void CreateNewBossUnit()
    {
        amountToCreate--;
        GameObject newUnit = Instantiate(bossUnitPrefab, transform.position, Quaternion.identity);

        Enemy_BossUnit bossUnit = newUnit.GetComponent<Enemy_BossUnit>();

        bossUnit.SetupEnemy(GetFinalWaypoint(),this,myPortal);

        createdEnemies.Add(bossUnit);
    }

    private void EliminateAllUnits()
    {
        foreach (Enemy enemy in createdEnemies)
        {
            enemy.Die();
        }
    }

    public override void Die()
    {
        EliminateAllUnits();
        base.Die();
    }
}
