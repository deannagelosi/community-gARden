using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoomAliveToolkit
{

    // Class object for collecting all the tracked hand data
    public class TrackedHand
    {
        // public RATSkeletonProvider skelProvider { get; set; }
        // public int skelID { get; set; }
        public List<string> handIDs { get; set; }
        public Vector3 position { get; set; }
        public bool frameMatch;
        public GameObject handObject;
    }

    // Main HandControl program
    public class HandControl : MonoBehaviour
    {
        public GameObject baseHandObj;
        public float mergeDistance = 0.15f; // when hand objects are this close together, merge them
        public List<RATSkeletonProvider> skeletonProviders;
        public int maxBodies = 1; // Track 1 skeleton by default (Kinect can track up to 6)
        public bool skipConfidenceCheck = true; // skip checking if hand confidence is 1 (high)

        // Collect inactive hands for re-use (instead of instantiating more and more)
        private List<GameObject> inactiveHands = new List<GameObject>();
        private List<TrackedHand> prevFrameHands = new List<TrackedHand>();
        private float objDiameter;

        // Start is called before the first frame update
        void Start()
        {
            if (maxBodies > 6 || maxBodies < 1)
            {
                Debug.LogError("maxBodies must be between 1 and 6");
                return;
            }

            if (skeletonProviders.Count <= 0)
            {
                Debug.LogError("Missing a skeleton provider");
                return;
            }
        }

        // Update is called once per frame
        void Update()
        {
            List<TrackedHand> currFrameHands = new List<TrackedHand>();

            // Gather all the detected hands accross all providers and skeleton ids
            gatherAllHands(skeletonProviders, currFrameHands);

            // Current Task: Add code to de-dupe hands list, merging hands in proximity
            // mergeProximityMatches(currFrameHands);

            // Clear old match data and check for new matches
            resetAndFindMatch(currFrameHands, prevFrameHands);
            // To Do: ^ make sure passed in vars modifications are being seen after

            // Manage CURRENT hand game objects
            foreach (TrackedHand currHand in currFrameHands)
            {
                if (currHand.frameMatch)
                {
                    // Not New. Retrieved this hand from the previous frame list
                    currHand.handObject.transform.position = currHand.position;
                }
                else if (!currHand.frameMatch)
                {
                    // New. This hand was not seen in the previous frame
                    currHand.handObject = getHandObject();
                    currHand.handObject.transform.position = currHand.position;
                }
            }

            // Manage PREVIOUS hand game objects
            foreach (TrackedHand prevHand in prevFrameHands)
            {
                // Previous hand was not found in the current frame
                if (!prevHand.frameMatch) { discardHand(prevHand.handObject); }
            }

            // Save current matches for the next frame
            // prevFrameHands = new List<TrackedHand>();
            prevFrameHands = currFrameHands;
        }

        private void gatherAllHands(List<RATSkeletonProvider> skeletonProviders, List<TrackedHand> currFrameHands)
        {
            if (skeletonProviders.Count > 0)
            {
                int providerID = 0;
                foreach (RATSkeletonProvider skelProvider in skeletonProviders)
                {
                    for (int skelID = 0; skelID < maxBodies; skelID++) // Loops 1 thru maxBodies
                    {
                        RATKinectSkeleton skel = skelProvider.GetKinectSkeleton(skelID);
                        if (skel != null && skel.valid)
                        {
                            string providerSkelID = $"SP{providerID}-SK{skelID}";
                            // print($"found a skeleton: {providerSkelID}");

                            List<TrackedHand> skelHands = getHandPositions(skelProvider, providerSkelID, skel);
                            if (skelHands != null)
                            {
                                currFrameHands.AddRange(skelHands);
                            }
                        }
                    }
                    providerID++;
                }
            }
        }

        private void mergeProximityMatches(List<TrackedHand> handsList)
        {
            // Loop each hand and gather any hands that are in proximity (includes itself)
            List<TrackedHand> mergedHands = new List<TrackedHand>();

            if (handsList.Count > 1)
            {
                print(handsList.Count);
                print($"{handsList.Count} Distance: {Vector3.Distance(handsList[0].position, handsList[1].position)}");

                // to do: here in the code: distance works, but not de-dupping. check distint code?
            }

            foreach (TrackedHand hand in handsList)
            {
                // Get position of current hand in loop
                Vector3 handPos = hand.position;
                // Find all other hands in proximity (including itself)
                List<TrackedHand> handsNear = handsList.FindAll(h => Vector3.Distance(handPos, h.position) <= objDiameter);

                // Merge all the found hands together into 1 hand object
                // 1a. Get the first hand object
                GameObject oneHandObject = handsNear[0].handObject;
                // 1b. Deactivate the rest if more than one
                if (handsNear.Count > 1)
                {
                    for (int i = 1; i < handsNear.Count; i++)
                    {
                        if (handsNear[i].handObject != null)
                        {
                            discardHand(handsNear[i].handObject);
                        }
                    }
                }

                // 2. List of all the handIDs
                List<string> allHandIDs = handsNear.Select(h => h.handIDs[0]).ToList();
                allHandIDs.Sort();

                // 3. Average all the positions
                Vector3 sumPos = Vector3.zero;
                foreach (TrackedHand nearHand in handsNear)
                {
                    sumPos += nearHand.position.normalized;
                }
                sumPos = sumPos.normalized;
                Vector3 averagePos = new Vector3(sumPos.x / handsNear.Count, sumPos.y / handsNear.Count, sumPos.z / handsNear.Count);
                // to do: test this is averaging ^

                TrackedHand mergedHand = new TrackedHand()
                {
                    handIDs = allHandIDs,
                    position = averagePos,
                    handObject = oneHandObject,
                    frameMatch = false
                };
                mergedHands.Add(mergedHand);
            }

            // to do: test this is de-duping 
            handsList = mergedHands.Distinct().ToList();
        }

        private List<TrackedHand> getHandPositions(RATSkeletonProvider skelProvider, string providerSkelID, RATKinectSkeleton skel)
        {
            List<TrackedHand> hands = new List<TrackedHand>();

            if (skel != null && skel.valid)
            {
                if (skel.handRightConfidence == 1 || skipConfidenceCheck)  // returns 0 or 1, not a range
                {
                    Vector3 providerPosition = skel.jointPositions3D[(int)JointType.HandRight]; // returns skeleton provider coordinate system
                    string handID = $"{providerSkelID}-Right";
                    addValidHand(hands, handID, providerPosition, skelProvider);
                }

                if (skel.handLeftConfidence == 1 || skipConfidenceCheck)   // returns 0 or 1, not a range
                {
                    Vector3 providerPosition = skel.jointPositions3D[(int)JointType.HandLeft]; // in skeleton provider coordinate system
                    string handID = $"{providerSkelID}-Left";
                    addValidHand(hands, handID, providerPosition, skelProvider);
                }
            }
            return hands;
        }

        private void addValidHand(List<TrackedHand> hands, string handID, Vector3 providerPosition, RATSkeletonProvider skelProvider)
        {
            Vector3 worldPos = skelProvider.transform.localToWorldMatrix.MultiplyPoint(providerPosition); // move to world coordinates

            if (worldPos != null)
            {
                hands.Add(new TrackedHand()
                {
                    handIDs = new List<string> { handID },
                    position = worldPos
                });
            }
        }

        private void resetAndFindMatch(List<TrackedHand> currentHands, List<TrackedHand> prevHands)
        {
            // Set them all to false first, clearing any old match data from previous frames
            foreach (TrackedHand currHand in currentHands) { currHand.frameMatch = false; }
            foreach (TrackedHand prevHand in prevHands) { prevHand.frameMatch = false; }

            // Find matches
            foreach (TrackedHand currHand in currentHands)
            {
                foreach (TrackedHand prevHand in prevHands)
                {
                    // handID matches same provider, skel id, and hand (L or R)
                    currHand.handIDs.Sort();
                    prevHand.handIDs.Sort();

                    // if (currHand.handIDs[0] == prevHand.handIDs[0])
                    if (Enumerable.SequenceEqual(currHand.handIDs, prevHand.handIDs))
                    {
                        currHand.frameMatch = true;
                        prevHand.frameMatch = true;
                        currHand.handObject = prevHand.handObject;
                    }
                    else
                    {
                        // print($"no match. Curr HandID: {currHand.handIDs[0]} Prev HandID: {prevHand.handIDs[0]}");
                    }
                }
            }
        }

        private GameObject getHandObject()
        {
            // Recycle old inactive hand if available, or make a new one
            GameObject handObject;

            if (inactiveHands.Count > 0)
            {
                // print("use existing hand");
                handObject = inactiveHands[0];
                inactiveHands.RemoveAt(0);
            }
            else
            {
                // print("instantiate");
                handObject = Instantiate(baseHandObj, new Vector3(0, 0, 0), Quaternion.identity);
            }
            handObject.SetActive(true);
            return handObject;
        }

        private void discardHand(GameObject obj)
        {
            // Deactivate and add to inactive list
            // print("deactivate hand");
            obj.SetActive(false);
            inactiveHands.Add(obj);
        }

    }
}