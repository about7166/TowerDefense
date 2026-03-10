using System.Collections;
using UnityEngine;

public class Hammer_Visuals : MonoBehaviour
{
    private Tower_Hammer myTower;

    [SerializeField] private ParticleSystem[] vfx;
    [SerializeField] private RotateObject valveRotation;

    [Header("槌子設定")]
    [SerializeField] private Transform hammer;
    [SerializeField] private Transform hammerHolder;
    [Space]
    [SerializeField] private Transform sideWire;
    [SerializeField] private Transform sideHandle;

    [Header("槌子動作設定")]
    [SerializeField] private float attackOffsetY = 0.57f;
    [SerializeField] private float attackDuration;
    [SerializeField] private float reloadDuration;
    [SerializeField] private float hammerHolderScaleY = 7;
    [SerializeField] private float hammerHolderTargetScaleY = 1;

    private void Awake()
    {
        myTower = GetComponent<Tower_Hammer>();
        reloadDuration = myTower.GetAttackCooldown() - attackDuration;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            PlayAttackAnimation();
    }

    public void PlayAttackAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(HammerAttackCo());
    }

    private void PlayVFXs()
    {
        foreach (var particle in vfx)
        {
            particle.Play();
        }
    }

    private IEnumerator HammerAttackCo()
    {
        // ★ 加入 null 檢查：只有當 valveRotation 存在時才執行
        if (valveRotation != null) valveRotation.AdjustRotationSpeed(25);

        StartCoroutine(ChangePositionCo(hammer, -attackOffsetY, attackDuration));
        StartCoroutine(ChangeScaleCo(hammerHolder, hammerHolderScaleY, attackDuration));

        // ★ 加入 null 檢查：只有當物件存在時才執行位移與縮放
        if (sideHandle != null) StartCoroutine(ChangePositionCo(sideHandle, 0.45f, attackDuration));
        if (sideWire != null) StartCoroutine(ChangeScaleCo(sideWire, 0.1f, attackDuration));

        yield return new WaitForSeconds(attackDuration);
        PlayVFXs();

        if (valveRotation != null) valveRotation.AdjustRotationSpeed(3);
        StartCoroutine(ChangePositionCo(hammer, attackOffsetY, reloadDuration));
        StartCoroutine(ChangeScaleCo(hammerHolder, hammerHolderTargetScaleY, reloadDuration));

        if (sideHandle != null) StartCoroutine(ChangePositionCo(sideHandle, -0.45f, reloadDuration));
        if (sideWire != null) StartCoroutine(ChangeScaleCo(sideWire, 1f, reloadDuration));
    }
    private IEnumerator ChangePositionCo(Transform transform, float yOffset, float duration = 0.1f)
    {
        float time = 0;

        Vector3 initialPosition = transform.localPosition;
        Vector3 targetPosition = new Vector3(initialPosition.x, initialPosition.y +  yOffset, initialPosition.z);

        while (time < duration)
        {
            transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
    }

    private IEnumerator ChangeScaleCo(Transform transform, float newScale, float duration = 0.25f)
    {
        float time = 0;

        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(initialScale.x, newScale, initialScale.z); // (1, newScale, 1);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
