using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSky : MonoBehaviour
{
    public GameObject day;
    public GameObject night;

    static bool active = false; // someone is currently touching the
    static bool activeBefore = false;
   
    // Update is called once per frame
    private void Update()   
    {
        if ((!activeBefore && active)) {
            Switch();
            activeBefore = true;
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
        if (other.gameObject.CompareTag("UserHand")) {
            active = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("UserHand")) {
            active = false;
            activeBefore = false;
        }
    }
}
