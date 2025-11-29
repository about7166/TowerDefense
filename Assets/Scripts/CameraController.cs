using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float movementSpeed = 120;
    [SerializeField] private float mouseMovementSpeed = 5;

    [Header("邊界移動設定")]
    [SerializeField] private float edgeMovementSpeed = 50;
    [SerializeField] private float edgeThreshold = 10;
    private float screenWidth;
    private float screenHeight;

    [Header("旋轉設定")]
    [SerializeField] private Transform focusPoint;
    [SerializeField] private float maxFocusPointDistance = 15;
    [SerializeField] private float rotationSpeed = 200;
    [Space]
    private float pitch;
    [SerializeField] private float minPitch = 5f;
    [SerializeField] private float maxPitch = 85f;

    [Header("縮放設定")]
    [SerializeField] private float zoomSpeed = 10;
    [SerializeField] private float minZoom = 3;
    [SerializeField] private float maxZoom = 15;

    private float smoothTime = 0.1f;
    private Vector3 movementVelocity = Vector3.zero;
    private Vector3 mouseMovementVelocity = Vector3.zero;
    private Vector3 edgeMovementVelocity = Vector3.zero;
    private Vector3 zoomVelocity = Vector3.zero;
    private Vector3 lastMousePosition;

    private void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }
    void Update()
    {
        HandleRotation();
        HandleZoom();
        HandleEdgeMovement();
        HandleMouseMovement();
        HandleMovement();

        focusPoint.position = transform.position + (transform.forward * GetFocusPointDistane());
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
        Vector3 targetPosition = transform.position + zoomDirection;

        if (transform.position.y < minZoom && scroll > 0)
            return;

        if (transform.position.y > maxZoom && scroll < 0)
            return;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref zoomVelocity, smoothTime);
    }

    private float GetFocusPointDistane()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxFocusPointDistance))
            return hit.distance;

        return maxFocusPointDistance;
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) //1是右鍵
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float verticalRotation = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            pitch = Mathf.Clamp(pitch - verticalRotation, minPitch, maxPitch);

            transform.RotateAround(focusPoint.position, Vector3.up, horizontalRotation);

            transform.RotateAround(focusPoint.position, transform.right, pitch - transform.eulerAngles.x);

            transform.LookAt(focusPoint);
        }
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = transform.position;

        float vInput = Input.GetAxisRaw("Vertical");
        float hInput = Input.GetAxisRaw("Horizontal");

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        if (vInput > 0)
            targetPosition += flatForward * movementSpeed * Time.deltaTime;
        if (vInput < 0)
            targetPosition -= flatForward * movementSpeed * Time.deltaTime;


        if (hInput > 0)
            targetPosition += transform.right * movementSpeed * Time.deltaTime;
        if (hInput < 0)
            targetPosition -= transform.right * movementSpeed * Time.deltaTime;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref movementVelocity, smoothTime);
    }

    private void HandleMouseMovement()
    {
        if (Input.GetMouseButtonDown(2))//2是滑鼠中鍵
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 positionDifference = Input.mousePosition - lastMousePosition;
            Vector3 moveRight = transform.right * (-positionDifference.x) * mouseMovementSpeed * Time.deltaTime;
            Vector3 moveForward = transform.forward * (-positionDifference.y) * mouseMovementSpeed * Time.deltaTime;

            moveRight.y = 0;
            moveForward.y = 0;

            Vector3 movement = moveRight + moveForward;
            Vector3 targetPosition = transform.position + movement;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref mouseMovementVelocity, smoothTime);
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleEdgeMovement()
    {
        Vector3 targetPosition = transform.position;
        Vector3 mousePosition = Input.mousePosition;
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        if (mousePosition.x > screenWidth - edgeThreshold)
            targetPosition += transform.right * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.x < edgeThreshold)
            targetPosition -= transform.right * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.y > screenHeight - edgeThreshold)
            targetPosition += flatForward * edgeMovementSpeed * Time.deltaTime;

        if (mousePosition.y < edgeThreshold)
            targetPosition -= flatForward * edgeMovementSpeed * Time.deltaTime;

        transform.position =Vector3.SmoothDamp(transform.position, targetPosition, ref edgeMovementVelocity, smoothTime);
    }
}
