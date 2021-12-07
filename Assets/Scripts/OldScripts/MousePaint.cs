using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePaint : MonoBehaviour
{
    public Transform baseDot;
    public KeyCode mouseLeft;
    public static string toolType;

    //This is Main Camera in the Scene
    public Camera MainCamera;

    // Start is called before the first frame update
    void Start()
    {
        if (Camera.main) {
            MainCamera = Camera.main; // override user set camera if scene has a main camera
        }
        print(MainCamera);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = MainCamera.nearClipPlane;
        Vector3 objPosition = MainCamera.ScreenToWorldPoint(mousePosition);

        if (Input.GetKey(mouseLeft))
        {
            Instantiate(baseDot, objPosition, baseDot.rotation);
        }
    }
}

// Vector3 worldPosition

// void Update()
// {
//     Vector3 mousePos = Input.mousePosition;
//     mousePos.z = Camera.main.nearClipPlane;
//     worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
// }
