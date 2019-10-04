using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenVISDIVE
{
    using ColliderTriggerMap = Dictionary<Collider, bool>;
    public static class PhysicsUtility
    {

        /// <summary>
        /// Store a map of Colliders and their trigger state into a dictionary
        /// </summary>
        /// <param name="targetObject">Game object to scan</param>
        /// <param name="colliderTriggerBackup">Dictionary used to map colliders to their trigger state</param>
        public static void BackupTriggerState(GameObject targetObject, ColliderTriggerMap colliderTriggerBackup)
        {
            colliderTriggerBackup.Clear();
            foreach (Collider collider in targetObject.GetComponentsInChildren<Collider>(true))
            {
                colliderTriggerBackup.Add(collider, collider.isTrigger);
            }
        }


        /// <summary>
        /// Replace the collider trigger states of a game object using a previously stored map (see <see cref="BackupTriggerState"/>)
        /// </summary>
        /// <param name="targetObject">Game object to scan</param>
        /// <param name="colliderTriggerBackup">Dictionary that maps colliders to their trigger state</param>
        /// <remarks>If the given dictionary does not match the colliders the behaviour is undefined.</remarks>
        public static void RestoreTriggerState(GameObject targetObject, ColliderTriggerMap colliderTriggerBackup)
        {
            foreach (Collider collider in targetObject.GetComponentsInChildren<Collider>(true))
            {
                if (colliderTriggerBackup.ContainsKey(collider))
                {
                    collider.isTrigger = colliderTriggerBackup[collider];
                }
            }
        }


        /// <summary>
        /// Replace the collider isTrigger property of a game object with the given value
        /// </summary>
        /// <param name="targetObject">Game object to scan.</param>
        /// <param name="triggerState">Value for the isTrigger property.</param>
        public static void SetTriggerState(GameObject targetObject, bool triggerState)
        {
            foreach (Collider collider in targetObject.GetComponentsInChildren<Collider>(true))
            {
                collider.isTrigger = triggerState;
            }
        }


        /// <summary>
        /// Replace the collider isConvex property of a game object with the given value
        /// </summary>
        /// <param name="targetObject">Game object to scan.</param>
        /// <param name="isConvex">Value for the isConvex property.</param>
        public static void SetColliderConvex(GameObject targetObject, bool isConvex)
        {
            foreach (MeshCollider collider in targetObject.GetComponentsInChildren<MeshCollider>(true))
            {
                collider.convex = isConvex;
            }
        }


        /// <summary>
        /// Get the composite bounds of a game object, scanning its Colliders.
        /// </summary>
        /// <param name="gameObj">Game object to scan.</param>
        /// <returns></returns>
        /// <remarks>Collider's bounds are axis-aligned bounding boxes expressed in world space.</remarks>
        public static Bounds GetColliderBounds(GameObject gameObj)
        {
            Bounds bounds = new Bounds();
            bounds.center = gameObj.transform.position;
            foreach(Collider collider in gameObj.GetComponentsInChildren<Collider>())
            {
                bounds.Encapsulate(collider.bounds);
            }
            return bounds;
        }

    }
}
