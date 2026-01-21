using UnityEngine;

public class Projectile_Harpoon : MonoBehaviour
{
    private Tower_Harpoon tower;
    private bool isAttached;
    private float speed;
    private Enemy enemy;

    [SerializeField] private Transform connectionPoint;

    private void Update()
    {
        if (enemy == null || isAttached)
            return;

        MoveTowersEnemy();

        if (Vector3.Distance(transform.position, enemy.transform.position) < 0.35f)
            AttachToEnemy();
    }

    private void MoveTowersEnemy()
    {
        transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
        transform.forward = enemy.transform.position - transform.position;
    }

    private void AttachToEnemy()
    {
        isAttached = true;
        transform.parent = enemy.transform;
        tower.ActivateAttack();
    }

    public void SetupProjectile(Enemy newEnemy, float newSpeed, Tower_Harpoon newTower)
    {
        speed = newSpeed;
        enemy = newEnemy;
        tower = newTower;
    }

    public Transform GetConnectionPoint()
    {
        if (connectionPoint == null)
            return transform;

        return connectionPoint;
    }
}
