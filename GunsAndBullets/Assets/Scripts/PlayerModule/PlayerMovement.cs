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
        air 
    };

    public bool wallrunning;
    public bool swinging;
    LayerMask wallRunningLayer;

    float startingMass;
    
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
        StateHandler();
        
        if(grounded)
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

    private void StateHandler()
    {
        if(wallrunning)
        {
            state = MovementState.wallrunning;
            moveSpeed = wallRunSpeed;
        }
        else if(swinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
        }
        if (Input.GetKeyDown(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        
        //up here
        //here
        //here
        //here
        //here
        
        
        
        
        
        
        if (grounded && Input.GetKey(sprintKey))
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
        if(swinging)return;
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down*playerHeight * (0.5f + 0.2f)));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down*playerHeight * (0.5f + 0.2f)));
    } 
}
