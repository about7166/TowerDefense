using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public Enemy currentEnemy;

    [SerializeField] protected float attackCooldown = 1;
    protected float lastTimeAttacked;

    [Header("塔的設定")]
    [SerializeField] protected EnemyType enemyPriorityType = EnemyType.None;
    [SerializeField] protected Transform towerHead;
    [SerializeField] protected float rotationSpeed = 10;
    private bool canRotate;

    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected LayerMask whatIsEnemy;

    [Space]
    [Tooltip("啟用此功能後，防禦塔可以在攻擊間隙切換目標")]
    [SerializeField] private bool dynamicTargetChange;
    private float targetCheckInterval = 0.1f;
    private float lastTimeCheckedTarget;

    protected virtual void Awake()
    {
        EnableRotation(true);
    }

    protected virtual void Update()
    {
        UpdateTargetIfNeeded();

        if (currentEnemy == null)
        {
            currentEnemy = FindEnemyWithRange();
            return;
        }

        if (CanAttack())
            Attack();

        LooseTargetIfNeeded();
        RotateTowarsEnemy();
    }

    public float GetAttackRange() => attackRange; 
    private void LooseTargetIfNeeded()
    {
        if (Vector3.Distance(currentEnemy.CenterPoint(), transform.position) > attackRange)
            currentEnemy = null;
    }

    private void UpdateTargetIfNeeded()
    {
        if (dynamicTargetChange == false)
            return;

        if (Time.time > lastTimeCheckedTarget + targetCheckInterval)
        {
            lastTimeCheckedTarget = Time.time;
            currentEnemy = FindEnemyWithRange();
        }
    }

    protected virtual void Attack()
    {
        //Debug.Log("Attcak performed at" +  Time.time);
    }

    protected bool CanAttack()
    {
        if (Time.time > lastTimeAttacked + attackCooldown)
        {
            lastTimeAttacked = Time.time;
            return true;
        }

        return false;
    }

    protected Enemy FindEnemyWithRange()
    {
        List<Enemy> priorityTargets = new List<Enemy>();
        List<Enemy> possibleTargets = new List<Enemy>();

        Collider[] enemiesAround = Physics.OverlapSphere(transform.position, attackRange, whatIsEnemy);

        foreach (Collider enemy in enemiesAround)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>();
            EnemyType newEnemyType = newEnemy.GetEnemyType();
         
            if (newEnemyType == enemyPriorityType)
                priorityTargets.Add(newEnemy);
            else
                possibleTargets.Add(newEnemy);
            
        }

        if (priorityTargets.Count > 0)
            return GetMostAdvancedEnemy(priorityTargets);

        if (possibleTargets.Count > 0)
            return GetMostAdvancedEnemy(possibleTargets);

        return null;

    }

    private Enemy GetMostAdvancedEnemy(List<Enemy> targets)
    {
        Enemy mostAdvanceEnemy = null;
        float minRemainingDistance = float.MaxValue;

        foreach (Enemy enemy in targets)
        {
            float remainingDistance = enemy.DistanceToFinishLine();

            if (remainingDistance < minRemainingDistance)
            {
                minRemainingDistance = remainingDistance;
                mostAdvanceEnemy = enemy;
            }
        }

        return mostAdvanceEnemy;
    }

    public void EnableRotation(bool enable)
    {
        canRotate = enable;
    }

    protected virtual void RotateTowarsEnemy()
    {
        if (canRotate == false)
            return;

        if (currentEnemy == null) 
            return;

        Vector3 directionToEnemy = DirectionToEnemyFrom(towerHead);

        Quaternion lookRotation =  Quaternion.LookRotation(directionToEnemy);

        Vector3 rotation = Quaternion.Lerp(towerHead.rotation, lookRotation, rotationSpeed * Time.deltaTime).eulerAngles;

        towerHead.rotation = Quaternion.Euler(rotation);
    }

    protected Vector3 DirectionToEnemyFrom(Transform startPoint)
    {
        return (currentEnemy.CenterPoint() - startPoint.position).normalized;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
