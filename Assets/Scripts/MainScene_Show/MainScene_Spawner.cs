using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        GridBuilder grid = FindFirstObjectByType<GridBuilder>();
        if (grid != null) grid.UpdateNavMesh();

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
    }
}