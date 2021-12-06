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
        public List<string> handID { get; set; }
        public Vector3 position { get; set; }
        public bool frameMatch;
        public GameObject handObject;
    }

    // Main HandControl program
    public class HandControl : MonoBehaviour
    {
        public GameObject baseHandObj;
        public List<RATSkeletonProvider> skeletonProviders;
        public int maxBodies = 1; // Track 1 skeleton by default (Kinect can track up to 6)

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

            objDiameter = baseHandObj.GetComponent<SphereCollider>().radius * 2;
        }

        // Update is called once per frame
        void Update()
        {
            // Gather all the detected hands accross all providers and skeleton ids
            List<TrackedHand> currFrameHands = gatherAllHands(skeletonProviders);

            // Current Task: Add code to de-dupe hands list, merging hands in proximity
            mergeProximityMatches(currFrameHands);

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
            prevFrameHands = new List<TrackedHand>();
            prevFrameHands = currFrameHands;
        }

        private List<TrackedHand> gatherAllHands(List<RATSkeletonProvider> skeletonProviders)
        {
            List<TrackedHand> currFrameHands = new List<TrackedHand>();

            if (skeletonProviders.Count > 0)
            {
                int providerID = 1;
                foreach (RATSkeletonProvider skelProvider in skeletonProviders)
                {
                    // To Do: check these outputs show unique providers
                    print("Skel Provider:");
                    print(skelProvider);

                    for (int skelID = 1; skelID <= maxBodies; skelID++) // Loops 1 thru maxBodies
                    {
                        RATKinectSkeleton skel = skelProvider.GetKinectSkeleton(skelID);
                        if (skel != null && skel.valid)
                        {
                            string providerSkelID = $"SP{providerID}-SK{skelID}";

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
            return currFrameHands;
        }

        private List<TrackedHand> mergeProximityMatches(List<TrackedHand> handsList)
        {

            // if matched, set handID to 'merged', then create merged list for match checker to compare with instead
            // merged list is the list of all the handIDs in the merge
            // list of merged could be more than 2. two hands together is 4 seen hands across 2 providers
            // Vector3.Distance
            List<TrackedHand> mergedList = new List<TrackedHand>();

            foreach (TrackedHand hand in handsList)
            {
                // make list of merged hands. new class that contains the hand class, plus list of ids?
                // new class in total?

                Vector3 pos = hand.position;
                List<TrackedHand> near = handsList.FindAll(h => Vector3.Distance(pos, h.position) < objDiameter);

                // list of all the hands that this one matches, including itself
                // merge these together and save
                TrackedHand merged = mergeHands(near);

                mergedList.Add(merged);

            }

            // tasks: what about the matching system's use of provider/skel/hand ids?
            //  - once you merge, that will be in the current and that gets pushed to previous, with out that id
            //  - merge matches should also list all the handIds they are made up of
            //  - see about a method to compare un-ordered lists

            return mergedList;
        }

        private TrackedHand mergeHands(List<TrackedHand> near)
        {

            return new TrackedHand()
            {
                handID = near.Select(h => h.handID[0]).ToList()
                // Vector3 position = average position
                // bool frameMatch = false 
                // GameObject handObject = pick one, recycle the rest

            };

            // to do: here in the code ^
            // to do: can this function code be merged into the calling function

        }

        private List<TrackedHand> getHandPositions(RATSkeletonProvider skelProvider, string providerSkelID, RATKinectSkeleton skel)
        {
            List<TrackedHand> hands = new List<TrackedHand>();

            if (skel != null && skel.valid)
            {
                // check if there is confidence here, if it returns a hand
                // To Do: validate this works and add filter code here
                print("RHand Confidence: " + skel.handRightConfidence);
                print("LHand Confidence: " + skel.handLeftConfidence);

                Vector3 kinPosRight = skel.jointPositions3D[(int)JointType.HandRight]; // in skeleton provider coordinate system 
                Vector3 kinPosLeft = skel.jointPositions3D[(int)JointType.HandLeft]; // in skeleton provider coordinate system

                Vector3 worldPosRight = skelProvider.transform.localToWorldMatrix.MultiplyPoint(kinPosRight); // move to world coordinates
                Vector3 worldPosLeft = skelProvider.transform.localToWorldMatrix.MultiplyPoint(kinPosLeft); // move to world coordinates

                if (worldPosRight != null)
                {
                    hands.Add(new TrackedHand()
                    {
                        handID = new List<string> { $"{providerSkelID}-Right" },
                        position = worldPosRight
                    });
                }
                if (worldPosLeft != null)
                {
                    hands.Add(new TrackedHand()
                    {
                        handID = new List<string> { $"{providerSkelID}-Left" },
                        position = worldPosLeft
                    });
                }
            }
            return hands;
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
                    if (currHand.handID == prevHand.handID)
                    {
                        currHand.frameMatch = true;
                        prevHand.frameMatch = true;
                        currHand.handObject = prevHand.handObject;
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
                handObject = inactiveHands[0];
                inactiveHands.RemoveAt(0);
            }
            else
            {
                handObject = Instantiate(baseHandObj, new Vector3(0, 0, 0), Quaternion.identity);
            }
            handObject.SetActive(true);
            return handObject;
        }

        private void discardHand(GameObject obj)
        {
            // Deactivate and add to inactive list
            obj.SetActive(false);
            inactiveHands.Add(obj);
        }

    }
}