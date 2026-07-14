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

    public override void RemoveEnemy()
    {
        // 你的程式碼裡應該有一個迴圈或變數在通知觀察中的魚叉塔
        foreach (var tower in observingTowers) // (你的變數名稱可能略有不同)
        {
            if (tower != null)
            {
                tower.ResetAttack();
            }
        }

        base.RemoveEnemy();
    }
}
