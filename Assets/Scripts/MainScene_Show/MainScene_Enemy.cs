using UnityEngine;

// 繼承自原本的 Enemy，保留特效但擁有獨立的行動迴圈
public class MainScene_Enemy : Enemy
{
    // 專屬的初始化，直接接收主場景中設定好的環狀點位
    public void SetupMainScene(Transform[] points)
    {
        myWaypoints = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            myWaypoints[i] = points[i].position;
        }

        ResetEnemy();

        currentWaypointIndex = 0;
        nextWaypointIndex = 0;
        agent.SetDestination(myWaypoints[0]);
    }

    // 覆寫「是否到達終點」，移除到達最後一點會停止的設定
    protected override bool ShouldChangeWaypoint()
    {
        return agent.remainingDistance < 0.5f;
    }

    // 覆寫「換下一個點」，加入無限迴圈邏輯
    protected override void ChangeWaypoint()
    {
        if (myWaypoints == null || myWaypoints.Length == 0) return;

        nextWaypointIndex++;

        // 如果超過最後一個點，就歸零回到第一個點，達成無限繞圈
        if (nextWaypointIndex >= myWaypoints.Length)
        {
            nextWaypointIndex = 0;
        }

        currentWaypointIndex = nextWaypointIndex;
        agent.SetDestination(myWaypoints[nextWaypointIndex]);
    }
}