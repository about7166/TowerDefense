using UnityEngine;

public class Projectile_Cannon : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private Rigidbody rb;
    private float damage;

    [SerializeField] private float damageRadius;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private GameObject explosionFx;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    public void SetupProjectile(Vector3 newVelocity, float newDamage, ObjectPoolManager newPool)
    {
        trail.Clear();
        objectPool = newPool;
        rb.linearVelocity = newVelocity;
        damage = newDamage;
    }

    private void DamageEnemiesAround()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, damageRadius, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            IDamagable damagable = enemy.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(damage);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ✅ 修正版：直接檢查對方腳本，不用管 Tag 了
        if (other.GetComponent<Tower>() != null)
            return;

        // 也要忽略自己人 (其他大砲子彈)
        if (other.GetComponent<Projectile_Cannon>() != null)
            return;

        DamageEnemiesAround();

        objectPool.Get(explosionFx, transform.position + new Vector3(0, 0.5f, 0));
        objectPool.Remove(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
