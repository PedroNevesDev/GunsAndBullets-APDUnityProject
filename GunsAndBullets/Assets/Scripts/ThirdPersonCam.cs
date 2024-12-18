using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public Transform combatLookAt;

    public CameraStyle currentStyle;

    [Header("Cinamachine Objects")]
    public GameObject thirdPersonCam;
    public GameObject combatCam;
    public GameObject topdawnCam;


    public enum CameraStyle
    {
        Basic,
        Combat,
        Topdown
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        InputHandler();

        switch(currentStyle)
        {
            case CameraStyle.Basic:BasicCamera();
            break;
            
            case CameraStyle.Combat:CombatCamera();
            break;

            case CameraStyle.Topdown:TopdownCamera();
            break;

        }
    }

    void InputHandler()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))SwitchCameraStyle(CameraStyle.Basic);
        if(Input.GetKeyDown(KeyCode.Alpha2))SwitchCameraStyle(CameraStyle.Combat);
        if(Input.GetKeyDown(KeyCode.Alpha3))SwitchCameraStyle(CameraStyle.Topdown);
    }

    void BasicCamera()
    {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if(inputDir != Vector3.zero)
                playerObj.forward = Vector3.Slerp(playerObj.forward,inputDir.normalized, Time.deltaTime * rotationSpeed);
    }
    void CombatCamera()
    {
            Vector3 dirToCombatLookAt = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
            orientation.forward = dirToCombatLookAt.normalized;

            playerObj.forward = dirToCombatLookAt.normalized;
    }

    void TopdownCamera()
    {
        BasicCamera();
    }

    void SwitchCameraStyle( CameraStyle newStyle)
    {
        thirdPersonCam.SetActive(false);
        combatCam.SetActive(false);
        topdawnCam.SetActive(false);

        switch(newStyle)
        {
            case CameraStyle.Basic:thirdPersonCam.SetActive(true);
            break;
            case CameraStyle.Combat:combatCam.SetActive(true);
            break;
            case CameraStyle.Topdown:topdawnCam.SetActive(true);
            break;
        }

        currentStyle = newStyle;
    }
}
