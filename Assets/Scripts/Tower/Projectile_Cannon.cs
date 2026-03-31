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

    // ★ 效能優化：預先準備一個陣列來裝炸到的敵人 (跟 Hammer 一樣)
    private Collider[] hitColliders = new Collider[20];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    public void SetupProjectile(Vector3 newVelocity, float newDamage, ObjectPoolManager newPool)
    {
        // ★ 防呆：確定有這個元件才清空軌跡
        if (trail != null)
        {
            trail.Clear();
        }

        objectPool = newPool;
        rb.linearVelocity = newVelocity;
        damage = newDamage;
    }

    private void DamageEnemiesAround()
    {
        // ★ 效能優化：改用 NonAlloc 版本，不產生額外的記憶體垃圾
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, damageRadius, hitColliders, whatIsEnemy);

        for (int i = 0; i < hitCount; i++)
        {
            IDamagable damagable = hitColliders[i].GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.TakeDamage(damage);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ★ 關鍵防護：忽略所有「隱形的偵測圈 (Trigger)」
        if (other.isTrigger)
            return;

        if (other.GetComponent<Tower>() != null)
            return;

        if (other.GetComponent<Projectile_Cannon>() != null)
            return;

        DamageEnemiesAround();

        // 產生爆炸特效並回收砲彈
        objectPool.Get(explosionFx, transform.position + new Vector3(0, 0.5f, 0));
        objectPool.Remove(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}