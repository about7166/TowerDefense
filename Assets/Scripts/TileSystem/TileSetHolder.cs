using UnityEngine;

public class TileSetHolder : MonoBehaviour
{
    public GameObject levelSelectTile;

    [Header("基礎地塊")]
    public GameObject tileRoad;
    public GameObject tileCanBuild;
    public GameObject tileNoBuild;
    public GameObject tileSideway;

    [Header("拐角地塊")]
    public GameObject tileInnerCorner;
    public GameObject tileInnerCornerSmall;
    public GameObject tileOuterCorner;
    public GameObject tileOuterCornerSmall;

    [Header("斜坡地塊")]
    public GameObject tileHill_1;
    public GameObject tileHill_1_L; // 新增
    public GameObject tileHill_1_R; // 新增
    public GameObject tileHill_2;
    public GameObject tileHill_2_L; // 新增
    public GameObject tileHill_2_R; // 新增
    public GameObject tileHill_3;
    public GameObject tileHill_3_L; // 新增
    public GameObject tileHill_3_R; // 新增

    [Header("橋地塊")]
    public GameObject tileBridgeNoBuild_Middle; // 原本的 tileBridgeField
    public GameObject tileBridgeRoad_Middle;    // 原本的 tileBridgeRoad
    public GameObject tileBridgeSideway_Middle; // 原本的 tileBridgeSideway
    public GameObject tileBridgeNoBuild_Side;   // 新增
    public GameObject tileBridgeRoad_Side;      // 新增
    public GameObject tileBridgeSideway_Side;   // 新增
    public GameObject tileBridgeInnerCorner;    // 新增
    public GameObject tileBridgeOuterCornerSmall; // 新增
    public GameObject tileBridgeNoBuild_InnerCorner; // 新增：不能蓋塔的內角
    public GameObject tileBridgeNoBuild_OuterCornerSmall; // 新增：不能蓋塔的小外角
}