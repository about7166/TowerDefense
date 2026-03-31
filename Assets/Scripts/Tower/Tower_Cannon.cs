using UnityEngine;

public class Tower_Cannon : Tower
{
    [Header("大砲設定")]
    [SerializeField] private float damage;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float timeToTarget = 1.5f;
    [SerializeField] private ParticleSystem attackVFX;

    // ★ 新增：指派那個只會上下轉的頭
    [SerializeField] private Transform cannonHead;

    private Collider[] explosionBuffer = new Collider[50];

    protected override void Attack()
    {
        base.Attack();

        Vector3 velocity = CalculateLaunchVelocity();
        attackVFX.Play();

        GameObject newProjectile = objectPool.Get(projectilePrefab, gunPoint.position, Quaternion.identity);
        newProjectile.GetComponent<Projectile_Cannon>().SetupProjectile(velocity, damage, objectPool);
    }

    // ... (FindEnemyWithinRange 與 EnemiesAroundEnemy 保持不變)

    protected override void HandleRotation()
    {
        if (currentEnemy == null)
            return;

        // ★ 修改：拆分兩軸旋轉邏輯
        FaceLaunchDirection();
    }

    private void FaceLaunchDirection()
    {
        // 1. 取得拋物線發射向量
        Vector3 launchVelocity = CalculateLaunchVelocity();
        if (launchVelocity == Vector3.zero) return;

        // 2. 水平旋轉 (作用於 base，即 towerHead)
        // 我們只取水平方向的向量
        Vector3 horizontalDir = new Vector3(launchVelocity.x, 0, launchVelocity.z);
        if (horizontalDir != Vector3.zero)
        {
            Quaternion horizontalRotation = Quaternion.LookRotation(horizontalDir);
            towerHead.rotation = Quaternion.Slerp(towerHead.rotation, horizontalRotation, rotationSpeed * Time.deltaTime);
        }

        // 3. 垂直旋轉 (作用於 cannonHead)
        if (cannonHead != null)
        {
            // 將 launchVelocity 轉為 LookRotation，它包含了正確的仰角
            Quaternion fullLookRotation = Quaternion.LookRotation(launchVelocity);

            // 我們只需要它的 X 軸（上下仰角）
            // 透過 Lerp 讓轉動平滑
            float targetPitch = fullLookRotation.eulerAngles.x;
            Quaternion currentLocalRot = cannonHead.localRotation;
            Quaternion targetLocalRot = Quaternion.Euler(targetPitch, 0, 0);

            cannonHead.localRotation = Quaternion.Slerp(currentLocalRot, targetLocalRot, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 CalculateLaunchVelocity()
    {
        if (currentEnemy == null) return Vector3.zero;

        Vector3 direction = currentEnemy.CenterPoint() - gunPoint.position;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
        float distanceXZ = directionXZ.magnitude;

        Vector3 velocityXZ = directionXZ.normalized * (distanceXZ / timeToTarget);

        // 使用物理公式計算 Y 軸初始速度
        float yVelocity = (direction.y - 0.5f * Physics.gravity.y * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        return velocityXZ + Vector3.up * yVelocity;
    }
}