using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce;
    private InputAction move;
    private Vector2 moveDir;

    private PlayerInput PlayerinputRef;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PlayerinputRef = GetComponent<PlayerInput>();
       
    }

    private void OnEnable()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Moving(InputAction.CallbackContext context)
    {
            moveDir = context.ReadValue<Vector2>();
            Debug.Log("moved");   
    }
}
