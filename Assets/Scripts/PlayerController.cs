using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float MovementSpeed;
    [SerializeField] private float TurnSpeed;
    [SerializeField] private float JumpForce;

    private float Y_Rotation;
    private bool IsGrounded = true;
    private bool IsJumpArea = false;

    [SerializeField] private GameController gameController;

    private Transform PlayerTransform;

    private void Awake()
    {
        PlayerTransform = gameObject.transform;
        Y_Rotation = 0;
    }

    private void Start()
    {
        Initialization();
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void Initialization()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsGrounded = true;
    }

    private void PlayerMovement()
    {
        if(IsGrounded && Input.GetKey(KeyCode.Space) && IsJumpArea)
        {
            transform.GetComponent<Rigidbody>().AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            IsGrounded = false;
            gameController.PlayerJumpHandler();
        }

        if(!IsJumpArea)
        {
            float HorizontalMovement = Input.GetAxisRaw("Horizontal") * Time.fixedDeltaTime * MovementSpeed;
            float VerticalMovement = Input.GetAxisRaw("Vertical") * Time.fixedDeltaTime * MovementSpeed;
            float HorizontalRotation = Input.GetAxisRaw("Mouse X") * Time.fixedDeltaTime * TurnSpeed;

            Vector3 movDir = transform.right * HorizontalMovement + transform.forward * VerticalMovement;
            transform.position += movDir;

            if (Input.GetMouseButton(0))
            {
                Y_Rotation -= HorizontalRotation;
                PlayerTransform.rotation = Quaternion.Euler(0, Y_Rotation, 0);
            }
        }  
    }

    private void OnCollisionEnter(Collision collision)
    {
        gameController.PlayerActionHandler(collision.transform);

        string value = collision.gameObject.tag.ToString();
        if (value == "Ground")
        {
            IsGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        gameController.PlayerActionHandler(collision.transform);

        string value = collision.gameObject.tag.ToString();
        if (value == "Ground")
        {
            IsGrounded = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        gameController.PlayerActionHandler(other.transform);

        string value = other.gameObject.tag.ToString();

        if (value == "JumpArea")
        {
            IsJumpArea = true;
            transform.position = other.transform.position;
            other.gameObject.SetActive(false);
            transform.rotation = Quaternion.Euler(0, 90f, 0);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        gameController.PlayerActionHandler(other.transform);
    }
}
