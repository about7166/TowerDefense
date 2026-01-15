using UnityEngine;

public class Enemy_Visuals_Spider : Enemy_Visuals
{
    [Header("腳的設定")]
    public float legSpeed = 3;
    public float increasedLegSpeed = 10;

    private Spider_Leg[] legs;

    [Header("身體動畫設定")]
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private float bodyAnimSpeed = 1;
    [SerializeField] private float maxHeight = 0.1f;

    private Vector3 startPosition;
    private float elapsedTime;

    [Header("煙的動畫")]
    [SerializeField] private ParticleSystem[] smokeFx;
    [SerializeField] private float smokeCooldown;
    private float smokeTimer;

    protected override void Awake()
    {
        base.Awake();
        legs = GetComponentsInChildren<Spider_Leg>();
    }

    protected override void Start()
    {
        base.Start();

        startPosition = bodyTransform.localPosition;
    }

    protected override void Update()
    {
        base.Update();

        AnimateBody();
        ActiveSmokeFxIfCan();
        UpdateSpiderLegs();
    }

    private void ActiveSmokeFxIfCan()
    {
        smokeTimer -= Time.deltaTime;

        if (smokeTimer < 0)
        {
            smokeTimer = smokeCooldown;

            foreach (var smoke in smokeFx)
            {
                smoke.Play();
            }
        }
    }

    private void AnimateBody()
    {
        elapsedTime += Time.deltaTime * bodyAnimSpeed;

        float sinValue = (Mathf.Sin(elapsedTime) + 1) / 2; //sin會上下上下，+1讓他變成0~2，/2讓他變成0~1
        float newY = Mathf.Lerp(0, maxHeight, sinValue);

        bodyTransform.localPosition = startPosition + new Vector3(0, newY, 0);
    }

    private void UpdateSpiderLegs()
    {
        foreach (var leg in legs)
        {
            leg.UpdateLeg();
        }
    }

    public void BrieflySpeedUpLegs()
    {
        foreach (var leg in legs)
        {
            leg.SpeedUpLeg();
        }
    }
}
