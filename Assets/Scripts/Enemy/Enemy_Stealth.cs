using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Stealth : Enemy
{
    [Header("隱形敵人設定")]
    [SerializeField] private List<Enemy> enemiesToHide;
    [SerializeField] private float hideDuration = 0.5f;
    [SerializeField] private ParticleSystem smokeFX;
    private bool canHideEnemies = true;

    protected override void Awake()
    {
        base.Awake();

        InvokeRepeating(nameof(HideItself), 0.1f, hideDuration);
        InvokeRepeating(nameof(HideEnemies), 0.1f, hideDuration); //(方法名稱, 第一次執行的延遲, 之後重複的間隔)
    }

    private void HideItself() => HideEnemy(hideDuration);

    private void HideEnemies()
    {
        if (canHideEnemies == false)
            return;

        foreach (Enemy enemy in enemiesToHide)
        {
            enemy.HideEnemy(hideDuration);
        }
    }

    public List<Enemy> GetEnemiesToHide() => enemiesToHide;
    public void EnableSmoke(bool enable)
    {

        if (smokeFX.isPlaying == false && enable)
            smokeFX.Play();
        else if (smokeFX.isPlaying == true && enable == false)
            smokeFX.Stop();
    }

    protected override IEnumerator DisableHideCo(float duration)
    {
        EnableSmoke(false);
        canBeHidden = false;
        canHideEnemies = false;

        yield return new WaitForSeconds(duration);

        EnableSmoke(true);
        canBeHidden = true;
        canHideEnemies = true;
    }
}
