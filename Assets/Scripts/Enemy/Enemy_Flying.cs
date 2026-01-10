using UnityEngine;

public class Enemy_Flying : Enemy
{
    protected override void Start()
    {
        base.Start();

        agent.SetDestination(GetFinalWaypoint());
    }
}
