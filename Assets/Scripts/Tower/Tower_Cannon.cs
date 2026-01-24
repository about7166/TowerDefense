using UnityEngine;

public class Tower_Cannon : Tower
{
    [Header("大砲設定")]
    [SerializeField] private float damage;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float timeToTarget = 1.5f;
    [SerializeField] private ParticleSystem attackVFX;

    protected override void Attack()
    {
        base.Attack();

        Vector3 velocity = CalculateLaunchVelocity();
        attackVFX.Play();

        GameObject newProjectile = objectPool.Get(projectilePrefab, gunPoint.position, Quaternion.identity);
        newProjectile.GetComponent<Projectile_Cannon>().SetupProjectile(velocity, damage, objectPool);
    }

    protected override Enemy FindEnemyWithinRange()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);
        Enemy bestTarget = null;
        int maxNearByEnemies = 0;

        foreach (Collider enemy in enemiesAround)
        {
            int amountOfEnemiesAround = EnemiesAroundEnemy(enemy.transform);

            if (amountOfEnemiesAround > maxNearByEnemies)
            {
                maxNearByEnemies = amountOfEnemiesAround;
                bestTarget = enemy.GetComponent<Enemy>();
            }
        }

        return bestTarget;
    }

    private int EnemiesAroundEnemy(Transform enemyToCheck)
    {
        Collider[] enemiesAround = Physics.OverlapSphere(enemyToCheck.position, 1, whatIsEnemy);
        return enemiesAround.Length;
    }

    protected override void HandleRotation()
    {
        if (currentEnemy == null)
            return;

        RotateBodyTowardsEnemy();
        FaceLaunchDirection();
    }

    private void FaceLaunchDirection()
    {
        Vector3 attackDirection = CalculateLaunchVelocity();
        Quaternion lookRotation = Quaternion.LookRotation(attackDirection);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation.x, towerHead.eulerAngles.y, 0);
    }

    private Vector3 CalculateLaunchVelocity()
    {
        Vector3 direction = currentEnemy.CenterPoint() - gunPoint.position;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z); //平面距離
        Vector3 velocityXZ = directionXZ / timeToTarget; //速度 = 距離 / 時間

        float yVelocity = (direction.y - (Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / 2) / timeToTarget; //s=v0t+1/2at^2
        Vector3 launchVelocity = velocityXZ + (Vector3.up * yVelocity);

        return launchVelocity;
    }
}
