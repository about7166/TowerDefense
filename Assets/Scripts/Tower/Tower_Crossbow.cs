using UnityEngine;

public class Tower_Crossbow : Tower
{

    private Crossbow_Visuals visuals;

    [Header("弩的細項")]
    [SerializeField] private int damage;

    protected override void Awake()
    {
        base.Awake();
        visuals = GetComponent<Crossbow_Visuals>();
    }
    protected override void Attack()
    {
        base.Attack();
        Vector3 directionToEnemy = DirectionToEnemyFrom(gunPoint);

        if (Physics.Raycast(gunPoint.position, directionToEnemy, out RaycastHit hitInfo, Mathf.Infinity, whatIsTargetable))
        {
            // ============ 👇 修改重點：強制水平轉向 👇 ============

            Vector3 lookDir = directionToEnemy;
            lookDir.y = 0; // 把上下傾斜的角度拿掉，只留水平方向

            if (lookDir != Vector3.zero)
            {
                towerHead.forward = lookDir; // 這樣它就永遠不會低頭了！
            }

            // ============ 👆 修改結束 👆 ============

            IDamagable damagable = hitInfo.transform.GetComponent<IDamagable>();
            damagable.TakeDamage(damage);

            visuals.CreateOnHitFX(hitInfo.point);
            visuals.PlayAttackVFX(gunPoint.position, hitInfo.point);
            visuals.PlayReloaxVFX(attackCooldown);
            AudioManager.instance?.PlaySFX(attackSfx, true);
        }
    }
}
