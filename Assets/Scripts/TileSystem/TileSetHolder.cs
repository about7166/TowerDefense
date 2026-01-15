using UnityEngine;

public class TileSetHolder : MonoBehaviour
{
    public GameObject levelSelectTile;

    [Header("基礎地塊")]
    public GameObject tileRoad;
    public GameObject tileField;
    public GameObject tileSideway;

    [Header("拐角地塊")]
    public GameObject tileInnerCorner;
    public GameObject tileInnerCornerSmall;
    public GameObject tileOuterCorner;
    public GameObject tileOuterCornerSmall;

    [Header("斜坡地塊")]
    public GameObject tileHill_1;
    public GameObject tileHill_2;
    public GameObject tileHill_3;

    [Header("橋地塊")]
    public GameObject tileBridgeField;
    public GameObject tileBridgeRoad;
    public GameObject tileBridgeSideway;
}
