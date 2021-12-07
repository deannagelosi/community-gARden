using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    public GameObject rain;
    public GameObject ground;
    static bool raining = false;
    static float rainStopTime = 0.0f;
    static bool groundWet = false;

    // Update is called once per frame
    void Update()
    {
        if (!Rain.raining) {
            // ground is dry again 5 seconds after rain stops falling
            if (Time.time - Rain.rainStopTime >= 5) {
                Rain.groundWet = false;
            }
        }
        if (Rain.groundWet) {
            ground.GetComponent<SpriteRenderer>().color = new Color32(120, 120, 120, 255);
        } else {
            ground.GetComponent<SpriteRenderer>().color = new Color32(191, 191, 191, 255);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + gameObject.name);
        Debug.Log("Raining " + Rain.raining);
        if (Rain.raining) {
            Rain.raining = false;
            rain.SetActive(false);
            Rain.rainStopTime = Time.time;
        } else {
            Rain.raining = true;
            rain.SetActive(true);
            Rain.groundWet = true;
        }
    }
}
