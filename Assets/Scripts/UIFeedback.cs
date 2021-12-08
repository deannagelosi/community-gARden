using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIFeedback : MonoBehaviour
{
    public GameObject baseUIObject;
    public int maxCameraDistance = 1; // used to control ui dot color temp
    public float maxStepDistance = 0.02f; // stops the ui element from shaking as it moves

    private List<GameObject> sceneHands = new List<GameObject>();
    private List<GameObject> activeUIObjs = new List<GameObject>();
    private List<GameObject> inactiveUIObjs = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        sceneHands = GameObject.FindGameObjectsWithTag("UserHand").Where(h => h.activeSelf == true).ToList();

        if (sceneHands.Count > 0)
        {
            foreach (GameObject hand in sceneHands)
            {
                string trackingID = hand.GetComponent<ObjState>().trackingID;
                Vector3 wallPos = new Vector3(hand.transform.position.x, hand.transform.position.y, 0.0f);

                // Search active UI dots. Update or setup dot for each hand in scene
                GameObject uiWallOBj = activeUIObjs.Find(o => trackingID == o.GetComponent<ObjState>().trackingID);

                if (uiWallOBj != null)
                {
                    // There is an active ui dot
                    float dist = Vector3.Distance(uiWallOBj.transform.position, wallPos);
                    if (dist > maxStepDistance) // Don't move if "close enough" already
                    {
                        // Use MoveToward for a smooth position update
                        Vector3 currentPosition = uiWallOBj.transform.position;
                        uiWallOBj.transform.position = Vector3.MoveTowards(currentPosition, wallPos, maxStepDistance);
                    }
                }
                else if (uiWallOBj == null)
                {
                    uiWallOBj = getUIObject(trackingID);
                    uiWallOBj.transform.position = wallPos;
                }

                // Change color based on distance to wall
                Color colorStart = Color.red;
                Color colorAlpha = Color.white;
                float distWall = Vector3.Distance(uiWallOBj.transform.position, hand.transform.position);
                uiWallOBj.GetComponent<Renderer>().material.color = Color.Lerp(colorStart, colorAlpha, distWall / maxCameraDistance);
            }
        }

        List<GameObject> discardThese = new List<GameObject>();
        foreach (GameObject uiObj in activeUIObjs)
        {
            // Is there a matching scene hand?
            string objID = uiObj.GetComponent<ObjState>().trackingID;

            GameObject foundHand = sceneHands.Find(h => h.GetComponent<ObjState>().trackingID == objID);
            if (foundHand == null)
            {
                discardThese.Add(uiObj);
            }
        }
        discardThese.ForEach(o => discardUIObject(o));
    }

    // Reuse existing disabled UI elements
    private GameObject getUIObject(string trackingID)
    {
        // Recycle old inactive hand if available, or make a new one
        GameObject uiObject;

        if (inactiveUIObjs.Count > 0)
        {
            uiObject = inactiveUIObjs[0];
            inactiveUIObjs.RemoveAt(0);
        }
        else
        {
            uiObject = Instantiate(baseUIObject, new Vector3(0, 0, 0), Quaternion.identity);
            uiObject.transform.parent = gameObject.transform.parent.transform;
        }

        uiObject.GetComponent<ObjState>().trackingID = null;
        uiObject.GetComponent<ObjState>().trackingID = trackingID;
        uiObject.SetActive(true);
        activeUIObjs.Add(uiObject);
        return uiObject;
    }

    private void discardUIObject(GameObject uiObj)
    {
        // Deactivate and add to inactive list
        if (uiObj != null)
        {
            uiObj.SetActive(false);
            inactiveUIObjs.Add(uiObj);
            activeUIObjs.Remove(uiObj);
        }
    }
}
