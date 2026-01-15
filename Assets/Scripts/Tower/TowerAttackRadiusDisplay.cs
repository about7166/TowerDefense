using UnityEngine;


[RequireComponent (typeof(LineRenderer))]
public class TowerAttackRadiusDisplay : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float radius;
    private int segments = 50;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;//多一段讓圓成形
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = FindFirstObjectByType<BuildManager>().GetAttackRadiusMaterial();
    }


    public void CreateCircle(bool showCircle, float radius = 0)
    {
        lineRenderer.enabled = showCircle;

        if (showCircle == false)
            return;

        float angle = 0;
        Vector3 center = transform.position;

        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x + center.x, center.y, z + center.z));
            angle += 360f / segments;
        }

        lineRenderer.SetPosition(segments, lineRenderer.GetPosition(0));
    }
}
