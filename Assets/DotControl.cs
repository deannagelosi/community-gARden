using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotControl : MonoBehaviour
{
    float destroyTime = 5f;
    static Color startColor = Color.blue;
    static Color endColor = Color.magenta;
    // Amount to interpolate between start and end color
    static float colorInterp = 0f;
    // The amount to change interpolation by after each dot is generated
    static float interpStep = 0.05f;

    void Start () {
       Debug.Log("Color interpretation: " + DotControl.colorInterp);
       // Get the Renderer component for dot
       Renderer renderer = gameObject.GetComponent<Renderer>();

       // Get color for dot
       Color color = Color.Lerp(
           DotControl.startColor, DotControl.endColor, DotControl.colorInterp);
       
       // Update static variables for interpolation for next dot generated
       // First, check if end of gradient has been reached, and if so, 
       // switch direction of interpolation
       if((DotControl.colorInterp >= 1 && DotControl.interpStep > 0) 
        || (DotControl.colorInterp <= 0 && DotControl.interpStep < 0)) {
           DotControl.interpStep = DotControl.interpStep * -1;
       }
       DotControl.colorInterp += DotControl.interpStep;

       //Call SetColor using the shader property name "_Color" and setting the color to red
       renderer.material.SetColor("_Color", color);
    }
    
    void Update () { 
        Destroy(gameObject, destroyTime); 
    }

}
