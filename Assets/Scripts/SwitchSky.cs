using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSky : MonoBehaviour
{
    public GameObject day;
    public GameObject night;

    private bool rightActive = false; // is the right hand colliding with sun/moon
    private bool leftActive = false; // is the left hand colliding with sun/moon
    private bool rightActiveBefore = false;
    private bool leftActiveBefore = false;
   
    // Start is called before the first frame update
    void Start()
    {
        rightActiveBefore = false;
        leftActiveBefore = false;
        rightActive = false;
        leftActive = false;
    }

    // Update is called once per frame
    private void Update()   
    {
        if ((!rightActiveBefore && rightActive)) {
            Switch();
            rightActiveBefore = true;
        } else if ((!leftActiveBefore && leftActive)) {
            Switch();
            leftActiveBefore = true;
        }
    }

    private void Switch()
    {
        if (gameObject.name == "Sun"){
            day.SetActive(false);
            night.SetActive(true);
        } else if (gameObject.name == "Moon"){
            day.SetActive(true);
            night.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + gameObject.name);
        if (other.gameObject.CompareTag("RightHand")) {
            rightActive = true;
        } else if (other.gameObject.CompareTag("LeftHand")) {
            leftActive = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("RightHand")) {
            rightActive = false;
            rightActiveBefore = false;
        } else if (other.gameObject.CompareTag("LeftHand")) {
            leftActive = false;
            leftActiveBefore = false;
        }
    }
}
