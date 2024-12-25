using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")] public Transform orientation;
    public Rigidbody rb;
    public LayerMask whatIsWall;
    [Header("Climbing")] 
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;
    private Transform lastWall;
    private Vector3 lastWallNormal;



    [Header("Detection")] 
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWalltTimer;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    PlayerMovement pm;

    void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }
    void Update()
    {
        WallCheck();
        StateMachine();

        if(pm.climbing && !exitingWall) ClimbingMovement();
    }

    private void StateMachine()
    {
        if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if(!pm.climbing && climbTimer > 0) StartClimbing();
            if(climbTimer>0) climbTimer -= Time.deltaTime;
            if(climbTimer<0) StopClimbing();
        }
        else if(exitingWall)
        {
            if(pm.climbing) StopClimbing();

            if(exitWalltTimer > 0) exitWalltTimer -= Time.deltaTime;

            if(exitWalltTimer < 0) exitingWall = false;
        }
        else
        {
            if(pm.climbing) StopClimbing();
        }


        if(wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0)
            ClimbJump();
    }
    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward,out frontWallHit,detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != null || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if((wallFront && newWall) || pm.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        pm.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3( rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void StopClimbing()
    {
        pm.climbing = false;
    }
    
    void ClimbJump()
    {
        exitingWall = true;
        exitWalltTimer = exitWallTime;
        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
        print("jump"+climbJumps);
    }
}
