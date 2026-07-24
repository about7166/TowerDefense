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

        // 終極防呆 1：確保「爆炸特效」欄位有放東西，且「物件池」存在才執行
        if (explosionFx != null && objectPool != null)
        {
            objectPool.Get(explosionFx, transform.position + new Vector3(0, 0.5f, 0));
        }

        // 終極防呆 2：確保「音效」欄位有放東西才播放，沒有放就安靜略過，絕不報錯
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);
        }

        // 終極防呆 3：確保物件池存在才回收，如果真的找不到物件池，就直接銷毀避免殘留
        if (objectPool != null)
        {
            objectPool.Remove(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}