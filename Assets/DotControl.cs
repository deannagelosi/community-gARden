using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotControl : MonoBehaviour
{
    float destroyTime = 5f;
    
    void Update () { 
        Destroy(gameObject, destroyTime); 
    }

}
