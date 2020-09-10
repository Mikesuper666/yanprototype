using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraY : MonoBehaviour
{
    private Transform targetLookAt;
    public float mouseSensitive = 100f;
    public Transform playerBoy;
    float xRotation = 0f;
    private float mouseY;
    private float mouseX;

    void Awake()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitive * Time.deltaTime;
        mouseX = Input.GetAxis("Mouse X") * mouseSensitive * Time.deltaTime;

        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -20f, 20f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBoy.Rotate(Vector3.up * mouseX);
    }

    public void Init()
    {
       
        if (!targetLookAt) targetLookAt = new GameObject("targetLookAt").transform;
        targetLookAt.rotation = transform.rotation;
        // targetLookAt.hideFlags = HideFlags.HideInHierarchy;
        

    }
}
