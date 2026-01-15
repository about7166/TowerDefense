using UnityEngine;

public class Projectile_MachineGun : MonoBehaviour
{
    private Vector3 target;
    private IDamagable damagable;
    private float damage;
    private float speed;
    private bool isActive = true;

    [SerializeField] private GameObject onHitFx;

    public void SetupProjectile(Vector3 targetPosition, IDamagable newDamagable, float newDamage, float newSpeed)
    {
        target = targetPosition;
        damagable = newDamagable;

        damage = newDamage;
        speed = newSpeed;
    }

    private void Update()
    {
        if (isActive == false)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) <= 0.01f)
        {
            isActive = false;
            damagable.TakeDamage(Mathf.RoundToInt(damage));

            onHitFx.SetActive(true);
            Destroy(gameObject, 1);
        }
    }
}
