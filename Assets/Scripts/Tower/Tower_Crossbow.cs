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
            Vector3 lookDir = directionToEnemy;
            lookDir.y = 0; // 把上下傾斜的角度拿掉，只留水平方向

            if (lookDir != Vector3.zero)
            {
                towerHead.forward = lookDir; // 這樣它就永遠不會低頭了！
            }

            // ============ 👇 加上安全鎖 👇 ============
            IDamagable damagable = hitInfo.transform.GetComponent<IDamagable>();

            // 如果射中的東西真的有血條腳本 (是活著的怪物)，才進行扣血
            if (damagable != null)
            {
                damagable.TakeDamage(damage);
            }

            // 確保不管射中怪物還是射空，視覺特效跟「拉弓裝填」的動作都必須照常執行！
            visuals.CreateOnHitFX(hitInfo.point);
            visuals.PlayAttackVFX(gunPoint.position, hitInfo.point);
            visuals.PlayReloaxVFX(attackCooldown);
            AudioManager.instance?.PlaySFX(attackSfx, true);
            // ============ 👆 修改結束 👆 ============
        }
    }
}
