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

    [Header("爆炸音效設定")]
    [SerializeField] private AudioClip explosionSound;
    [Range(0f, 1f)]
    [SerializeField] private float explosionVolume = 0.8f; // 預設音量 0.8，可在 Unity 自由拉動
    
    private Collider[] hitColliders = new Collider[20];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    public void SetupProjectile(Vector3 newVelocity, float newDamage, ObjectPoolManager newPool)
    {
        // 防呆：確定有這個元件才清空軌跡
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
        // 效能優化：改用 NonAlloc 版本，不產生額外的記憶體垃圾
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
        // 關鍵防護：忽略所有「隱形的偵測圈 (Trigger)」
        if (other.isTrigger)
            return;

        if (other.GetComponent<Tower>() != null)
            return;

        if (other.GetComponent<Projectile_Cannon>() != null)
            return;

        DamageEnemiesAround();

        // 產生爆炸特效
        objectPool.Get(explosionFx, transform.position + new Vector3(0, 0.5f, 0));

        if (explosionSound != null)
        {
            // 在砲彈爆炸的位置，以設定的音量播放音效
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }

        // 回收砲彈
        objectPool.Remove(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}