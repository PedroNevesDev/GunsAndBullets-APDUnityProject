using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;
    [Header("References")]
    public LineRenderer lr;
    public Transform lineOringin, cam, player;
    public LayerMask whatIsGrappleable;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint = Vector3.zero;
    private SpringJoint joint;

    public GrappleUI grappleCanvas;

    bool swinging;
    

    void Update()
    {
        CheckForGrappleable();
        UpdateGrappleUI();
        if(Input.GetKeyDown(swingKey))StartSwing();
        if(Input.GetKeyUp(swingKey))StopSwing();
        UpdateLineRenderer();
    }
    private void CheckForGrappleable()
    {
        if(swinging)return;
        //Reseting swingPoint per detection
        swingPoint = Vector3.zero;

        // Get the camera's frustum planes
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // Perform a sphere cast to detect nearby grappleable objects
        RaycastHit[] hits = Physics.SphereCastAll(cam.position, 5f, cam.forward, maxSwingDistance, whatIsGrappleable);

        foreach (var hit in hits)
        {
            // Check if the object is within the camera's frustum
            if (GeometryUtility.TestPlanesAABB(frustumPlanes, hit.collider.bounds))
            {
                print("Found " + hit.collider.name);
                swingPoint = hit.point;
                break; // Start swinging with the first valid hit
            }
        }
    }
    void UpdateLineRenderer()
    {
        if(swinging)
        {
            lr.SetPosition(0,player.position);
            lr.SetPosition(1,swingPoint);
        }
    }
    void UpdateGrappleUI()
    {
        if(swingPoint==Vector3.zero)
        {
            grappleCanvas.gameObject.SetActive(false);
        }
        else
        {
            grappleCanvas.transform.position = swingPoint;
            grappleCanvas.gameObject.SetActive(true);
        }
    }

    void StartSwing()
    {
        if(swingPoint==Vector3.zero) return;

        swinging = true;
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
        swinging = false;
        lr.positionCount = 0;
        if (joint != null) Destroy(joint);
    }
}
