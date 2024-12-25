using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerMesh;

    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;
    
    [Header("Input")]

    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private bool sliding;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerMesh.localScale.y;
    }

    private void StartSlide()
    {
        pm.sliding = true;

        playerMesh.localScale = new Vector3(playerMesh.localScale.x,slideYScale, playerMesh.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerMesh.localScale = new Vector3(playerMesh.localScale.x,startYScale, playerMesh.localScale.z);
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection  = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if(!pm.OnSlope() || rb.velocity.y >-0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce,ForceMode.Force);

            slideTimer -= Time.fixedDeltaTime;            
        }
        // apply force in the slope direction for better sliding downwards, also not counting the time makes it for that you can slide down a slope as long as you want
        else
        {
            rb.AddForce(pm.GetSlopeMovementDirection(inputDirection) * slideForce,ForceMode.Force);
        }

        if(slideTimer <= 0) 
            StopSlide();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput !=0) && pm.grounded) StartSlide();
        if(Input.GetKeyUp(slideKey) && pm.sliding) StopSlide();
    }

    void FixedUpdate()
    {
        if(pm.sliding)
            SlidingMovement();
    }
}
