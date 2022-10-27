using Cinemachine.Utility;
using UnityEngine;
using Cursor = UnityEngine.Cursor;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : PortalTraveller
{
    public float last_dst = 0;
    public Vector3 move_dir;
    public Vector3 cam_pos { get { return camera_transform.position ; } }
    public Quaternion cam_local_rot { get { return camera_transform.localRotation ; } }
    public Vector3 cam_dir { get { return camera_transform.forward; } }
    virtual public bool is_grounded {get { return m_CharacterController != null && m_CharacterController.isGrounded; }}
    protected CharacterController m_CharacterController;
    [SerializeField]
    protected float m_MouseSensitivity = 800f;
    [SerializeField]
    protected float m_MovementSpeed = 5f;
    public float gravity
    {
        get { return is_grounded ? 0f : m_gravity; }
        set { m_gravity = value; }
    }
    [SerializeField]
    protected bool m_MoveWithMouse = true;
    protected virtual float speed_y
    {
        get
        {
            if (is_grounded)
                m_speed_y = 0f;
            return m_speed_y;
        }
        set { m_speed_y = value; }
    }
    private float m_gravity = 9.8f;
    [SerializeField]
    private Transform camera_transform = null;
    [SerializeField]
    private byte m_ButtonMovementFlags;
    float m_speed_y = 0f;
    float m_XRotation = 0f;

    void Start()
    {
        OnStart();
    }

    new protected virtual void OnStart()
    {
        base.OnStart();
        #if ENABLE_INPUT_SYSTEM
                        Debug.Log("The FirstPersonController uses the legacy input system. Please set it in Project Settings");
                        m_MoveWithMouse = false;
        #endif
        if (m_MoveWithMouse)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        m_CharacterController = GetComponent<CharacterController>();
    }


    void Update()
    {
        Look();
        Move();
        GetSprayInput();
    }

    //=================================== Move ===================================

    private void Look()
    {
        Vector2 lookInput = GetLookInput();

        m_XRotation -= lookInput.y;
        m_XRotation = Mathf.Clamp(m_XRotation, -90f, 90f);

        camera_transform.localRotation = Quaternion.Euler(m_XRotation, 0, 0);
        transform.Rotate(Vector3.up * lookInput.x, Space.World);
    }

    private void Move()
    {
        Vector3 movementInput = GetMovementInput();
        Vector3 move_xz = (transform.right * movementInput.x + transform.forward * movementInput.z) * m_MovementSpeed;
        Vector3 move_y = transform.up * movementInput.y;
        Vector3 move = move_xz + move_y;
        move_dir = move.normalized;
        m_CharacterController.Move(move * Time.deltaTime);
    }

    private Vector2 GetLookInput()
    {
        float mouseX = 0;
        float mouseY = 0;
        if (m_MoveWithMouse)
        {
            mouseX = Input.GetAxis("Mouse X") * m_MouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * m_MouseSensitivity * Time.deltaTime;
        }
        return new Vector2(mouseX, mouseY);
    }

    virtual protected Vector3 GetMovementInput()
    {
        float x = 0;
        float z = 0;

        if (m_MoveWithMouse)
        {
            x = Input.GetAxis("Horizontal");
            speed_y += Input.GetAxis("Jump");
            speed_y -= m_gravity * 0.1f;
            speed_y = Mathf.Clamp(speed_y, -5f, 5f);
            z = Input.GetAxis("Vertical");
        }

        return new Vector3(x, speed_y, z);
    }

    private Vector3 GetSprayInput()
    {
        bool is_spray = Input.GetButtonDown("Spray");
        if (is_spray)
            Debug.Log("this is spray button");

        return new Vector3(1, 0, 1);
    }

}
