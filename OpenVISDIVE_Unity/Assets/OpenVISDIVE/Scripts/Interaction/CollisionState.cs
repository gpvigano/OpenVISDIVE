using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenVISDIVE
{
    public class CollisionState : MonoBehaviour
    {
        public float collisionDisplacement = 0.01f;
        private Vector3 collisionOffset = Vector3.zero;
        private Vector3 collisionLowerPoint = Vector3.zero;

        public bool IsColliding { get => collidingObjects.Count > 0; }
        public float CollisionDepth { get; protected set; } = 0;
        public Vector3 CollisionOffset { get => collisionOffset; }
        public Vector3 CollisionLowerPoint { get => collisionLowerPoint; }
        public GameObject CollidingObject { get => IsColliding ? collidingObjects[0] : null; }
        public List<GameObject> CollidingObjects { get => collidingObjects; }

        private List<GameObject> collidingObjects = new List<GameObject>();

        public void Clear()
        {
            Init();
            collidingObjects.Clear();
        }


        private void Init()
        {
            CollisionDepth = 0;
            collisionOffset = Vector3.zero;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Init();
            collidingObjects.Add(collision.gameObject);
        }


        private void OnCollisionStay(Collision collision)
        {
            collisionLowerPoint = Vector3.up * float.MaxValue;
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.separation <= 0)
                {
                    Vector3 offset = contact.normal * contact.separation;
                    if (Mathf.Abs(collisionLowerPoint.y) > Mathf.Abs(contact.point.y))
                    {
                        collisionLowerPoint = contact.point;
                    }
                    if (Mathf.Abs(collisionOffset.x) < Mathf.Abs(offset.x))
                    {
                        collisionOffset.x = offset.x + contact.normal.x * collisionDisplacement;
                    }
                    if (Mathf.Abs(collisionOffset.y) < Mathf.Abs(offset.y))
                    {
                        collisionOffset.y = offset.y + contact.normal.y * collisionDisplacement;
                    }
                    if (Mathf.Abs(collisionOffset.z) < Mathf.Abs(offset.z))
                    {
                        collisionOffset.z = offset.z + contact.normal.y * collisionDisplacement;
                    }
                }
                if (contact.separation < CollisionDepth)
                {
                    CollisionDepth = contact.separation;
                }
            }
        }


        private void OnCollisionExit(Collision collision)
        {
            Init();
            collidingObjects.Remove(collision.gameObject);
        }
    }
}
