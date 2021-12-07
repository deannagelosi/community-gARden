using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RoomAliveToolkit
{

    // Class object for collecting all the tracked hand data
    public class TrackedHand
    {
        // public RATSkeletonProvider skelProvider { get; set; }
        // public int skelID { get; set; }
        public string handID { get; set; }
        public Vector3 providerPosition { get; set; }
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
        public float proximityDistance = 0.15f; // Default to hand object colision mesh diameter

        // Collect inactive hands for re-use (instead of instantiating more and more)
        private List<GameObject> inactiveHands = new List<GameObject>();
        private List<TrackedHand> currFrameHands = new List<TrackedHand>();
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
            currFrameHands = gatherAllHands(skeletonProviders);

            // Merging hands in proximity, de-duplicating hand objects in the location space
            currFrameHands = mergeProximityMatches(currFrameHands);

            // Clear old match data and check for new matches
            resetAndFindMatch(currFrameHands, prevFrameHands);

            // Manage CURRENT hand game objects
            foreach (TrackedHand currHand in currFrameHands)
            {
                if (currHand.frameMatch)
                {
                    // Not New. Retrieved this hand from the previous frame list
                    currHand.handObject.transform.position = currHand.position;
                }
                else if (!currHand.frameMatch) // || currHand.handObject == null (and move this up one check ^)
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
                if (!prevHand.frameMatch) { discardHand(prevHand); }
            }

            // Save current matches for the next frame
            // prevFrameHands = new List<TrackedHand>();
            prevFrameHands = currFrameHands;
        }

        private List<TrackedHand> gatherAllHands(List<RATSkeletonProvider> skeletonProviders)
        {

            List<TrackedHand> foundHands = new List<TrackedHand>();

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

                            List<TrackedHand> skelHands = getHandPositions(skelProvider, providerSkelID, skel);
                            if (skelHands != null)
                            {
                                foundHands.AddRange(skelHands);
                            }
                        }
                    }
                    providerID++;
                }
            }

            return foundHands;
        }

        private List<TrackedHand> mergeProximityMatches(List<TrackedHand> handsList)
        {
            List<TrackedHand> mergedHands = new List<TrackedHand>();

            foreach (TrackedHand hand in handsList)
            {
                // Find all other hands in proximity (including itself)
                List<TrackedHand> handsNear = handsList.FindAll(h => Vector3.Distance(hand.position, h.position) <= proximityDistance);

                if (handsNear.Count > 1)
                {
                    // Disable old hand objects, going to use a new one
                    handsNear.ForEach(h => discardHand(h));

                    // Combine sorted handIDs of all matched hands
                    List<string> allHandIDs = handsNear.Select(h => h.handID).ToList();
                    allHandIDs.Sort();
                    string sortedHandIDs = String.Join("+", allHandIDs.ToArray());

                    // Check if a new find (ie. merging hands A and B, but already merged B and A)
                    bool alreadyFound = mergedHands.Any(h => h.handID == sortedHandIDs);

                    if (!alreadyFound)
                    {
                        // Create a new merged hand
                        // - Average all of the positions
                        Vector3 sumPos = Vector3.zero;
                        handsNear.ForEach(h => sumPos += h.position);
                        Vector3 averagePos = new Vector3(sumPos.x / handsNear.Count, sumPos.y / handsNear.Count, sumPos.z / handsNear.Count);

                        // - Construct and save merged hand
                        TrackedHand mergedHand = new TrackedHand()
                        {
                            handID = sortedHandIDs,
                            position = averagePos,
                            handObject = null, // Update() assigns new hands a GameObject
                            frameMatch = false
                        };
                        mergedHands.Add(mergedHand);
                    }
                }
                else if (handsNear.Count == 1)
                {
                    // Only found 1 hand in proximity (itself). Save and return it
                    mergedHands.Add(handsNear[0]);
                }
            }

            return mergedHands; // Replace old hand list with new one
        }

        private List<TrackedHand> getHandPositions(RATSkeletonProvider skelProvider, string providerSkelID, RATKinectSkeleton skel)
        {
            List<TrackedHand> hands = new List<TrackedHand>();
            // Get any detected Hands
            if (skel != null && skel.valid)
            {
                // Note: Code duplication. Could this instead loop {"Right", "Left"} and call right/left methods?
                if (skipConfidenceCheck || skel.handRightConfidence == 1) // Get Right
                {
                    Vector3 providerPos = skel.jointPositions3D[(int)JointType.HandRight];
                    Vector3 worldPos = skelProvider.transform.localToWorldMatrix.MultiplyPoint(providerPos); // convert to world coordinates

                    if (worldPos != null)
                    {
                        hands.Add(new TrackedHand()
                        {
                            handID = $"{providerSkelID}-Right",
                            position = worldPos
                        });
                    }
                }

                if (skipConfidenceCheck || skel.handLeftConfidence == 1) // Get Left
                {
                    Vector3 providerPos = skel.jointPositions3D[(int)JointType.HandLeft];
                    Vector3 worldPos = skelProvider.transform.localToWorldMatrix.MultiplyPoint(providerPos); // convert to world coordinates

                    if (worldPos != null)
                    {
                        hands.Add(new TrackedHand()
                        {
                            handID = $"{providerSkelID}-Left",
                            position = worldPos
                        });
                    }
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
                    // handID matches same provider, skel id, and hand (L or R) + pairs
                    if (currHand.handID == prevHand.handID)
                    // if (Enumerable.SequenceEqual(currHand.handIDs, prevHand.handIDs))
                    {
                        currHand.frameMatch = true;
                        prevHand.frameMatch = true;
                        currHand.handObject = prevHand.handObject;
                    }
                    else
                    {
                        // print($"no match. Curr HandID: {currHand.handID} Prev HandID: {prevHand.handID}");
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

        private void discardHand(TrackedHand hand)
        {
            // Deactivate and add to inactive list
            if (hand.handObject != null)
            {
                hand.handObject.SetActive(false);
                inactiveHands.Add(hand.handObject);
            }
        }

    }
}