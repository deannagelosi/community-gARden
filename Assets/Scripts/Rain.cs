using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    public ParticleSystem rain;
    public GameObject ground;
    private bool raining = false;
    float rainStopTime = 0.0f;
    public bool groundWet = false;
    // Start is called before the first frame update
    void Start()
    {
        raining = false;
        groundWet = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (raining) {
            rain.Play();
        } else {
            rain.Stop();
            // ground is dry again 5 seconds after rain stops falling
            if (Time.time - rainStopTime >= 5) {
                groundWet = false;
            }
        }
        if (groundWet) {
            ground.GetComponent<SpriteRenderer>().color = new Color32(120, 120, 120, 255);
        } else {
            ground.GetComponent<SpriteRenderer>().color = new Color32(191, 191, 191, 255);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + gameObject.name);
        if (raining) {
            raining = false;
            rainStopTime = Time.time;
        } else {
            raining = true;
            groundWet = true;
        }
    }
}
