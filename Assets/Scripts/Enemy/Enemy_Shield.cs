using System.Collections;
using UnityEngine;

public class Enemy_Shield : MonoBehaviour, IDamagable
{
    [SerializeField] private float currentShieldAmount;

    [Header("¨üÀ»³]©w")]
    [SerializeField] private Material shieldMaterial;
    [SerializeField] private float defaultShieldGlow = 1;
    [SerializeField] private float impactShieldGlow = 3;
    [SerializeField] private float impactScaleMultiplier = 0.97f;
    [SerializeField] private float impactSpeed = 0.1f;
    [SerializeField] private float impactResetDuration = 0.5f;

    private float defaultScale;
    private string shieldFrenelParametr = "_FresnelPower";
    private Coroutine currentCo;


    private void Start()
    {
        defaultScale = transform.localScale.x;
    }
    public void SetupShield(int shieldAmount)
    {
        currentShieldAmount = shieldAmount;
    }

    public void TakeDamage(int damage)
    {
        currentShieldAmount -= damage;
        ActivateShieldImpact();

        if (currentShieldAmount <= 0 )
            gameObject.SetActive(false);
    }

    private void ActivateShieldImpact()
    {
        if (currentCo != null)
            StopCoroutine(currentCo);

        currentCo =StartCoroutine(ImpactCo());
    }

    private IEnumerator ImpactCo()
    {
        yield return StartCoroutine(ShieldChangeCo(impactShieldGlow, defaultScale * impactScaleMultiplier, impactSpeed));

        StartCoroutine(ShieldChangeCo(defaultShieldGlow, defaultScale, impactResetDuration));
    }

    private IEnumerator ShieldChangeCo(float targetGlow, float targetScale, float duration)
    {
        float time = 0;
        float startGlow = shieldMaterial.GetFloat(shieldFrenelParametr);
        Vector3 initialScale = transform.localScale;
        Vector3 newTargetScale = new Vector3(targetScale, targetScale, targetScale);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, newTargetScale, time / duration);

            float newGlow = Mathf.Lerp(startGlow, targetGlow, time / duration);
            shieldMaterial.SetFloat(shieldFrenelParametr, newGlow);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = newTargetScale;
        shieldMaterial.SetFloat(shieldFrenelParametr, targetGlow);
    }
}
