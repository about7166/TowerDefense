using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance;

    [Header("物件池設定")]
    [SerializeField] private GameObject[] predefinedPools; //predefined定義
    [SerializeField] private int defaultPoolSize = 50;
    [SerializeField] private int maxPoolSize = 500;

    private Dictionary<GameObject, ObjectPool<GameObject>> poolDictionary;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializePools();
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
    {
        if (poolDictionary.ContainsKey(prefab) == false)
        {
            Debug.LogWarning("找不到遊戲物件池" + prefab.name + ",生成新池");
            CreateNewPool(prefab);
        }

        GameObject objectToGet = poolDictionary[prefab].Get();
        objectToGet.transform.position = position;
        objectToGet.transform.rotation = rotation ?? Quaternion.identity;
        objectToGet.transform.parent = parent;

        return objectToGet;
    }

    public void Remove(GameObject objectToRemove)
    {
        GameObject originalPrefab = objectToRemove.GetComponent<PooledObject>()?.originalPrefab;

        if (originalPrefab == null)
        {
            Debug.LogWarning("此遊戲物件沒有物件池。遊戲物件將被銷毀。");
            Destroy(objectToRemove);
            return;
        }

        poolDictionary[originalPrefab].Release(objectToRemove);
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<GameObject, ObjectPool<GameObject>>();

        foreach (GameObject prefab in predefinedPools)
            CreateNewPool(prefab);
    }

    // 這是一個「建立新池子」的方法
    private void CreateNewPool(GameObject prefab)
    {
        // 這裡在創建一個「物件池實例」
        // var 是讓電腦自己判斷型別 (這裡型別是 ObjectPool<GameObject>)
        var pool = new ObjectPool<GameObject>
            (
                // 1. createFunc: 當池子空了，有人要借東西時，要怎麼「生出新的」？
                // 這裡使用了 Lambda 寫法：呼叫 NewPoolObject 方法去 Instantiate
                createFunc: () => NewPoolObject(prefab),

                // 2. actionOnGet: 當東西從池子「被借出去」時，要做什麼？
                // 通常是把它 SetActive(true) 打開
                actionOnGet: obj => obj.SetActive(true),

                // 3. actionOnRelease: 當東西「被還回來」時，要做什麼？
                // 通常是把它 SetActive(false) 關掉，讓它在背景待命
                actionOnRelease: obj =>
                {
                    obj.SetActive(false);
                    obj.transform.parent = transform;
                },
                // 4. actionOnDestroy: 當池子滿了(超過 maxSize)，還有人要還東西時，該怎麼辦？
                // 為了不佔用無限記憶體，多出來的直接真·銷毀 (Destroy)
                actionOnDestroy: obj => Destroy(obj),

                // 5. collectionCheck: 是否檢查「重複歸還」？
                // 例如同一個子彈被連續還兩次。設為 true 會報錯但安全，設為 false 效能比較好。
                collectionCheck: false,

                // 6. defaultCapacity: 初始容量 (建議設為預期會用到的數量)
                defaultCapacity: defaultPoolSize,

                // 7. maxSize: 最大容量 (防止池子無限膨脹)
                maxSize: maxPoolSize
            );

        poolDictionary.Add(prefab, pool);
        StartCoroutine(PreloadPoolCo(pool, defaultPoolSize));
    }

    private IEnumerator PreloadPoolCo(ObjectPool<GameObject> poolToPreload, int count)
    {
        List<GameObject> preloadedObjects = new List<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = poolToPreload.Get();
            preloadedObjects.Add(obj);
            obj.SetActive(false);
            yield return null;
        }

        foreach (GameObject obj in preloadedObjects)
            poolToPreload.Release(obj);
    }

    private GameObject NewPoolObject(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.AddComponent<PooledObject>().originalPrefab = prefab;

        return newObject;
    }
}
