using UnityEngine;

public class SwingObject : MonoBehaviour
{
    [Header("搖晃設定")]
    [SerializeField] private Vector3 swingAxis;
    [SerializeField] private float swingDegree = 10;
    [SerializeField] private float swingSpeed = 1;

    private Quaternion startRotation;

    private void Start()
    {
        startRotation = transform.localRotation;
    }

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingDegree;

        transform.localRotation = startRotation * Quaternion.AngleAxis(angle, swingAxis.normalized); //(角度，旋轉軸)
    }
}
