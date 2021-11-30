using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotControl : MonoBehaviour
{
    float destroyTime = 5f;
    static Color startColor = new Color(201f/255f, 255f/255f, 191f/255f, 0.5f);
    static Color endColor = new Color(255f/255f, 175f/255f, 189f/255f, 0.5f);
    // Amount to interpolate between start and end color
    static float colorInterp = 0f;
    // The amount to change interpolation by after each dot is generated
    static float interpStep = 0.05f;
    static float scale = 1f;

    void Start () {
       Debug.Log("Color interpretation: " + DotControl.colorInterp);
       // Get the Renderer component for dot
       Renderer renderer = gameObject.GetComponent<Renderer>();

       // Get color for dot
       Color color = Color.Lerp(
           DotControl.startColor, DotControl.endColor, DotControl.colorInterp);
       
       // Set scale for dot
       float scale = DotControl.scale + UnityEngine.Random.Range(-0.1f, 0.1f);
       scale = Mathf.Clamp(scale, 0.5f, 2f);
       gameObject.transform.localScale = new Vector3(scale, scale, scale);
       DotControl.scale = scale;

       //Call SetColor using the shader property name "_Color"
       renderer.material.SetColor("_Color", color);

       // Update static color variables for next dot generated
       // First, check if end of gradient has been reached, and if so, 
       // switch direction of interpolation
       if((DotControl.colorInterp >= 1 && DotControl.interpStep > 0) 
        || (DotControl.colorInterp <= 0 && DotControl.interpStep < 0)) {
           DotControl.interpStep = DotControl.interpStep * -1;
       }
       DotControl.colorInterp += DotControl.interpStep;
       
       Hashtable args = new Hashtable();
       args.Add("alpha", 0f);
       args.Add("delay", 5f);
       args.Add("time", 5f);
       args.Add("onComplete", "destroy");
       iTween.FadeTo(gameObject, args);
    }
    

}
