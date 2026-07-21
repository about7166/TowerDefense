using System.Collections;
using UnityEngine;

public class Tower_Harpoon : Tower
{
    private Harpoon_Visuals harpoonVisuals;

    [Header("魚叉設定")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileDefaultPosition;
    [SerializeField] private float projectileSpeed = 15;
    private Projectile_Harpoon currentProjectile;

    [Header("傷害設定")]
    [SerializeField] private float initialDamage = 5;
    [SerializeField] private float damageOverTime = 10;
    [SerializeField] private float overTimeEffectDuration = 4;
    [Range(0f, 1f)]
    [SerializeField] private float slowEffect = 0.7f;
   
    // ============  新增：專門用來放導電音效的播放器  ============
    [Header("持續導電音效")]
    public AudioSource electricLoopSound;
    // ============  ============

    private bool reachedTarget;
    private bool busyWithAttack;
    private Coroutine damageOverTimeCo;

    public override float GetSlowPercentage() => (1f - slowEffect) * 100f;

    protected override void Awake()
    {
        base.Awake();
        currentProjectile = GetComponentInChildren<Projectile_Harpoon>(true);
        harpoonVisuals = GetComponent<Harpoon_Visuals>();
    }

    protected override void Attack()
    {
        base.Attack();

        if (currentEnemy != null)
        {
            if (currentProjectile == null)
            {
                CreateNewProjectile();
            }

            busyWithAttack = true;
            currentProjectile.SetupProjectile(currentEnemy, projectileSpeed, this);
            harpoonVisuals.EnableChainVisuals(true, currentProjectile.GetConnectionPoint());

            if (attackSfx != null)
            {
                AudioManager.instance?.PlaySFX(attackSfx, true);
            }

            Invoke(nameof(ResetAttackIfMissed), 1);
        }
    }

    public void ActivateAttack()
    {
        if (currentEnemy == null || currentEnemy.gameObject.activeInHierarchy == false)
            return;

        reachedTarget = true;
        currentEnemy.GetComponent<Enemy_Flying>().AddObservingTower(this);
        currentEnemy.SlowEnemy(slowEffect, overTimeEffectDuration);
        harpoonVisuals.CreateElectricVFX(currentEnemy.transform);

        IDamagable damagable = currentEnemy.GetComponent<IDamagable>();
        damagable?.TakeDamage(initialDamage);

        if (electricLoopSound != null)
        {
            electricLoopSound.Play();
        }

        damageOverTimeCo = StartCoroutine(DamageOverTimeCo(damagable));
    }

    private IEnumerator DamageOverTimeCo(IDamagable damagable)
    {
        float time = 0;
        float damageFrequency = overTimeEffectDuration / damageOverTime;
        float damagePerTick = damageOverTime / (overTimeEffectDuration / damageFrequency);

        while (time < overTimeEffectDuration)
        {
            if (currentEnemy == null || !currentEnemy.gameObject.activeInHierarchy)
            {
                break; // 跳出迴圈，直接執行底下的 ResetAttack()
            }

            damagable?.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(damageFrequency);
            time += damageFrequency;
        }

        ResetAttack();
    }

    public void ResetAttack()
    {
        if (damageOverTimeCo != null)
            StopCoroutine(damageOverTimeCo);

        if (electricLoopSound != null)
        {
            electricLoopSound.Stop();
        }

        busyWithAttack = false;
        reachedTarget = false;

        currentEnemy = null;
        lastTimeAttacked = Time.time;
        harpoonVisuals.EnableChainVisuals(false);
        CreateNewProjectile();
    }

    private void CreateNewProjectile()
    {
        GameObject newProjectile =
            objectPool.Get(projectilePrefab, projectileDefaultPosition.position, projectileDefaultPosition.rotation, towerHead);

        currentProjectile = newProjectile.GetComponent<Projectile_Harpoon>();
    }

    private void ResetAttackIfMissed()
    {
        if (reachedTarget)
            return;

        Destroy(currentProjectile.gameObject);
        ResetAttack();
    }

    protected override bool CanAttack()
    {
        return base.CanAttack() & busyWithAttack == false;
    }

    protected override void LooseTargetIfNeeded()
    {
        if (busyWithAttack == false)
            base.LooseTargetIfNeeded();
    }
}