using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //serialized fields
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _dashForce;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _slideJumpForce;
    [SerializeField] private float _dashCooldown;
    [SerializeField] private float _wallKickForce;
    [SerializeField] private float _wallJumpLockTime;
     

    // private value variables
    private Vector2 _moveDir;
    private float _inputX;
    private float _crouchSpeedMultiplier = .5f;
    private float _wallJumpAngle =  60f;
    private float _dashMomentum = 0;
    private float _dashMomentumDecayTime = 0.3f;
    private float _decayRate;
    private bool _isDashing = false;
    private bool _isSliding = false;    
    private bool _wallSliding = false;
    private bool _inputLock = false;
    private bool _climbingLedge = false;
    private bool _isCrouching = false;
    private int _jumpCount = 0;
    private float _wallDir;
    private float _wallJumpRad;
    
    // Component variables
    private Rigidbody2D _rb;
    private Collider2D _hitbox;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _hitbox = GetComponent<Collider2D>();
        _decayRate = _dashForce / _dashMomentumDecayTime;
        _wallJumpRad = _wallJumpAngle * Mathf.Deg2Rad;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!_wallSliding && !_isSliding && !_climbingLedge)
        {
            float speed = _isCrouching ? _movementSpeed * _crouchSpeedMultiplier : _movementSpeed;
            _inputX = _inputLock ? 0f : _moveDir.x;
            _rb.linearVelocity = new Vector2(_inputX * speed + _dashMomentum, _rb.linearVelocityY);
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

    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && _jumpCount == 0 && !_wallSliding && !_isSliding)
        {
            _rb.linearVelocityY = 0;
            _rb.AddForce(Vector2.up.normalized * _jumpForce, ForceMode2D.Impulse);
            _jumpCount++;
        }
        else if(context.performed && _wallSliding)
        {
            WallJump();
        }
        else if(context.performed && _isSliding && _jumpCount == 0)
        {
            SlideJump();
        }

    }

    private void SlideJump()
    {
         _rb.linearVelocityY = 0;
         _rb.AddForce(Vector2.up.normalized * Mathf.Sin(_wallJumpRad) * _slideJumpForce , ForceMode2D.Impulse);
         _dashMomentum = Mathf.Sign(_rb.linearVelocityX) * (Mathf.Abs(_rb.linearVelocityX) + Mathf.Cos(_wallJumpRad) * _slideJumpForce);
         _jumpCount++;
         StartCoroutine(WallJumpLock());
         _isSliding = false;
         _isCrouching = false;
         CrouchScaleUp();
    }

    private void WallJump()
    {
         Vector2 dir = new Vector2(_wallDir * Mathf.Cos(_wallJumpRad), Mathf.Sin(_wallJumpRad));

             _rb.linearVelocityY = 0;
             _rb.AddForce( Vector2.up * dir.y * _wallKickForce, ForceMode2D.Impulse);
             _dashMomentum = dir.x * _wallKickForce;
              StartCoroutine(WallJumpLock());
             _wallSliding = false;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && !_isDashing && !_climbingLedge)
        {
            _rb.linearVelocityY = 0;
            _dashMomentum = _moveDir.x * _dashForce;
            StartCoroutine(DashCooldown());
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed  && _jumpCount == 0)
        {
            if(!_isDashing){

              _isCrouching = true;
            }
            else
            {
              _isSliding = true;
            }
            CrouchScaleDown();
        }

        if (context.canceled)
        {
            _isCrouching = false;
            _isSliding = false;
            CrouchScaleUp();
        }
    }

    private void CrouchScaleDown()
    {
          transform.localScale = new Vector3(1, 0.8f, 1);
    }

    private void CrouchScaleUp()
    {
          transform.localScale = new Vector3(1, 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Wall":
                 WallSlide(collision);
            break;
            case "Ground":
                 Groundreset();
            break;
            case "Platform":
                  WallSlide(collision);
            break;
        }

       LedgeGrab(collision);

    }

    private void LedgeGrab(Collision2D collision)   
    {
         if(collision.gameObject.tag != "Ground"){

            ContactPoint2D contact = collision.GetContact(0);
            float ledgeZone = contact.collider.bounds.max.y - 1; 

            if(ledgeZone < _hitbox.bounds.center.y && _hitbox.bounds.min.y !< ledgeZone)
            {
                StartCoroutine(LedgeClimb(contact.collider.bounds.max.y,-contact.normal.x));
                transform.rotation = Quaternion.Euler(0, contact.normal.x > 0 ? 180 : 0, 0);
            }
        }
    }

    private IEnumerator LedgeClimb(float targetTop, float intotTargetDir)
    {
        _inputLock = true;
        _climbingLedge = true;
        _rb.linearVelocity = Vector2.zero;
        _wallSliding = false;
        _dashMomentum = 0;
        _rb.gravityScale = 0;

        Vector2 start = _rb.position;
        float topY = targetTop + _hitbox.bounds.extents.y + 0.05f;
        Vector2 up = new Vector2(start.x, topY);
        Vector2 over = new Vector2(start.x + intotTargetDir * _hitbox.bounds.size.x, topY);

        yield return StartCoroutine(ClimbLedge(up));
        yield return StartCoroutine(ClimbLedge(over));

        _rb.gravityScale = 1;
        _jumpCount = 0;
        _inputLock = false;
        _climbingLedge = false;
        Debug.Log("Ledge climbed");
    }

    private IEnumerator ClimbLedge(Vector2 target)
    {
        while ( Vector2.Distance(_rb.position, target) > 0.01f)
        {
            _rb.MovePosition(Vector2.MoveTowards(_rb.position,target,15f * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Moving towards the top");
        _rb.position = target;
    }

    private void Groundreset()
    {
        _jumpCount = 0;
        _wallSliding = false;
        _isDashing = false;
        _climbingLedge = false;
        _isSliding = false;
    }

    private IEnumerator DashCooldown()
    {
        _isDashing = true;
        yield return new WaitForSeconds(_dashCooldown);
        _isDashing = false;
    }

    private void WallSlide(Collision2D collision)
    {
        _wallDir = collision.GetContact(0).normal.x;

        if(collision.GetContact(0).normal.y < 0.5f && collision.GetContact(0).normal.y > -0.5f){

          _wallSliding = true;
          _rb.linearVelocityY = 0;
          transform.rotation = Quaternion.Euler(0, _wallDir > 0 ? 0 : 180, 0);
        }
        else if(collision.GetContact(0).normal.y > 0.5f )
        {
            Groundreset ();
        }
    }

    private IEnumerator WallJumpLock()
    {
        _inputLock = true;
        yield return new WaitForSeconds(_wallJumpLockTime);
        _inputLock = false;
    }
}


