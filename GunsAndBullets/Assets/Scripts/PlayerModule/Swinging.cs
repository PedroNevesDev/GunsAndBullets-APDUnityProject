using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;
    [Header("References")]
    public LineRenderer lr;
    public Transform lineOringin, cam, player;
    public LayerMask whatIsSwingable;
    public PlayerMovement pm;

    [Header("Swinging")]
    public float maxSwingDistance = 25f;
    private Vector3 swingPoint = Vector3.zero;
    private SpringJoint joint;

    public GrappleUI swingCanvas;

    [Header("AirMovement")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    ThirdPersonCam myThirdPersonCamController;
    public Transform invisisbleGameObject;

    
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        myThirdPersonCamController = Camera.main.GetComponent<ThirdPersonCam>();
    }

    void Update()
    {
        CheckForSwingable();
        UpdateGrappleUI();
        if(Input.GetKeyDown(swingKey))StartSwing();
        if(Input.GetKeyUp(swingKey))StopSwing();

    }

    void FixedUpdate()
    {
        if(joint!=null) AirMovement();
    }

    void LateUpdate()
    {
        UpdateLineRenderer();        
    }
    private void CheckForSwingable()
    {
        if(pm.swinging)return;
        //Reseting swingPoint per detection
        swingPoint = Vector3.zero;

        // Get the camera's frustum planes
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // Perform a sphere cast to detect nearby grappleable objects
        RaycastHit[] hits = Physics.SphereCastAll(cam.position, 15f, cam.forward, maxSwingDistance, whatIsSwingable);

        foreach (var hit in hits)
        {
            // Check if the object is within the camera's frustum
            if (GeometryUtility.TestPlanesAABB(frustumPlanes, hit.collider.bounds))
            {
                swingPoint = hit.point;
                break; // Start swinging with the first valid hit
            }
        }
    }
    void UpdateLineRenderer()
    {
        if(pm.swinging)
        {
            lr.SetPosition(0,player.position);
            lr.SetPosition(1,swingPoint);
            invisisbleGameObject.transform.position = swingPoint;
        }
    }

    void AirMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 dirForce = orientation.right * horizontalInput * horizontalThrustForce + orientation.forward * verticalInput * forwardThrustForce;
        rb.AddForce(dirForce*Time.fixedDeltaTime);

        if(Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.fixedDeltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }

        if(Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }
    void UpdateGrappleUI()
    {
        if(swingPoint==Vector3.zero)
        {
            swingCanvas.gameObject.SetActive(false);
        }
        else
        {
            swingCanvas.transform.position = swingPoint;
            swingCanvas.gameObject.SetActive(true);
        }
    }

    void StartSwing()
    {
        if(swingPoint==Vector3.zero) return;

        myThirdPersonCamController.SwitchCameraStyle(ThirdPersonCam.CameraStyle.Swinging);
        pm.swinging = true;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;
        
        lr.positionCount = 2;
    }

    private void StopSwing()
    {
        myThirdPersonCamController.SwitchCameraStyle(ThirdPersonCam.CameraStyle.Basic);
        pm.swinging = false;
        lr.positionCount = 0;
        if (joint != null) Destroy(joint);
    }
}
