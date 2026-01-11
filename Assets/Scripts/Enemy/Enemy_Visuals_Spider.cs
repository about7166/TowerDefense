using UnityEngine;

public class Enemy_Visuals_Spider : Enemy_Visuals
{
    [Header("¸}ªº³]©w")]
    public float legSpeed = 3;

    private Spider_Leg[] legs;

    protected override void Start()
    {
        base.Start();

        legs = GetComponentsInChildren<Spider_Leg>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateSpiderLegs();
    }

    private void UpdateSpiderLegs()
    {
        foreach (var leg in legs)
        {
            leg.UpdateLeg();
        }
    }
}
