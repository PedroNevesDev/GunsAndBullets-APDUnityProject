using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;

    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Jump")]
    public float jumpGraceTime;
    private float jumpGraceTimer;
    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit oldLeft;
    private RaycastHit rightWallhit;
    private RaycastHit oldRight;
    private bool wallLeft;
    private bool wallRight;
    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    bool jump;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if(pm.freeze||pm.activeGrapple)
         {
            StopWallRun();return;
         }
        CheckForWall();
        StateMachine();
    }

    void FixedUpdate()
    {
        if(pm.wallrunning)
            WallRunningMovement();

        if(jump)
            WallJump();
    }

    private void CheckForWall()
    {
        if(leftWallhit.collider != null)
            oldLeft = leftWallhit;
        if(rightWallhit.collider != null)
            oldRight = rightWallhit; 

        
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        if((wallLeft || wallRight)&& verticalInput>0&& AboveGround()&&!exitingWall)
        {
            if(!pm.wallrunning)
                StartWallRun();

            if(wallRunTimer > 0)
                wallRunTimer-=Time.deltaTime;
            if(wallRunTimer<=0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
                print("TimeOut");
            }

            jumpGraceTimer = jumpGraceTime;
        }
        else if(exitingWall)
        {
            if(pm.wallrunning)
                StopWallRun();
            if(exitWallTimer>0)
                exitWallTimer-=Time.deltaTime;
            if(exitWallTimer<=0)
                exitingWall = false;
            if(wallLeft || wallRight) {exitWallTimer = exitWallTime;}
            
            if(jumpGraceTimer>0)
                jumpGraceTimer-=Time.deltaTime;
        }
        else
        {
            if(pm.wallrunning)
                StopWallRun();
        }
        if(Input.GetKeyDown(jumpKey)&&jumpGraceTimer>0)jump = true;

    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
        wallRunTimer = maxWallRunTime;
        rb.velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((orientation.forward - wallForward).magnitude>(orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if(upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed,rb.velocity.z);
        if(downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed/2,rb.velocity.z);

        if(!(wallLeft && horizontalInput>0) && !(wallRight && horizontalInput<0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if(useGravity)
        {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        rb.useGravity = true;
        pm.wallrunning = false;
    }

    private void WallJump()
    {
        jump = false;
        exitingWall = true;
        exitWallTimer = exitWallTime;
        jumpGraceTimer = 0;
        exitWallTimer=0;
        Vector3 wallNormal;

        if(wallRight || wallLeft)
            wallNormal = wallRight? rightWallhit.normal : leftWallhit.normal;
        else
            wallNormal = oldLeft.collider!=null? oldLeft.normal : oldRight.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);

        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
