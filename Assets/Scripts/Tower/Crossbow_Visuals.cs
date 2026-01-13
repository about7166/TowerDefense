using System.Collections;
using UnityEngine;

public class Crossbow_Visuals : MonoBehaviour
{
    [Header("攻擊效果")]
    [SerializeField] private GameObject onHitFX;
    [SerializeField] private LineRenderer attackVisuals;
    [SerializeField] private float attackVisualDuration = 0.1f;
    private Vector3 hitPoint;

    [Header("發光效果")]
    [SerializeField] private MeshRenderer meshRenderer;
    private Material material;

    [Space]
    [SerializeField] private float maxIntensity = 150; //調整發光亮度
    private float currentIntensity;
    [Space]
    [SerializeField] private Color startColor;
    [SerializeField] private Color endcolor;

    [Header("轉子")]
    [SerializeField] private Transform rotor;
    [SerializeField] private Transform rotorUnloaded;
    [SerializeField] private Transform rotorLoaded;

    [Header("前弓弦")]
    [SerializeField] private LineRenderer frontString_L;
    [SerializeField] private LineRenderer frontString_R;

    [Space]

    [SerializeField] private Transform frontStartPoint_L;
    [SerializeField] private Transform frontStartPoint_R;
    [SerializeField] private Transform frontEndPoint_L;
    [SerializeField] private Transform frontEndPoint_R;

    [Header("後弓弦")]
    [SerializeField] private LineRenderer backString_L;
    [SerializeField] private LineRenderer backString_R;

    [Space]

    [SerializeField] private Transform backStartPoint_L;
    [SerializeField] private Transform backStartPoint_R;
    [SerializeField] private Transform backEndPoint_L;
    [SerializeField] private Transform backEndPoint_R;

    [SerializeField] private LineRenderer[] lineRenderers;

    private void Awake()
    {        
        material = new Material(meshRenderer.material);
        meshRenderer.material = material;

        UpdateMaterialsOnLineRenders();
        StartCoroutine(ChangeEmission(1));
    }

    private void UpdateMaterialsOnLineRenders()
    {
        foreach (var lr in lineRenderers)
        {
            lr.material = material;
        }
    }

    private void Update()
    {
        UpdateEmissionColor();
        UpdateStrings();
        UpdateAttackVisualsIfNeeded();
    }

    //擊中特效
    public void CreateOnHitFX(Vector3 hitPoint)
    {
        GameObject newFX = Instantiate(onHitFX, hitPoint, Random.rotation);
        Destroy(newFX, 1);
    }

    private void UpdateAttackVisualsIfNeeded()
    {
        if (attackVisuals.enabled && hitPoint != Vector3.zero)
            attackVisuals.SetPosition(1, hitPoint);
    }

    private void UpdateStrings()
    {
        UpdateStringVisual(frontString_L, frontStartPoint_L, frontEndPoint_L);
        UpdateStringVisual(frontString_R, frontStartPoint_R, frontEndPoint_R);
        UpdateStringVisual(backString_L, backStartPoint_L, backEndPoint_L);
        UpdateStringVisual(backString_R, backStartPoint_R, backEndPoint_R);
    }

    private void UpdateEmissionColor()
    {
        Color emissionColor = Color.Lerp(startColor, endcolor, currentIntensity / maxIntensity);

        emissionColor = emissionColor * Mathf.LinearToGammaSpace(currentIntensity);

        material.SetColor("_EmissionColor", emissionColor);
    }

    public void PlayReloaxVFX(float duration)
    {
        float newDuration = duration / 2;

        StartCoroutine(ChangeEmission(newDuration));
        StartCoroutine(UpdateRotorPosition(newDuration));

    }

    public void PlayAttackVFX(Vector3 startPoint, Vector3 endPoint)
    {
        StartCoroutine(VFXCoroutione(startPoint,endPoint));
    }

    private IEnumerator VFXCoroutione(Vector3 startPoint, Vector3 endPoint)
    {
        hitPoint = endPoint;
        
        attackVisuals.enabled = true;
        attackVisuals.SetPosition(0, startPoint);
        attackVisuals.SetPosition(1, endPoint);

        yield return new WaitForSeconds(attackVisualDuration);
        attackVisuals.enabled = false;
    }

    private IEnumerator ChangeEmission(float duration)
    {
        float startTime = Time.time;
        float startIntensity = 0;

        while (Time.time - startTime < duration)
        {
            float tValue = (Time.time - startTime) / duration;
            currentIntensity = Mathf.Lerp(startIntensity, maxIntensity, tValue);
            yield return null;
        }

        currentIntensity = maxIntensity;
    }

    private IEnumerator UpdateRotorPosition(float duration)
    {
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float tValue = (Time.time - startTime) / duration;
            rotor.position = Vector3.Lerp(rotorUnloaded.position, rotorLoaded.position, tValue);
            yield return null;
        }

        rotor.position = rotorLoaded.position;
    }

    private void UpdateStringVisual(LineRenderer lineRenderer, Transform startPoint, Transform endPoint)
    {
        lineRenderer.SetPosition(0, startPoint.position);
        lineRenderer.SetPosition(1, endPoint.position);
    }
}
