using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    //serialized fields
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _dashForce;
    [SerializeField] private float _jumpForce;

    // private value variables
    private Vector2 _moveDir;
    private bool _isJumping = false;
    private bool _isDashing = false;
    private int _jumpCount = 0;

    private int _dashCount = 0;
    
    // Component variables
    private Rigidbody2D _rb;
    private PlayerInput _playerInputRef;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
         _rb = GetComponent<Rigidbody2D>();
        _playerInputRef = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       _rb.linearVelocity = new Vector2(_moveDir.x * _movementSpeed, _rb.linearVelocityY);
    }

    public void Moving(InputAction.CallbackContext context)
    {
            _moveDir = context.ReadValue<Vector2>();
            Debug.Log(_moveDir.x);   
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && !_isJumping)
        {
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
            DoubleJumpCheck();
        }
        Debug.Log(_rb.linearVelocityY);
    }

    private void DoubleJumpCheck()
    {
        switch (_jumpCount)
        {
            case 0:
                _jumpCount++;
                break;
            case 1:
                _isJumping = true;
                break;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !_isDashing)
        {
            _isDashing = true;
            _rb.AddForce(new Vector2(_moveDir.x * _dashForce, 0), ForceMode2D.Impulse);
            Debug.Log("Dashing");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Ground":
                _isJumping = false;
                _isDashing = false;
                _jumpCount = 0;
                Debug.Log("Grounded");
                break;
        }
    }
}
