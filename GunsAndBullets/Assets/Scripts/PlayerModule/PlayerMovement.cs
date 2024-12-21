using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    private float moveSpeed;

    public float walkSpeed;
    public float sprintSpeed;
    public float wallRunSpeed;
    public float swingSpeed;
    public float groundDrag;
    
    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;
    
    [Header("Crouching")]
    public float crouchSpeed;

    public float crouchYScale;
    private float startYScale;
    
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    public Transform orientation;
    
    float horizontalInput;
    float verticalInput;

    private Vector3 moveDirection;
    
    Rigidbody rb;
    
    public MovementState state;

    public enum MovementState 
    { 
        walking, 
        sprinting, 
        swinging,
        wallrunning,
        crouching,
        freeze, 
        air 
    };

    public bool wallrunning;
    public bool swinging;
    public bool freeze;
    public bool activeGrapple;
    LayerMask wallRunningLayer;

    float startingMass;

    private Vector3 velocityToSet;
    private bool enableMovementOnNextTouch;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        readyToJump = true;
        
        startYScale = transform.localScale.y;

        startingMass = rb.mass;
    }
    void Update()
    {
        grounded = Physics.Raycast(transform.position,Vector3.down, playerHeight * 0.5f+0.2f, whatIsGround);
        
        MyInput();
        SpeedControl();
        StateMachine();
        
        if(grounded && !activeGrapple)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

    }

    void FixedUpdate()
    {
        MovePlayer();
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x  , crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x  , startYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
    }

    private void StateMachine()
    {
        if(freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.velocity = Vector3.zero;
        }
        else if(wallrunning)
        {
            state = MovementState.wallrunning;
            moveSpeed = wallRunSpeed;
        }
        else if(swinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
        }

        else if (Input.GetKeyDown(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }

        //After doing the on slope thing change rb.useGravity = !OnSlope add condition (!wallrunning)
    }
    private void MovePlayer()
    {
        if(activeGrapple) return;
        if(swinging)return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        if(activeGrapple) return;

        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;


        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);

        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestriction), 3f);
    }
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }
    Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementeXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * trajectoryHeight * gravity);
        Vector3 velocityXZ = displacementeXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    void OnCollisionEnter(Collision other) 
    {
        if(enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestriction();
            
            GetComponent<Grappling>().StopGrapple();
        }
    }

    void ResetRestriction()
    {
        activeGrapple = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down*playerHeight * (0.5f + 0.2f)));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down*playerHeight * (0.5f + 0.2f)));
    } 
}
