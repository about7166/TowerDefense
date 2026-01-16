using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Enemy_Visuals : MonoBehaviour
{
    [SerializeField] private GameObject onDeathFx;
    [SerializeField] private float onDeathFxScale = 0.5f;
    [Space]
    [SerializeField] protected Transform visuals;// 敵人的模型
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float verticalRotationSpeed;

    [Header("透明設定")]
    [SerializeField] private Material transparentMaterial;
    private List<Material> originalMaterial;
    private MeshRenderer[] myRenderers;

    protected virtual void Awake()
    {
        CollectDefaultMaterials();        
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        AlignWithSlope(); //每幀都去檢查並貼合斜坡
    }

    public void CreateOnDeathVFX()
    {
        GameObject newDeathVFX = Instantiate(onDeathFx, transform.position + new Vector3(0, 0.15f, 0), Quaternion.identity);
        newDeathVFX.transform.localScale = new Vector3(onDeathFxScale, onDeathFxScale, onDeathFxScale);
    }

    public void MakeTransparent(bool transparent)
    {
        for (int i = 0; i < myRenderers.Length; i++)
        {
            Material materialToApply = transparent ? transparentMaterial : originalMaterial[i];
            myRenderers[i].material = materialToApply;
        }
    }

    protected void CollectDefaultMaterials() //收集預設材料
    {
        myRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMaterial = new List<Material>();

        foreach (var renderer in myRenderers)
        {
            originalMaterial.Add(renderer.material);
        }
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
