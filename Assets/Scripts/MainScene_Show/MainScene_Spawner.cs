using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation; // ★ 新增：需要引入這個來控制 NavMeshSurface

public class MainScene_Spawner : MonoBehaviour
{
    [Header("展示設定")]
    public GameObject[] showcaseEnemyPrefabs;
    public Transform[] loopPath;
    public int maxEnemies = 10;
    public float spawnInterval = 1.5f;

    [Header("清理設定")]
    public GameObject showcaseTowersParent;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        if (showcaseEnemyPrefabs.Length == 0 || loopPath.Length == 0)
        {
            Debug.LogWarning("生怪器警告：未放入怪物 Prefab 或 路線點！");
            return;
        }

        //  遊戲開始時，啟動「監視」協程
        StartCoroutine(WaitAnimationAndSpawn());
    }

    // ============  新增的監視邏輯  ============
    private IEnumerator WaitAnimationAndSpawn()
    {
        TileAnimator tileAnimator = FindFirstObjectByType<TileAnimator>();

        if (tileAnimator != null)
        {
            // 稍微等待 0.5 秒，確保 TileAnimator 的動畫協程已經正式啟動
            yield return new WaitForSeconds(0.5f);

            // 核心魔法：只要地塊還在移動 (IsGridMoving 為 true)，我們就每一幀都在這裡等
            while (tileAnimator.IsGridMoving())
            {
                yield return null;
            }
        }

        // 離開上面的 while 迴圈，代表地塊動畫徹底播完了！可以開始呼叫生怪了！
        BeginSpawning();
    }
    // ==============================================

    public void BeginSpawning()
    {
        // ★ 修改 1：精準取得「主選單專屬」的 GridBuilder，不亂抓關卡的
        GridBuilder grid = GetMySceneGrid();
        if (grid != null)
        {
            // 確保元件開啟並重新烘焙
            NavMeshSurface surface = grid.GetComponent<NavMeshSurface>();
            if (surface != null) surface.enabled = true;

            grid.UpdateNavMesh();
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            int randomIndex = Random.Range(0, showcaseEnemyPrefabs.Length);
            GameObject prefabToSpawn = showcaseEnemyPrefabs[randomIndex];

            GameObject newEnemy = Instantiate(prefabToSpawn, loopPath[0].position, Quaternion.identity, transform);

            spawnedEnemies.Add(newEnemy);

            MainScene_Enemy enemyScript = newEnemy.GetComponent<MainScene_Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetupMainScene(loopPath);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void ClearShowcase()
    {
        StopAllCoroutines();

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        spawnedEnemies.Clear();

        if (showcaseTowersParent != null)
        {
            showcaseTowersParent.SetActive(false);
        }

        DisableShowcaseNavMesh();

        // 把這段打掃邏輯也複製貼上到這裡！
        Projectile_Cannon[] leftoverProjectiles = FindObjectsByType<Projectile_Cannon>(FindObjectsSortMode.None);
        foreach (var proj in leftoverProjectiles)
        {
            if (proj != null && proj.gameObject.scene == this.gameObject.scene)
            {
                proj.ForceRecycleSafe();
            }
        }
    }

    public void StopSpawningAndSink()
    {
        StopAllCoroutines();

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.enabled = false;

                var collider = enemy.GetComponent<Collider>();
                if (collider != null) collider.enabled = false;

                Destroy(enemy, 2f);
            }
        }

        spawnedEnemies.Clear();
        DisableShowcaseNavMesh();

        // 修改：找出場景中殘留的主場景子彈，呼叫安全回收方法，保護物件池不壞掉
        Projectile_Cannon[] leftoverProjectiles = FindObjectsByType<Projectile_Cannon>(FindObjectsSortMode.None);
        foreach (var proj in leftoverProjectiles)
        {
            if (proj != null && proj.gameObject.scene == this.gameObject.scene)
            {
                // 用剛剛寫好的安全方法來回收，而不是暴力的 Destroy！
                proj.ForceRecycleSafe();
            }
        }
    }
    // ================= 新增的場景隔離邏輯 =================

    // 嚴格比對：只回傳跟這個生怪器在「同一個 Scene (MainScene)」的 GridBuilder
    private GridBuilder GetMySceneGrid()
    {
        GridBuilder[] allGrids = FindObjectsByType<GridBuilder>(FindObjectsSortMode.None);
        foreach (var grid in allGrids)
        {
            if (grid.gameObject.scene == this.gameObject.scene)
            {
                return grid;
            }
        }
        return null;
    }

    // 關閉主選單的 NavMeshSurface，Unity 就會自動將它的導航網格從遊戲世界中抹除！
    private void DisableShowcaseNavMesh()
    {
        GridBuilder myGrid = GetMySceneGrid();
        if (myGrid != null)
        {
            NavMeshSurface surface = myGrid.GetComponent<NavMeshSurface>();
            if (surface != null)
            {
                surface.enabled = false;
            }
        }
    }
}