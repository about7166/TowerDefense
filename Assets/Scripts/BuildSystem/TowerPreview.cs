using System.Collections.Generic;
using UnityEngine;

public class TowerPreview : MonoBehaviour
{
    private List<System.Type> compToKeep = new List<System.Type>();

    private MeshRenderer[] meshRenderers;
    private ForwardAttackDisplay forwardDisplay;

    private float attackRange;
    private bool towerAttacksForward;

    // 新增的範圍圈系統
    private LineRenderer rangeBorder;
    private SpriteRenderer rangeFill;
    private GameObject indicatorObj;

    public void SetupTowerPreview(GameObject towerToBuild)
    {
        Tower tower = towerToBuild.GetComponent<Tower>();

        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        forwardDisplay = tower.GetComponent<ForwardAttackDisplay>();
        attackRange = tower.GetAttackRange();
        towerAttacksForward = tower.towerAttacksForward;

        SecureComponents();
        MakeAllMeshTransparent();
        DestroyExtraComponents();

        // 在清理完不必要的組件後，生成我們的新版範圍圈
        CreateLeagueOfLegendsIndicator();

        gameObject.SetActive(false);
    }

    private void CreateLeagueOfLegendsIndicator()
    {
        // 找總管要素材
        BuildManager bm = FindFirstObjectByType<BuildManager>();
        if (bm == null) return;

        indicatorObj = new GameObject("PreviewRangeIndicator");
        indicatorObj.transform.SetParent(this.transform);
        indicatorObj.transform.localPosition = Vector3.zero;

        // 1. 內圈漸層
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(indicatorObj.transform);
        fillObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        rangeFill = fillObj.AddComponent<SpriteRenderer>();
        rangeFill.sprite = bm.rangeGradientSprite;
        rangeFill.color = bm.rangeFillColor;
        rangeFill.sortingOrder = 4;

        // 2. 外圈發光實線
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(indicatorObj.transform);

        rangeBorder = borderObj.AddComponent<LineRenderer>();
        rangeBorder.useWorldSpace = true;
        rangeBorder.loop = true;
        rangeBorder.positionCount = 60;
        rangeBorder.startWidth = bm.rangeBorderThickness;
        rangeBorder.endWidth = bm.rangeBorderThickness;

        if (bm.rangeLineMaterial != null) rangeBorder.material = bm.rangeLineMaterial;
        rangeBorder.startColor = bm.rangeBorderColor;
        rangeBorder.endColor = bm.rangeBorderColor;
        rangeBorder.sortingOrder = 5;

        indicatorObj.SetActive(false);
    }

    public void ShowPreview(bool showPreview, Vector3 previewPosition)
    {
        transform.position = previewPosition;

        if (towerAttacksForward == false)
        {
            if (indicatorObj != null) indicatorObj.SetActive(showPreview);
            if (showPreview) UpdateIndicatorSize(previewPosition);
        }
        else
        {
            if (forwardDisplay != null) forwardDisplay.CreateLines(showPreview, attackRange);
        }
    }

    private void UpdateIndicatorSize(Vector3 centerPosition)
    {
        if (rangeFill == null || rangeBorder == null) return;

        Vector3 center = centerPosition + Vector3.up * 0.15f;

        // 更新內圈縮放
        rangeFill.transform.position = center;
        float spriteSize = rangeFill.sprite != null ? rangeFill.sprite.bounds.size.x : 1f;
        float targetScale = (attackRange * 2f) / spriteSize;
        rangeFill.transform.localScale = new Vector3(targetScale, targetScale, 1f);

        // 更新外圈 60 個頂點
        for (int i = 0; i < 60; i++)
        {
            float angle = ((float)i / 60) * Mathf.PI * 2f;
            float x = Mathf.Sin(angle) * attackRange;
            float z = Mathf.Cos(angle) * attackRange;
            rangeBorder.SetPosition(i, center + new Vector3(x, 0, z));
        }
    }

    private void SecureComponents()
    {
        compToKeep.Add(typeof(Transform));
        compToKeep.Add(typeof(TowerPreview));
        compToKeep.Add(typeof(ForwardAttackDisplay));
        // ★ 舊的 RadiusDisplay 已經被我們移除了
    }

    private bool ComponentSecured(Component compToCheck)
    {
        return compToKeep.Contains(compToCheck.GetType());
    }

    private void DestroyExtraComponents()
    {
        Component[] components = GetComponents<Component>();

        foreach (var componentToCheck in components)
        {
            if (ComponentSecured(componentToCheck) == false)
                Destroy(componentToCheck);
        }
    }

    private void MakeAllMeshTransparent()
    {
        Material previewMaterial = FindFirstObjectByType<BuildManager>().GetBuildPreviewMaterial();

        foreach (var mesh in meshRenderers)
        {
            mesh.material = previewMaterial;
        }
    }
}