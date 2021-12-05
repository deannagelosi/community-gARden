using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomAliveToolkit
{

    // Class object for collecting all the tracked hand data
    public class TrackedHand
    {
        public RATSkeletonProvider skelProvider { get; set; }
        public int skelID { get; set; }
        public string side { get; set; }
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
            // Gather all the detected hands accross all providers and skeleton ids
            List<TrackedHand> currFrameHands = gatherAllHands(skeletonProviders);

            // Clear old match data and check for new matches
            resetAndFindMatch(currFrameHands, prevFrameHands);
            // To Do: ^ make sure passed in vars modifications are being seen after

            // To Do: merge and de-dup found hands in close proximity

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
                            List<TrackedHand> skelHands = getHandPositions(skelProvider, skelID, skel);
                            if (skelHands != null)
                            {
                                currFrameHands.AddRange(skelHands);
                            }
                        }
                    }
                }
            }
            return currFrameHands;
        }

        private List<TrackedHand> getHandPositions(RATSkeletonProvider skelProvider, int skelID, RATKinectSkeleton skel)
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
                        skelProvider = skelProvider,
                        skelID = skelID,
                        side = "Right",
                        position = worldPosRight
                    });
                }
                if (worldPosLeft != null)
                {
                    hands.Add(new TrackedHand()
                    {
                        skelProvider = skelProvider,
                        skelID = skelID,
                        side = "Left",
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
                    // match between frames on: provider, skel id, hand (L or R)
                    bool provider = currHand.skelProvider == prevHand.skelProvider;
                    bool id = currHand.skelID == prevHand.skelID;
                    bool hand = currHand.side == prevHand.side;

                    if (provider && id && hand)
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