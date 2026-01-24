using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Harpoon_Visuals : MonoBehaviour
{
    private ObjectPoolManager objectPool;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [Space]
    [SerializeField] private GameObject linkPrefab;
    [SerializeField] private Transform linksParent;
    [SerializeField] private float linkDistance = 0.2f; //鐵環間距
    [SerializeField] private int maxLinks = 100;

    private List<Projectile_HarpoonLink> links = new List<Projectile_HarpoonLink>();

    [Space]
    [SerializeField] private GameObject onElectricVfx;
    [SerializeField] private Vector3 vfxOffset;
    private GameObject currentVfx;

    private void Start()
    {
        InitializeLinks();
        objectPool = ObjectPoolManager.instance;
    }

    private void Update()
    {
        if (endPoint == null)
            return;

        ActivateLinksIfNeeded();
    }

    public void CreateElectricVFX(Transform targetTransform)
    {
        currentVfx = objectPool.Get(onElectricVfx, targetTransform.position + vfxOffset, Quaternion.identity, targetTransform);
    }

    private void DestroyElectricVFX()
    {
        if (currentVfx != null)
            objectPool.Remove(currentVfx);
    }

    public void EnableChainVisuals(bool enable, Transform newEndPoint = null)
    {
        if (enable)
        {
            endPoint = newEndPoint;
        }

        if (enable == false)
        {
            endPoint = startPoint;
            DestroyElectricVFX();
        }
    }

    private void ActivateLinksIfNeeded()
    {
        Vector3 direction = (endPoint.position - startPoint.position).normalized; //方向
        float distance = Vector3.Distance(startPoint.position, endPoint.position); //總距離
        int activeLinksAmount = Mathf.Min(maxLinks, Mathf.CeilToInt(distance / linkDistance)); //CeilToInt無條件進位

        for (int i = 0; i < links.Count; i++)
        {
            if (i < activeLinksAmount)
            {
                Vector3 newPosition = startPoint.position + direction * linkDistance * (i + 1); //起點 + (方向 * 單個距離 * 第幾個)
                links[i].EnableLink(true, newPosition); //啟用這個鐵環，並把它放到算出來的位置
            }
            else
                links[i].EnableLink(false, Vector3.zero); //不需要的藏起來

            if (i != links.Count - 1)
                links[i].UpdateLineRenderer(links[i], links[i + 1]); //只有顯示出來的才需要連線
        }
    }

    private void InitializeLinks() //一次生出所有鐵環並關掉，準備隨時取用
    {
        for (int i = 0; i < maxLinks; i++)
        {
            Projectile_HarpoonLink newLink =
                Instantiate(linkPrefab, startPoint.position, Quaternion.identity, linksParent).GetComponent<Projectile_HarpoonLink>();

            links.Add(newLink);
        }
    }
}
