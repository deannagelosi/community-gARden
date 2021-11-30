using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPaint : MonoBehaviour
{
    public Transform baseDot;
    public GameObject rightHand;
    public GameObject leftHand;

    private bool colliding;

    private bool rightActive; // is the right hand in the canvas
    private bool leftActive; // is the left hand in the canvas
    private GameObject currentActive; // the hand most recently in the canvas

    // Start is called before the first frame update
    void Start()
    {
        rightActive = false;
        leftActive = false;

        // Rotate brush dot to line up with canvas
        baseDot.rotation = transform.rotation;
        baseDot.transform.Rotate(-90, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (rightActive || leftActive)
        {
            // Note: Z axis will include the depth of the canvas
            Vector3 brushPosition = currentActive.transform.position;
            Instantiate(baseDot, brushPosition, baseDot.rotation);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("RightHand"))
        {
            print("Right: True");
            rightActive = true;
            currentActive = other.gameObject;
        }
        else if (other.gameObject.CompareTag("LeftHand"))
        {
            print("Left: True");
            leftActive = true;
            currentActive = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("RightHand"))
        {
            print("Right: False");
            rightActive = false;
        }

        else if (other.gameObject.CompareTag("LeftHand"))
        {
            print("Left: False");
            leftActive = false;
        }
    }
}
