using UnityEngine;
using UnityEngine.EventSystems;

public class BuildSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private TileAnimator tileAnimator;
    private BuildManager buildManger;
    private Vector3 defaultposition;

    private bool tileCanBeMoved = true;
    private bool buildSlotAvailable = true;

    private Coroutine currentMovementUpCo;
    private Coroutine moveToDefaultCo;

    private void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        buildManger = FindFirstObjectByType<BuildManager>();
        defaultposition = transform.position;
    }

    private void Start()
    {
        if (buildSlotAvailable == false)
            transform.position += new Vector3(0, 0.1f);
    }

    public void SetSlotAvailableTo(bool value) => buildSlotAvailable = value;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buildSlotAvailable == false || tileAnimator.IsGridMoving())
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (buildManger.GetSelectedSlot() == this)
            return;

        SnapToBeforeBuildPosition();
        buildManger.EnableBuildMenu();
        buildManger.SelectBuildSlot(this);
        MoveTileUp();

        tileCanBeMoved = false;

        ui.buildButtonsUI.GetLastSelectedButton()?.SelectButton(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildSlotAvailable == false || tileAnimator.IsGridMoving())
            return;

        if (tileCanBeMoved == false)
            return;

        MoveTileUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(buildSlotAvailable == false || tileAnimator.IsGridMoving()) 
            return;

        if (tileCanBeMoved == false)
            return;

        if (currentMovementUpCo != null)
        {
            Invoke(nameof(MoveToDefaultPosition), tileAnimator.GetTravelDuration());
        }
        else
            MoveToDefaultPosition();
    }

    public void UnSelectTile()
    {
        MoveToDefaultPosition();
        tileCanBeMoved = true;
    }

    private void MoveTileUp()
    {
        Vector3 targetPosition = transform.position + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        currentMovementUpCo = StartCoroutine(tileAnimator.MoveTileCo(transform, targetPosition));
    }

    private void MoveToDefaultPosition()
    {
        moveToDefaultCo = StartCoroutine(tileAnimator.MoveTileCo(transform, defaultposition));
    }

    public void SnapToDefaultPositionImmidiatly()
    {
        if (moveToDefaultCo != null)
            StopCoroutine(moveToDefaultCo);

        transform.position = defaultposition;
    }

    public void SnapToBeforeBuildPosition()
    {
        Vector3 targetPosition = defaultposition + new Vector3(0, tileAnimator.GetBuildOffset(), 0);
        transform.position = targetPosition;
    }

    public Vector3 GetBuildPosition(float yOffset) => defaultposition + new Vector3(0, yOffset);
}
