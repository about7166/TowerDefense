using Unity.Mathematics;
using UnityEngine;

public class Enemy_Visuals : MonoBehaviour
{
    [SerializeField] protected Transform visuals;// 敵人的模型
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float verticalRotationSpeed;

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        AlignWithSlope(); // 每幀都去檢查並貼合斜坡
    }
    private void AlignWithSlope()
    {
        if (visuals == null)
            return;

        if (Physics.Raycast(visuals.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, whatIsGround))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            visuals.rotation = Quaternion.Slerp(visuals.rotation, targetRotation, Time.deltaTime * verticalRotationSpeed);
        }
    }
}
