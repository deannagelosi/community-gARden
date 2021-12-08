using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveProjector : MonoBehaviour
{
    public GameObject proxyLeft;

    // Start is called before the first frame update
    void Start()
    {
        // Auto move projector from RAT calibration position to the user adjusted position
        transform.position = proxyLeft.transform.position;

        // transform.rotation.x = proxyLeft.transform.rotation.x;
        // transform.rotation.y = proxyLeft.transform.rotation.y;
        // transform.rotation.z = proxyLeft.transform.rotation.z;

        Vector3 newRotation = proxyLeft.transform.eulerAngles;
        transform.eulerAngles = newRotation;
    }
}
