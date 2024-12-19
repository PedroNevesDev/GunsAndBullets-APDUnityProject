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
    public float speed = 10f;                // Rotation speed

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

        // Smoothly rotate the image towards the target rotation
        image.transform.rotation = Quaternion.Lerp(image.transform.rotation, image.transform.rotation * Quaternion.Euler(image.transform.forward * speed * Time.deltaTime), Time.deltaTime * speed);

        // Smoothly scale the image
        currentSize = Mathf.Lerp(currentSize, targetSize, scaleSpeed * Time.deltaTime);
        image.transform.localScale = startScale * currentSize;
    }
}
