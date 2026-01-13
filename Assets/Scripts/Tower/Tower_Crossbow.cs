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
            towerHead.forward = directionToEnemy;

            IDamagable damagable = hitInfo.transform.GetComponent<IDamagable>();
            damagable.TakeDamage(damage);

            visuals.CreateOnHitFX(hitInfo.point); //擊中特效
            visuals.PlayAttackVFX(gunPoint.position, hitInfo.point);
            visuals.PlayReloaxVFX(attackCooldown);
            AudioManager.instance?.PlaySFX(attackSfx, true);
        }
    }
}
