using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] 
    private float moveSpeed;

    public float walkSpeed;
    public float sprintSpeed;
    public float wallRunSpeed;
    public float swingSpeed;

    public float slidingSpeed;
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

    [Header("Slope Handling")]

    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    private bool exitingSlope;

    
    float horizontalInput;
    float verticalInput;

    private Vector3 moveDirection;
    
    Rigidbody rb;

    [Header("Debugging")]
    
    public MovementState state;

    public enum MovementState 
    { 
        walking, 
        sprinting, 
        swinging,
        wallrunning,
        crouching,
        freeze, 
        sliding,
        air 
    };

    public bool wallrunning;
    public bool swinging;
    public bool freeze;
    public bool activeGrapple;
    public bool sliding;
    LayerMask wallRunningLayer;

    float startingMass;

    private Vector3 velocityToSet;
    private bool enableMovementOnNextTouch;

    public TextMeshProUGUI stateText;
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

        stateText.text = state.ToString();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f+ 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle< maxSlopeAngle && angle!=0; 
        }
        return false;
    }

    private Vector3 GetSlopeMovementDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
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
        if(sliding)
        {
            state = MovementState.sliding;
            moveSpeed = slidingSpeed;
        }
        else if(freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.velocity = Vector3.zero;
            wallrunning = false;
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
        else if(!activeGrapple)
        {
            state = MovementState.air;
        }

        if(!wallrunning)
            rb.useGravity = !OnSlope();
    }
    private void MovePlayer()
    {
        if(activeGrapple) return;
        if(swinging)return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // if player iss standing on a slope
        if(OnSlope())
        {
            rb.AddForce(GetSlopeMovementDirection() * moveSpeed * 20f, ForceMode.Force);

            if(rb.velocity.y>0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        if(activeGrapple) return;

        // onslope speed control
        if(OnSlope() && !exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitVel.x, rb.velocity.y, limitVel.z);
            }            
        }

    }

    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
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

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 *  gravity * trajectoryHeight );
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
