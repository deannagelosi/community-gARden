using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIFeedback : MonoBehaviour
{
    public GameObject baseUIObject;
    private List<GameObject> sceneHands = new List<GameObject>();
    private List<GameObject> inactiveUIObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        sceneHands = GameObject.FindGameObjectsWithTag("UserHand").Where(h => h.activeSelf == true).ToList();

        if (sceneHands.Count > 0)
        {
            print($"I See {sceneHands.Count} active hands");
            // show ui element at correct x and y, 0 z
            // change color as user gets closer to the correct z range (hot/cold)
            foreach (GameObject hand in sceneHands)
            {
                // getUIObject
                // to do: dont get if already got. carry over from previous frame
            }



        }

    }

    // Reuse existing disabled UI elements
    private GameObject getUIObject()
    {
        // Recycle old inactive hand if available, or make a new one
        GameObject UIObject;

        if (inactiveUIObjects.Count > 0)
        {
            UIObject = inactiveUIObjects[0];
            inactiveUIObjects.RemoveAt(0);
        }
        else
        {
            UIObject = Instantiate(baseUIObject, new Vector3(0, 0, 0), Quaternion.identity);
            UIObject.transform.parent = gameObject.transform.parent.transform;
        }
        UIObject.SetActive(true);
        return UIObject;
    }

    private void discardUIObject(GameObject uiObj)
    {
        // Deactivate and add to inactive list
        if (uiObj != null)
        {
            uiObj.SetActive(false);
            inactiveUIObjects.Add(uiObj);
        }
    }
}
