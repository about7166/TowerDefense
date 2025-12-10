using Unity.VisualScripting;
using UnityEngine;

public class TowerPreview : MonoBehaviour
{
    private MeshRenderer[] meshRenderers;
    private TowerAttackRadiusDisplay attackRadiusDisplay;
    private Tower myTower;

    private float attackRange;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        myTower = GetComponent<Tower>();
        attackRadiusDisplay = transform.AddComponent<TowerAttackRadiusDisplay>();
        attackRange = myTower.GetAttackRange();

        MakeAllMeshTransparent();
        DestroyExtraComponents();
    }

    public void ShowPreview(bool showPreview, Vector3 previewPosition)
    {
        transform.position = previewPosition;
        attackRadiusDisplay.CreateCircle(showPreview, attackRange);
    }
    private void DestroyExtraComponents()
    {
        if (myTower != null)
        {
            Crossbow_Visuals crossbow_Visuals = GetComponent<Crossbow_Visuals>();

            Destroy(crossbow_Visuals);
            Destroy(myTower);
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
