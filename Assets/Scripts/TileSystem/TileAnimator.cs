using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private bool isGridMoving; //避免BuildSlot裡的游標動畫影響

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
        List<GameObject> objectsToMove = GetObjectsToMove(gridToMove, showGrid);

        if (gridToMove.IsOnFirstLoad())
            ApplyOffset(objectsToMove, new Vector3(0, -yOffset, 0));

        float offset = showGrid ? yOffset : -yOffset;

        currentActiveCo = StartCoroutine(MoveGridCo(objectsToMove, offset));
    }

    private IEnumerator MoveGridCo(List<GameObject> objectsToMove, float yOffset)
    {
        isGridMoving = true;

        for (int i = 0; i < objectsToMove.Count; i++)
        {
            yield return new WaitForSeconds(tileDelay);

            if (objectsToMove[i] == null)
                continue;

            Transform tile = objectsToMove[i].transform;

            Vector3 targetPosition = tile.position + new Vector3(0, yOffset, 0);

            MoveTile(tile, targetPosition, tileMoveDuration);
        }

        isGridMoving = false;
    }

    public void MoveTile(Transform objectToMove, Vector3 targetPosition, float? newDuration = null)
    {
        float duration = newDuration ?? defaultMoveDuration;
        StartCoroutine(MoveTileCo(objectToMove, targetPosition, duration));
    }

    public IEnumerator MoveTileCo(Transform objectToMove, Vector3 targetPosition, float? newDuration = null)
    {
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

    private void ApplyOffset(List<GameObject> objectsToMove, Vector3 offset)
    {
        foreach (var obj in objectsToMove)
        {
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
        mainMenuObjects.AddRange(GetExtraObjects());
    }

    private List<GameObject> GetObjectsToMove(GridBuilder gridToMove, bool startWithTiles)
    {
        List<GameObject> objectsToMove = new List<GameObject>();
        List<GameObject> extraObjexts = GetExtraObjects();

        if (startWithTiles)
        {
            objectsToMove.AddRange(gridToMove.GetTileSetup());
            objectsToMove.AddRange(extraObjexts);
        }
        else
        {
            objectsToMove.AddRange(extraObjexts);
            objectsToMove.AddRange(gridToMove.GetTileSetup());
        }

        return objectsToMove;
    }

    private List<GameObject> GetExtraObjects()
    {
        List<GameObject> extraObjects = new List<GameObject>();

        extraObjects.AddRange(FindObjectsOfType<EnemyPortal>().Select(component => component.gameObject));
        extraObjects.AddRange(FindObjectsOfType<Castle>().Select(component => component.gameObject));

        return extraObjects;
    }

    public Coroutine GetCurrentActiveCo() => currentActiveCo;
    public float GetBuildOffset() => buildSlotYOffset;
    public float GetTravelDuration() => defaultMoveDuration;
    public bool IsGridMoving() => isGridMoving;
}
