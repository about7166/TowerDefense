using UnityEngine;
using UnityEngine.EventSystems;

public class UI_BuildButtonOnHoverEffect : MonoBehaviour
{
    [SerializeField] private float adjustMovementSpeed = 10;

    [SerializeField] private float showcaseY;
    [SerializeField] private float defaultY;

    private float targetY;
    private bool canMove;

    private void Update()
    {
        if (Mathf.Abs(transform.position.y - targetY) > 0.01f && canMove)
        {
            float newPositionY = Mathf.Lerp(transform.position.y, targetY, adjustMovementSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newPositionY, transform.position.z);
        }
    }

    public void ToggleMovement(bool buttonMenuActive)
    {
        canMove = buttonMenuActive;
        SetTargetY(defaultY); //重置目標Y座標為預設值(確保它不會卡在showcaseY)

        if (buttonMenuActive == false)
            SetPositionToDefault(); //如果菜單關閉，強制將位置設回defaultY
    }

    private void SetPositionToDefault()
    {
        transform.position = new Vector3(transform.position.x, defaultY, transform.position.z);
    }

    private void SetTargetY(float newY) => targetY = newY;

    public void ShowcaseButton(bool showcase)
    {
        if (showcase)
            SetTargetY(showcaseY);
        else
            SetTargetY(defaultY);
    }
}
