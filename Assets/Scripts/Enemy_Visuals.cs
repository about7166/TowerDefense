using Unity.Mathematics;
using UnityEngine;

public class Enemy_Visuals : MonoBehaviour
{
    [SerializeField] private Transform visuals;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float verticalRotationSpeed;

    private void Update()
    {
        AlignWithSlope(); 
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
