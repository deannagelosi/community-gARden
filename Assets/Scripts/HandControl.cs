using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoomAliveToolkit
{
    public struct ValidSkel
    {
        public RATSkeletonProvider skelProvider { get; set; }
        public RATKinectSkeleton skel { get; set; }
    }

    public struct TrackedHand
    {
        public RATSkeletonProvider skelProvider { get; set; }
        public int skelID { get; set; }
        public string side { get; set; }
        public Vector3 position { get; set; }
    }

    public class HandControl : MonoBehaviour
    {
        public List<RATSkeletonProvider> skeletonProviders;
        public int maxBodies = 1; // Track 1 skeleton by default. Kinect can track up to 6 skeletons.

        // Track and reuse hand game objects
        private List<GameObject> activeHands = new List<GameObject>();
        private List<GameObject> inactiveHands = new List<GameObject>();

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
            // Gather all the valid skeletons visible to the kinect(s)
            List<ValidSkel> validSkels = new List<ValidSkel>();
            // Gather all the detected hands accross all providers and skeleton ids
            List<TrackedHand> allHands = new List<TrackedHand>();

            if (skeletonProviders.Count > 0)
            {
                foreach (RATSkeletonProvider skelProvider in skeletonProviders)
                {
                    for (int skelID = 1; skelID <= maxBodies; skelID++) // Loops 1 thru maxBodies
                    {
                        RATKinectSkeleton skel = skelProvider.GetKinectSkeleton(skelID);
                        if (skel != null && skel.valid)
                        {
                            validSkels.Add(new ValidSkel() // to do: do we even need this data again?
                            {
                                skelProvider = skelProvider,
                                skel = skel,
                            });

                            List<TrackedHand> skelHands = getHandPositions(skelProvider, skelID, skel);
                            if (skelHands != null)
                            {
                                allHands.AddRange(skelHands);
                            }
                        }
                    }
                }
            }

            // To Do: merge and de-dup hands in close proximity

            // Render hand game objects at each position
            // deactivate hands game objects that are lost
            // - need to track between frames which hand position is which GM, to detect loss
            // - use the provider and body id. 
            //   - if active in last frame is seen in new frame, use the same GM
            //   - if seen in last frame is not seen in this one, its a loss. deactivate and send to inactive list
            //   - if not seen in last frame, but seen in this one, pull a one from the inactive list (or instantiate one if list is empty)
            // Collect and reuse hand objects that are lost


            // match between frames on:
            // - same provider
            // - same skel id
            // - same hand (L or R)






            // 3. manage all the hand position coordinates
            // 4. straight populate render positions (stretch - calc'ed with pairing)
            // 5. dont need to manage adding/removing to list since its remade each time, but
            //    need a global way to keep track of the GMs so you are updating the same one each time right?
            // dedup render list and manage renders and transforms
            // manage active/inactive lists



            // if (skeletonProvider != null)
            // {
            //     RATKinectSkeleton skeleton = GetSkeleton();

            //     if (skeleton != null && skeleton.valid)
            //     {
            //         if (updateFromKinect)
            //         {
            //             if (gameObject.CompareTag("RightHand"))
            //             {
            //                 transform.position = getRightHandPosition();
            //             }

            //             else if (gameObject.CompareTag("LeftHand"))
            //             {
            //                 transform.position = getLeftHandPosition();
            //             }
            //         }
            //     }
            // }

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

    }
}