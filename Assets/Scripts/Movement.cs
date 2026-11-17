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
    [SerializeField] private float _wallKickForce;
    [SerializeField] private float _wallJumpLockTime;

    // private value variables
    private Vector2 _moveDir;
    private float _inputX;
    private float _wallJumpAngle =  60f;
    private float _dashMomentum = 0;
    private float _dashMomentumDecayTime = 0.3f;
    private float _decayRate;
    private bool _isJumping = false;
    private bool _isDashing = false;
    private bool _wallSliding = false;
    private bool _inputLock = false;
    private int _jumpCount = 0;
    private float _wallDir;
    private float _wallJumpRad;

    
    // Component variables
    private Rigidbody2D _rb;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _decayRate = _dashForce / _dashMomentumDecayTime;
        _wallJumpRad = _wallJumpAngle * Mathf.Deg2Rad;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!_wallSliding)
        {
             _inputX = _inputLock ? 0f : _moveDir.x;
            _rb.linearVelocity = new Vector2(_inputX * _movementSpeed + _dashMomentum, _rb.linearVelocityY);
        }
        if(_dashMomentum != 0){
            _dashMomentum = Mathf.MoveTowards(_dashMomentum, 0, _decayRate * Time.fixedDeltaTime);
        }
    }

    public void Moving(InputAction.CallbackContext context)
    {
         _moveDir = context.ReadValue<Vector2>();
         switch(_moveDir.x)
         {
             case >= 1:
                 transform.rotation = Quaternion.Euler(0, 0, 0);
                 break;
            case <= -1:
                  transform.rotation = Quaternion.Euler(0, 180, 0);
                  break;
         }
          Debug.Log(_moveDir.x);   
    }

    public void Jump(InputAction.CallbackContext context)
    {
        DoubleJumpCheck();
        if (context.performed && !_isJumping && !_wallSliding)
        {
            _rb.linearVelocityY = 0;
            _rb.AddForce(Vector2.up.normalized * _jumpForce, ForceMode2D.Impulse);
            _jumpCount++;
            Debug.Log("Jumping");
        }
        else if(context.performed && _wallSliding)
        {
             Vector2 dir = new Vector2(_wallDir * Mathf.Cos(_wallJumpRad), Mathf.Sin(_wallJumpRad));

             _rb.linearVelocityY = 0;
             _rb.AddForce( Vector2.up * dir.y * _wallKickForce, ForceMode2D.Impulse);
             _dashMomentum = dir.x * _wallKickForce;
              StartCoroutine(WallJumpLock());

             _wallSliding = false;
             Debug.Log("Wall Jumping");
        } 
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
            _rb.linearVelocityY = 0;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Wall":
                WallSlide(collision);
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

    private void WallSlide(Collision2D collision)
    {
          _wallSliding = true;
          _wallDir = collision.GetContact(0).normal.x;
          _rb.linearVelocityY = 0;
          transform.rotation = Quaternion.Euler(0, _wallDir > 0 ? 0 : 180, 0);
    }

    private IEnumerator WallJumpLock()
    {
        _inputLock = true;
        yield return new WaitForSeconds(_wallJumpLockTime);
        _inputLock = false;
    }
}


