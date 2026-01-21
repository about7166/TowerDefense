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
    [SerializeField] private float initialDamage = 5; //撞擊時的瞬間傷害
    [SerializeField] private float damageOverTime = 10; //持續傷害的「總量」
    [SerializeField] private float overTimeEffectDuration = 4; //持續傷害要跑幾秒
    [Range(0f, 1f)]
    [SerializeField] private float slowEffect = 0.7f;

    private bool reachedTarget;
    private bool busyWithAttack;
    private Coroutine damageOverTimeCo;

    protected override void Awake()
    {
        base.Awake();
        CreateNewProjectile();
        harpoonVisuals = GetComponent<Harpoon_Visuals>();
    }

    protected override void Attack()
    {
        base.Attack();

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hitInfo, Mathf.Infinity, whatIsEnemy))
        {
            currentEnemy = hitInfo.collider.GetComponent<Enemy>();
            busyWithAttack = true;
            currentProjectile.SetupProjectile(currentEnemy, projectileSpeed, this);
            harpoonVisuals.EnableChainVisuals(true, currentProjectile.GetConnectionPoint());

            Invoke(nameof(ResetAttackIfMissed), 1);
        }
    }

    public void ActivateAttack()
    {
        reachedTarget = true;
        currentEnemy.GetComponent<Enemy_Flying>().AddObservingTower(this);
        currentEnemy.SlowEnemy(slowEffect, overTimeEffectDuration);
        harpoonVisuals.CreateElectricVFX(currentEnemy.transform);
        
        IDamagable damagable = currentEnemy.GetComponent<IDamagable>(); //獲取傷害介面並造成初始傷害
        damagable?.TakeDamage(initialDamage);
        
        damageOverTimeCo = StartCoroutine(DamageOverTimeCo(damagable)); //開始慢慢扣血
    }

    private IEnumerator DamageOverTimeCo(IDamagable damagable)
    {
        float time = 0;
        // 這裡在計算「多久扣一次血」以及「一次扣多少」
        float damageFrequency = overTimeEffectDuration / damageOverTime;
        float damagePerTick = damageOverTime / (overTimeEffectDuration / damageFrequency);

        while (time < overTimeEffectDuration)
        {
            damagable?.TakeDamage(damagePerTick); //扣血
            yield return new WaitForSeconds(damageFrequency); //等待頻率時間
            time += damageFrequency; //累加時間
        }

        ResetAttack(); //持續傷害結束，重置塔的狀態
    }

    public void ResetAttack()
    {
        if (damageOverTimeCo != null)
            StopCoroutine(damageOverTimeCo);

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
            Instantiate(projectilePrefab, projectileDefaultPosition.position, projectileDefaultPosition.rotation, towerHead);
        
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
