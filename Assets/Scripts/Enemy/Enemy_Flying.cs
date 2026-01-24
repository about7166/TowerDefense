using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Flying : Enemy
{
    private List<Tower_Harpoon> observingTowers = new List<Tower_Harpoon>();

    protected override void Start()
    {
        base.Start();

        agent.SetDestination(GetFinalWaypoint());
    }

    public override float DistanceToFinishLine()
    {
         return Vector3.Distance(transform.position, GetFinalWaypoint());
    }

    public void AddObservingTower(Tower_Harpoon newTower) => observingTowers.Add(newTower);

    public override void DestroyEnemy()
    {
        foreach (var tower in observingTowers)
            tower.ResetAttack();

        foreach (var harpoon in GetComponentsInChildren<Projectile_Harpoon>())
            objectPool.Remove(harpoon.gameObject);

        base.DestroyEnemy();
    }
}
