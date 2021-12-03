using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSky : MonoBehaviour
{
    public GameObject day;
    public GameObject night;

    private bool rightActive; // is the right hand colliding with sun/moon
    private bool leftActive; // is the left hand colliding with sun/moon
   
    // Start is called before the first frame update
    void Start()
    {
        rightActive = false;
        leftActive = false;
        day = GameObject.Find("Day");
        night = GameObject.Find("Night");
    }

    // Update is called once per frame
    void Update()
    {
        if (rightActive || leftActive)
        {
            if (gameObject.name == "Sun"){
                day.SetActive(false);
                night.SetActive(true);
            } else if (gameObject.name == "Moon"){
                day.SetActive(true);
                night.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided");
        if (other.gameObject.CompareTag("RightHand"))
        {
            print("Right: True");
            rightActive = true;
        }
        else if (other.gameObject.CompareTag("LeftHand"))
        {
            print("Left: True");
            leftActive = true;
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
