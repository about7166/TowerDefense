using UnityEngine;

public class Castle : MonoBehaviour
{
    // 這裡我們甚至不需要 gameManager 了，因為扣血的動作已經交給 Enemy 去做了！

    private void OnTriggerEnter(Collider other)
    {
        // 建議用 CompareTag，效能比 == "Enemy" 更好
        if (other.CompareTag("Enemy"))
        {
            // 抓取撞到主堡的那個敵人
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                // 呼叫我們在 Enemy.cs 寫好的新方法：
                // 讓敵人依照自己的設定去扣主堡的血，然後移除自己
                enemy.ReachCastleAndDealDamage();
            }
        }
    }
}