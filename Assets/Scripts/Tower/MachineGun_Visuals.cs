using System.Collections;
using UnityEngine;

public class MachineGun_Visuals : MonoBehaviour
{
    [Header("後座力設定")]
    [SerializeField] private float recoilOffset = -0.1f;
    [SerializeField] private float recoverSpeed = 0.25f;
    [SerializeField] private ParticleSystem onAttackFx;

    public void RecoilFx(Transform gunPoint)
    {
        PlayOnAttackFx(gunPoint.position);
        StartCoroutine(RecoilCo(gunPoint));
    }

    private void PlayOnAttackFx(Vector3 position)
    {
        onAttackFx.transform.position = position;
        onAttackFx.Play();
    }

    private IEnumerator RecoilCo(Transform gunPoint)
    {
        Transform objectToMove = gunPoint.transform.parent;
        Vector3 originalPosition = objectToMove.localPosition;
        Vector3 recoilPosition = originalPosition + new Vector3(0, 0, recoilOffset);

        objectToMove.localPosition = recoilPosition;

        while (objectToMove.localPosition != originalPosition)
        {
            objectToMove.localPosition =
                Vector3.MoveTowards(objectToMove.localPosition, originalPosition, recoverSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
