using UnityEngine;

public class Projectile_Cannon : MonoBehaviour
{
    private Rigidbody rb;
    private float damage;

    [SerializeField] private float damageRadius;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private GameObject explosionFx;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetupProjectile(Vector3 newVelocity, float newDamage)
    {
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
                int newDamage = Mathf.RoundToInt(damage); //老師說以後再改
                damagable.TakeDamage(newDamage);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DamageEnemiesAround();
        explosionFx.SetActive(true);
        explosionFx.transform.parent = null; //讓特效不要跟著子彈一起被刪
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
