using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    private NavMeshSurface myNacMesh => GetComponent<NavMeshSurface>();
    [SerializeField] private GameObject mainPrefab;

    [SerializeField] private int gridLength = 10;
    [SerializeField] private int griWidth = 10;

    [SerializeField] private List<GameObject> createdTiles;
    public void UpdateNavMesh() => myNacMesh.BuildNavMesh();
    public List<GameObject> GetTileSetup() => createdTiles;


    private bool hadFristLoad;

    public bool IsOnFirstLoad() //給TileAnimator用的
    {
        if (hadFristLoad == false)
        {
            hadFristLoad = true;
            return true;
        }

        return false;
    }

    [ContextMenu("生成地板網格")]
    private void BuildGrid()
    {
        ClearGrid();
        createdTiles = new List<GameObject>();

        for (int x = 0; x < gridLength; x++)
        {
            for (int z = 0; z < griWidth; z++)
            {
                CreateTile(x,z);
            }
        }
    }

    [ContextMenu("清除地板網格")]

    private void ClearGrid()
    {
        foreach (GameObject tile in createdTiles)
        {
            DestroyImmediate(tile);
        }

        createdTiles.Clear();
    }

    private void CreateTile(float xPosition, float zPosition)
    {
        Vector3 newPosition = new Vector3 (xPosition, 0, zPosition);
        GameObject newTile = Instantiate(mainPrefab, newPosition, Quaternion.identity, transform);

        createdTiles.Add(newTile);
    }
}
