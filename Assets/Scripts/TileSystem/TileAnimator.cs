using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;

public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float defaultMoveDuration = 0.1f;

    [Header("可建地塊移動")]
    [SerializeField] private float buildSlotYOffset = 0.25f;

    [Header("地塊動畫設定")]
    [SerializeField] private float tileMoveDuration = 0.1f;
    [SerializeField] private float tileDelay = 0.1f;
    [SerializeField] private float yOffset = 5;

    [Space]
    [SerializeField] private List<GameObject> mainMenuObjects = new List<GameObject>();
    [SerializeField] private GridBuilder mainSceneGrid;
    private Coroutine currentActiveCo;
    private bool isGridMoving;

    [Header("地塊溶解效果設定")]
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private float dissolveDuration = 1.2f;
    [SerializeField] private List<Transform> dissolvingObjects = new List<Transform>();

    // 終極防護：防止關卡內自帶的測試用 TileAnimator 在打包後詐屍搗亂
    private void Awake()
    {
        TileAnimator[] animators = FindObjectsByType<TileAnimator>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // 如果場景裡有超過一個 TileAnimator，且這個腳本不在主選單，代表它是測試用的分身
        if (animators.Length > 1 && gameObject.scene.name != "MainScene")
        {
            gameObject.SetActive(false); // 立刻關閉，防止 Start 被執行
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (GameManager.instance.IsTestingLevel())
            return;

        CollectMainSceneObjects();
        ShowGrid(mainSceneGrid, true);
    }

    public void ShowMainGrid(bool showMainGrid)
    {
        ShowGrid(mainSceneGrid, showMainGrid);
    }

    public void ShowGrid(GridBuilder gridToMove, bool showGrid)
    {
        if (gridToMove == null) return;

        List<GameObject> objectsToMove = GetObjectsToMove(gridToMove, showGrid);

        if (gridToMove.IsOnFirstLoad())
            ApplyOffset(objectsToMove, new Vector3(0, -yOffset, 0));

        float offset = showGrid ? yOffset : -yOffset;

        gridToMove.MakeTilesNonInteractable(true);
        currentActiveCo = StartCoroutine(MoveGridCo(objectsToMove, offset, showGrid));
    }

    private IEnumerator MoveGridCo(List<GameObject> objectsToMove, float yOffset, bool showGrid)
    {
        isGridMoving = true;

        for (int i = 0; i < objectsToMove.Count; i++)
        {
            yield return new WaitForSeconds(tileDelay);

            if (objectsToMove[i] == null)
                continue;

            Transform tile = objectsToMove[i].transform;
            Vector3 targetPosition = tile.position + new Vector3(0, yOffset, 0);

            DissolveTile(showGrid, tile);
            MoveTile(tile, targetPosition, showGrid, tileMoveDuration);
        }

        while (dissolvingObjects.Count > 0)
        {
            dissolvingObjects.RemoveAll(item => item == null);
            yield return null;
        }

        yield return new WaitForSeconds(tileMoveDuration + 0.2f);

        foreach (var tile in objectsToMove)
        {
            if (tile != null)
            {
                TileSlot slot = tile.GetComponent<TileSlot>();
                if (slot != null)
                {
                    slot.MakeNonInteractable(false);
                }
            }
        }

        isGridMoving = false;
    }

    public void MoveTile(Transform objectToMove, Vector3 targetPosition, bool showGrid, float? newDuration = null)
    {
        float moveDelay = showGrid ? 0 : 0.8f;
        float duration = newDuration ?? defaultMoveDuration;
        StartCoroutine(MoveTileCo(objectToMove, targetPosition, moveDelay, duration));
    }

    public IEnumerator MoveTileCo(Transform objectToMove, Vector3 targetPosition, float delay = 0, float? newDuration = null)
    {
        yield return new WaitForSeconds(delay);
        float time = 0;
        Vector3 startPosition = objectToMove.position;
        float duration = newDuration ?? defaultMoveDuration;

        while (time < duration)
        {
            if (objectToMove == null)
                break;

            objectToMove.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        if (objectToMove != null)
            objectToMove.position = targetPosition;
    }

    public void DissolveTile(bool showtTile, Transform tile)
    {
        MeshRenderer[] meshRenderers = tile.GetComponentsInChildren<MeshRenderer>();
        if (tile.GetComponent<TileSlot>() != null)
        {
            foreach (MeshRenderer mesh in meshRenderers)
            {
                StartCoroutine(DissolveTileCo(mesh, dissolveDuration, showtTile));
            }
        }
    }

    private IEnumerator DissolveTileCo(MeshRenderer meshRenderer, float duration, bool showTile)
    {
        TextMeshPro textMeshPro = meshRenderer.GetComponent<TextMeshPro>();

        if (textMeshPro != null)
        {
            textMeshPro.enabled = showTile;
            yield break;
        }

        dissolvingObjects.Add(meshRenderer.transform);
        float startValue = showTile ? 1 : 0;
        float targetValue = showTile ? 0 : 1;

        Material originalMaterial = meshRenderer.material;
        meshRenderer.material = new Material(dissolveMaterial);
        Material dissolveMatInstance = meshRenderer.material;

        dissolveMatInstance.SetColor("_BaseColor", originalMaterial.GetColor("_BaseColor"));
        if (originalMaterial.HasProperty("_BaseMap"))
            dissolveMatInstance.SetTexture("_BaseMap", originalMaterial.GetTexture("_BaseMap"));
        else if (originalMaterial.HasProperty("_MainTex"))
            dissolveMatInstance.SetTexture("_BaseMap", originalMaterial.GetTexture("_MainTex"));

        dissolveMatInstance.SetFloat("_Metallic", originalMaterial.GetFloat("_Metallic"));
        dissolveMatInstance.SetFloat("_Smoothness", originalMaterial.GetFloat("_Smoothness"));
        dissolveMatInstance.SetFloat("_Dissolve", startValue);

        float time = 0;

        while (time < duration)
        {
            float currentDissolveValue = Mathf.Lerp(startValue, targetValue, time / duration);
            dissolveMatInstance.SetFloat("_Dissolve", currentDissolveValue);
            time += Time.deltaTime;
            yield return null;
        }

        meshRenderer.material = originalMaterial;
        if (meshRenderer != null)
            dissolvingObjects.Remove(meshRenderer.transform);
    }

    private void ApplyOffset(List<GameObject> objectsToMove, Vector3 offset)
    {
        foreach (var obj in objectsToMove)
        {
            if (obj == null) continue;

            // 用絕對物理座標防護，在地底就不再推
            if (obj.transform.position.y < -2f)
            {
                continue;
            }

            obj.transform.position += offset;
        }
    }

    public void EnableMainMenuGrid(bool enable)
    {
        ShowGrid(mainSceneGrid, enable);
        mainSceneGrid.GetComponent<NavMeshSurface>().enabled = enable;
    }

    public void EnableMainSceneObjects(bool enable)
    {
        foreach (var obj in mainMenuObjects)
        {
            obj.SetActive(enable);
        }
    }

    private void CollectMainSceneObjects()
    {
        mainMenuObjects.AddRange(mainSceneGrid.GetTileSetup());
        mainMenuObjects.AddRange(GetExtraObjects(mainSceneGrid));
    }

    private List<GameObject> GetObjectsToMove(GridBuilder gridToMove, bool startWithTiles)
    {
        List<GameObject> objectsToMove = new List<GameObject>();
        List<GameObject> extraObjects = GetExtraObjects(gridToMove);

        if (startWithTiles)
        {
            objectsToMove.AddRange(gridToMove.GetTileSetup());
            objectsToMove.AddRange(extraObjects);
        }
        else
        {
            objectsToMove.AddRange(extraObjects);
            objectsToMove.AddRange(gridToMove.GetTileSetup());
        }

        return objectsToMove;
    }

    private List<GameObject> GetExtraObjects(GridBuilder gridToMove)
    {
        List<GameObject> extraObjects = new List<GameObject>();
        UnityEngine.SceneManagement.Scene targetScene = gridToMove.gameObject.scene;

        foreach (var portal in FindObjectsByType<EnemyPortal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (portal.gameObject.scene == targetScene) extraObjects.Add(portal.gameObject);

        foreach (var castle in FindObjectsByType<Castle>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (castle.gameObject.scene == targetScene) extraObjects.Add(castle.gameObject);

        foreach (var dec in FindObjectsByType<MapDecoration>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (dec.gameObject.scene == targetScene) extraObjects.Add(dec.gameObject);

        return extraObjects.Distinct().ToList();
    }

    public Coroutine GetCurrentActiveCo() => currentActiveCo;
    public float GetBuildOffset() => buildSlotYOffset;
    public float GetTravelDuration() => defaultMoveDuration;
    public bool IsGridMoving() => isGridMoving;
}