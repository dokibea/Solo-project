using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Movement : MonoBehaviour
{
    //serialized fields
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _dashForce;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _dashCooldown;

    // private value variables
    private Vector2 _moveDir;
    private float _dashMomentum = 0;
    private float _dashMomentumDecayTime = 0.3f;
    private float _decayRate;
    private bool _isJumping = false;
    private bool _isDashing = false;
    private int _jumpCount = 0;

    
    // Component variables
    private Rigidbody2D _rb;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _decayRate = _dashForce / _dashMomentumDecayTime;
    }

    private void OnEnable()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
         _rb.linearVelocity = new Vector2(_moveDir.x * _movementSpeed + _dashMomentum, _rb.linearVelocityY);
         if(_dashMomentum != 0){
         _dashMomentum = Mathf.MoveTowards(_dashMomentum, 0, _decayRate * Time.fixedDeltaTime);
        }
    }

    public void Moving(InputAction.CallbackContext context)
    {
            _moveDir = context.ReadValue<Vector2>();
            Debug.Log(_moveDir.x);   
    }

    public void Jump(InputAction.CallbackContext context)
    {
        DoubleJumpCheck();
        if (context.performed && !_isJumping)
        {
            _rb.AddForce(Vector2.up.normalized * _jumpForce, ForceMode2D.Impulse);
            _jumpCount++;
        }
        Debug.Log(_rb.linearVelocityY);
    }

    private void DoubleJumpCheck()
    {
       if(_jumpCount >= 1)
        {
            _isJumping = true;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !_isDashing)
        {
            _dashMomentum = _moveDir.x * _dashForce;
            StartCoroutine(DashCooldown());
            Debug.Log("Dashing");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Ground":
                Groundreset();
                break;
        }
    }

    private void Groundreset()
    {
        _isJumping = false;
        _jumpCount = 0;
        Debug.Log("Grounded");
    }

    private IEnumerator DashCooldown()
    {
        _isDashing = true;
        yield return new WaitForSeconds(_dashCooldown);
        _isDashing = false;
    }
}


