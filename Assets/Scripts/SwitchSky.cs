using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwitchSky : MonoBehaviour
{
    public GameObject day;
    public GameObject night;
    public GameObject particles;

    static bool active = false; // someone is currently touching the
    static bool activeBefore = false;

    private List<GameObject> sceneHands = new List<GameObject>();

    // Update is called once per frame
    private void Update()   
    {
        if ((!activeBefore && active)) {
            Switch();
            activeBefore = true;
        }

        sceneHands = GameObject.FindGameObjectsWithTag("UserHand").Where(h => h.activeSelf == true).ToList();
        if (sceneHands.Count > 0)
        {
            //print($"I See {sceneHands.Count} active hands, particles activated");
            particles.SetActive(true);
        }
        else
        {
            //print("particles deactivated");
            particles.SetActive(false);
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
