using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrappleUI : MonoBehaviour
{
    private float targetSize = 0f;
    private float currentSize = 0f;


    [Header("Animation Settings")]
    public float scaleSpeed = 2f;
    public Vector3 rotationAxis = Vector3.up; // Axis of rotation (e.g., up = y-axis)
    public float speed = 10f;                // Rotation speed

    private Quaternion targetRotation;
    public Image image;

    Vector3 startScale;
    // Start is called before the first frame update
    void Start()
    {
        startScale = image.transform.localScale;
    }
    void OnEnable()
    {
        targetSize = 1f;
    }

    void OnDisable() 
    {
        targetSize = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        targetRotation *= Quaternion.Euler(rotationAxis * speed * Time.deltaTime);
                // Rotate smoothly towards the target
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * speed);

        currentSize = Mathf.Lerp(currentSize,targetSize,scaleSpeed*Time.deltaTime);
        image.transform.localScale = startScale * currentSize; 
    }
}
