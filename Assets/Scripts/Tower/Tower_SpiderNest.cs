using System.Collections;
using UnityEngine;

public class Tower_SpiderNest : Tower
{
    [Header("©Çª«±_¥Þ³]©w")]
    [SerializeField] private float damage;

    [SerializeField] private GameObject spiderPrefab;
    [Range(0f, 1f)]
    [SerializeField] private float attackTimeMultiplier = 0.4f;
    [SerializeField] private float reloadTimeMultiplier = 0.6f;
    [Space]
    [SerializeField] private Transform[] webSet;
    [SerializeField] private Transform[] attachPoint;
    [SerializeField] private Transform[] attachPointRef;

    private GameObject[] activeSpider;
    private int spiderInsex;
    private Vector3 spiderPointOffset = new Vector3(0, -0.175f, 0);

    protected override void Awake()
    {
        base.Awake();
        InitializeSpiders();

        reloadTimeMultiplier = 1 - attackTimeMultiplier;
    }

    protected override void Update()
    {
        base.Update();
        UpdateAttachPointsPosition();
    }

    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown && AtLeastOneEnemyAround();
    }
    
    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }

    private IEnumerator AttackCo()
    {
        Transform currentWeb = webSet[spiderInsex];
        Transform currentAttachPoint = attachPoint[spiderInsex];
        float attackTime = (attackCooldown / 4) * attackTimeMultiplier;
        float reloadTime = (attackCooldown / 4) * reloadTimeMultiplier;

        yield return ChangeScaleCo(currentWeb, 1, attackTime);

        activeSpider[spiderInsex].GetComponent<Projectile_SpiderNest>().SetupSpider(damage);

        yield return ChangeScaleCo(currentWeb, 0.1f, reloadTime);
        activeSpider[spiderInsex] = Instantiate(spiderPrefab, currentAttachPoint.position + spiderPointOffset, Quaternion.identity, currentAttachPoint);

        spiderInsex = (spiderInsex + 1) % attachPoint.Length;
    }

    private void UpdateAttachPointsPosition()
    {
        for (int i = 0; i < attachPoint.Length; i++)
        {
            attachPoint[i].position = attachPointRef[i].position;
        }
    }

    private void InitializeSpiders()
    {
        activeSpider = new GameObject[attachPoint.Length];
        
        for (int i = 0; i < activeSpider.Length; i++)
        {
            GameObject newSpider = Instantiate(spiderPrefab, attachPoint[i].position + spiderPointOffset, Quaternion.identity, attachPoint[i]);
            activeSpider[i] = newSpider;
        }
    }

    private IEnumerator ChangeScaleCo(Transform transform, float newScale, float duration = 0.25f)
    {
        float time = 0;

        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(initialScale.x, newScale, initialScale.z); // (1, newScale, 1);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
