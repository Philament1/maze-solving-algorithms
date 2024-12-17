using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //Constants
    const float SPEED_MULTIPLIER = 1.3f;
    const float ZOOM_SPEED = 1.5f;
    const float MIN_CAM_SIZE = 2f;
    const float MAX_CAM_SIZE = 24f;

    //Main camera
    Camera cam;
    //Max camera limits
    float maxX, maxY;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        //Camera limits set
        maxX = MAX_CAM_SIZE / 2f;
        maxY = MAX_CAM_SIZE / 2f;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement input from arrow keys and WASD
        Vector3 movementInput = new Vector3(Input.GetAxisRaw("Camera X"), 0f, -Input.GetAxisRaw("Camera Y"));
        //New position of camera, updated every frame
        Vector3 newPos = transform.position + (Vector3)movementInput * Time.deltaTime * SPEED_MULTIPLIER * cam.orthographicSize;
        newPos.x = Mathf.Clamp(newPos.x, -maxX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, -maxY, maxY);
        newPos.y = transform.position.y;
        transform.position = newPos;
        //New size of camera, updated every frame based off input from mouse scroll wheel
        float newCamSize = cam.orthographicSize - Input.GetAxisRaw("Camera Zoom") * ZOOM_SPEED;
        cam.orthographicSize = Mathf.Clamp(newCamSize, MIN_CAM_SIZE, MAX_CAM_SIZE);
    }

}
