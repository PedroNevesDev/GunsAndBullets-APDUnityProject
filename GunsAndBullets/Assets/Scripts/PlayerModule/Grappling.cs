using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;
    [Header("References")]
    public LineRenderer lr;
    public Transform lineOringin, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;
    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;
    public GrappleUI grappleCanvas;
    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCooldown;
    private float grapplingCooldownTimer;

    private bool grappling;

    private Swinging swing;

    ThirdPersonCam myThirdPersonCamController;

    public Transform invisisbleGameObject;

    // Start is called before the first frame update
    void Start()
    {
        pm = GetComponent<PlayerMovement>();
        swing = GetComponent<Swinging>();
        myThirdPersonCamController = Camera.main.GetComponent<ThirdPersonCam>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckForGrappleable();
        UpdateGrappleUI();
        if(Input.GetKeyDown(grappleKey)) StartGrapple();
        if(Input.GetKeyUp(grappleKey)) StopGrapple();

        if(grapplingCooldownTimer>0) grapplingCooldownTimer -= Time.deltaTime; 
    }

    void LateUpdate()
    {
        UpdateLineRender();
    }
    void StartGrapple()
    {
        if(grappling) return;
        if(grapplingCooldownTimer > 0) return;
        if(grapplePoint== Vector3.zero) return;

        swing?.StopSwing();

        
        grappling = true;

        pm.freeze = true;
        Invoke(nameof(ExecuteGrappel), grappleDelayTime);
    }
    void UpdateLineRender()
    {
        if(grappling)
        {
            lr.positionCount = 2;
            lr.SetPosition(0,player.position);
            lr.SetPosition(1,grapplePoint);
            invisisbleGameObject.transform.position = grapplePoint;
        }
    }
    void UpdateGrappleUI()
    {
        if(grapplePoint==Vector3.zero)
        {
            grappleCanvas.gameObject.SetActive(false);
        }
        else
        {
            grappleCanvas.transform.position = grapplePoint;
            grappleCanvas.gameObject.SetActive(true);
        }
    }


    private void CheckForGrappleable()
    {
        if(pm.swinging)return;
        // reseting swingPoint per detection
        grapplePoint = Vector3.zero;

        // camera's frustum planes
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // sphere cast to detect grappleable objects
        RaycastHit[] hits = Physics.SphereCastAll(cam.position, 10f, cam.forward, maxGrappleDistance, whatIsGrappleable);

        foreach (var hit in hits)
        {
            if (GeometryUtility.TestPlanesAABB(frustumPlanes, hit.collider.bounds))
            {
                grapplePoint = hit.point;
                break;
            }
        }
    }
    void ExecuteGrappel()
    {
        pm.freeze = false;  


        myThirdPersonCamController.SwitchCameraStyle(ThirdPersonCam.CameraStyle.Swinging);

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if(grapplePointRelativeYPos<0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOnArc);
        Invoke(nameof(StopGrapple),1f);
    }

    public void StopGrapple()
    {

        myThirdPersonCamController.SwitchCameraStyle(ThirdPersonCam.CameraStyle.Basic);
        pm.freeze = false;    

        grappling = false;

        grapplingCooldownTimer = grapplingCooldown;

        lr.positionCount = 0;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(cam.position, 10f);
        Gizmos.DrawWireSphere(cam.position + cam.forward * maxGrappleDistance,10f);
    }
}
