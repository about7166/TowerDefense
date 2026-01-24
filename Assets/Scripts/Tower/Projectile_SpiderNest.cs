using UnityEngine;
using UnityEngine.AI;

public class Projectile_SpiderNest : MonoBehaviour
{
    private TrailRenderer trail;
    private ObjectPoolManager objectPool;
    private NavMeshAgent agent;
    private Transform currentTarget;

    [SerializeField] private float damage;
    [SerializeField] private float damageRadius = 0.8f;
    [SerializeField] private float detonateDistance = 0.5f;
    [SerializeField] private GameObject explosionFx;
    [Space]
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private float enemyCheckRadius = 10;
    [SerializeField] private float targetUpdateInterval = 0.5f;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        agent = GetComponent<NavMeshAgent>();
        objectPool = ObjectPoolManager.instance;

        InvokeRepeating(nameof(UpdateClosestTarget), 0.1f, targetUpdateInterval);//0.1f後開始第一次，接下來每targetUpdateInterval一次
    }

    private void Update()
    {
        if (currentTarget == null || agent.enabled == false || agent.isOnNavMesh == false)//沒目標、NAV關閉、不在路上
            return;

        agent.SetDestination(currentTarget.position);

        if (Vector3.Distance(transform.position, currentTarget.position) < detonateDistance)
            Explode();
    }

    private void Explode()
    {
        DamageEnemiesAround();

        objectPool.Get(explosionFx, transform.position + new Vector3(0, 0.4f, 0));
        objectPool.Remove(gameObject);
    }

    public void SetupSpider(float newDamage)
    {
        trail.Clear();
        damage = newDamage;
        agent.enabled = true;
        transform.parent = null;
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

    private void UpdateClosestTarget()
    {
        currentTarget = FindClosestEnemy();
    }

    private Transform FindClosestEnemy()
    {
        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, enemyCheckRadius, whatIsEnemy);
        Transform nearestEnemy = null; float shortestDistance = float.MaxValue;

        foreach (Collider enemyCollider in enemiesAround)
        {
            float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);

            if (distance < shortestDistance)
            {
                nearestEnemy = enemyCollider.transform;
                shortestDistance = distance;
            }
        }

        return nearestEnemy;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
