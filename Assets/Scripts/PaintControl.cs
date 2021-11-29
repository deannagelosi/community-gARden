using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintControl : MonoBehaviour
{
    public Transform baseDot;
    public KeyCode mouseLeft;
    public static string toolType;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        // mousePosition.z = Camera.main.nearClipPlane;
        mousePosition.z = Camera.main.nearClipPlane;
        Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Debug.Log("Mouse Postition: " + mousePosition);
        Debug.Log("Obj Postition: " + objPosition);

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
