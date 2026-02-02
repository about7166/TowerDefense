using UnityEngine;

public class Enemy_Visuals_Swarm : Enemy_Visuals
{
    [Header("模型變體")]
    [SerializeField] private GameObject[] variants;

    [Header("彈跳設定")]
    [SerializeField] private AnimationCurve bounceCurve;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float minHeight = 0.1f;
    [SerializeField] private float maxHeight = 0.3f;
    private float bounceTimer;

    [Space]
    [SerializeField] private TrailRenderer myTrail;

    protected override void Awake()
    {
        ChooseVisualVariant();
        CollectDefaultMaterials();

        myTrail = GetComponentInChildren<TrailRenderer>();
        myTrail.gameObject.SetActive(false);
    }

    public void EnableTrail()
    {
        if (myTrail == null)
            return;

        myTrail.Clear();
        myTrail.gameObject.SetActive(true);
        myTrail.transform.parent = visuals.transform;
        myTrail.transform.localPosition = Vector3.zero;
    }

    public void OnEnable()
    {
        EnableTrail();
    }

    private void OnDisable()
    {
        if (myTrail != null)
            myTrail.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        BounceEffect();
    }

    private void BounceEffect()
    {
        bounceTimer += Time.deltaTime * bounceSpeed;
        // currentValue % 1 會讓數值永遠保持在 0.0 ~ 0.99 之間循環
        float bounceValue = bounceCurve.Evaluate(bounceTimer % 1);
        float bounceHeight = Mathf.Lerp(minHeight, maxHeight, bounceValue);

        visuals.localPosition = new Vector3(visuals.localPosition.x, bounceHeight, visuals.localPosition.z);
    }

    private void ChooseVisualVariant()
    {
        foreach (var option in variants)
        {
            option.SetActive(false);
        }

        int randomIndex = Random.Range(0, variants.Length);
        GameObject newVisuals = variants[randomIndex];

        newVisuals.SetActive(true);
        visuals = newVisuals.transform;
    }
}
