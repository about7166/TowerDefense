using UnityEngine;

public class Enemy_BossUnit : Enemy
{
    private Vector3 savedDestination;
    private Vector3 lastKnownBossPosition;
    private Enemy_Flying_Boss myBoss;

    protected override void Update()
    {
        base.Update();

        if (myBoss != null)
            lastKnownBossPosition = myBoss.transform.position;
    }

    public void SetupEnemy(Vector3 destination, Enemy_Flying_Boss myNewBoss, EnemyPortal myNewPoratl)
    {
        myBoss = myNewBoss;

        myPortal = myNewPoratl;
        myPortal.GetActiveEnemies().Add(gameObject);

        savedDestination = destination;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Enemy")
            return;

        if (Vector3.Distance(transform.position, lastKnownBossPosition) > 3f) //座標修正：如果著陸時離 Boss 太遠（超過3單位）這裡可以調整
            transform.position = lastKnownBossPosition + new Vector3(0, -1, 0);        

        rb.useGravity = false;
        rb.isKinematic = true;

        agent.enabled = true;
        agent.SetDestination(savedDestination);
    }
}
