using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
    public static bool IsSprinting { get; private set; }

    [SerializeField] private float walkSpeed = 5.2f;
    [SerializeField] private float sprintSpeed = 8.2f;
    [SerializeField] private float lookSensitivity = 2.1f;
    [SerializeField] private float jumpHeight = 1.15f;
    [SerializeField] private Transform view;

    private CharacterController controller;
    private float pitch;
    private float verticalSpeed;
    private float interactionCooldown;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (view == null && Camera.main != null)
            view = Camera.main.transform;
    }

    private void Update()
    {
        if (view == null && Camera.main != null)
            view = Camera.main.transform;

        if (DesertGame.Instance == null || DesertGame.Instance.IsDead || DesertGame.Instance.IsComplete)
        {
            IsSprinting = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        bool canLook = Cursor.lockState == CursorLockMode.Locked && view != null;
        if (canLook)
        {
            float yaw = Input.GetAxis("Mouse X") * lookSensitivity;
            float lookY = Input.GetAxis("Mouse Y") * lookSensitivity;
            pitch = Mathf.Clamp(pitch - lookY, -82f, 82f);
            view.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            transform.Rotate(Vector3.up * yaw);
        }

        IsSprinting = Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0.1f;
        float speed = IsSprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        move = Vector3.ClampMagnitude(move, 1f) * speed;

        if (controller.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -1.5f;
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        move.y = verticalSpeed;
        controller.Move(move * Time.deltaTime);

        interactionCooldown -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E) && interactionCooldown <= 0f)
        {
            interactionCooldown = 0.2f;
            Interact();
        }
    }

    private void Interact()
    {
        if (view == null || !Physics.Raycast(view.position, view.forward, out RaycastHit hit, 3.6f))
            return;

        WorldPickup pickup = hit.collider.GetComponentInParent<WorldPickup>();
        if (pickup != null)
        {
            DesertGame.Instance.Collect(pickup);
            return;
        }

        if (hit.collider.GetComponentInParent<OasisSource>() != null)
            DesertGame.Instance.Drink();
    }
}
