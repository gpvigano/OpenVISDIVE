using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenVISDIVE;

namespace OpenVISDIVE
{
    using MaterialMap = Dictionary<Renderer, Material>;

    public class RaycastPositioner : MonoBehaviour
    {
        [Tooltip("Script in charge of checking if a position is valid")]
        [SerializeField]
        private EntityValidator entityValidator = null;

        [Tooltip("Distance at which the object is placed if no obstacle is present")]
        [SerializeField]
        private float defaultDistance = 5f;

        [Tooltip("Distance at which the objects can be picked and placed")]
        [SerializeField]
        private float pickingDistance = 50f;

        [Tooltip("Align the object to the normal at hit point, when an obstacle is present")]
        [SerializeField]
        private bool alignToNormal = false;

        [SerializeField]
        private Color highlightColor = Color.white;

        [SerializeField]
        private Color collidingColor = Color.gray;

        [SerializeField]
        private Color allowedPositionColor = Color.green;

        [SerializeField]
        private Color issuesPositionColor = Color.yellow;

        [SerializeField]
        private Color collisionPositionColor = Color.red;

        [SerializeField]
        [Tooltip("Side of the object bounding box to be leant against a colliding object")]
        private MathUtility.Axis anchorFace = MathUtility.Axis.Down;

        [Tooltip("Input axis for rotation on collision")]
        [SerializeField]
        private string rotationInputAxis = "Mouse ScrollWheel";

        [Tooltip("Angle multiplier for rotation")]
        [SerializeField]
        private float rotationAngleFactor = 50f;

        //[Tooltip("Input axis for distance without collision")]
        //[SerializeField]
        //private string distanceInputAxis = "Mouse ScrollWheel";

        [SerializeField]
        private float collisionOffset = 0.025f;

        [Tooltip("Game object containing the UI (hidden when positioning)")]
        [SerializeField]
        private GameObject uiRoot = null;

        private Camera raycastingCamera = null;
        private GameObject aliasObject = null;
        //private GameObject aliasObjectHolder = null;
        private Rigidbody aliasObjectRigidbody = null;
        private bool isPositioning = false;
        private bool wasTargetObjectActive = false;
        private float currentDistance = 5f;
        private float originalHeadingOffset = 0f;
        private float currentHeadingOffset = 0f;

        // Game object hit by the casted ray
        private GameObject hitObject = null;
        // Entity data related to the game object hit by the casted ray (only if not allowed)
        private EntityData hitEntity = null;
        // Entity data related to the game object hit by the casted ray (even if not allowed)
        private EntityData bearingSurfaceEntity = null;
        // Entity data related to the original game object
        private EntityData objectEntity = null;
        // Entity data related to the game object colliding with the alias game object
        private EntityData collidingEntity = null;
        private CollisionState collisionState = null;
        private Vector3 pickingOffset = Vector3.zero;
        private Ray pickingRay = new Ray();
        private Vector3 aliasObjBoundAnchor = Vector3.zero;
        private MaterialMap originalMaterialMap = new MaterialMap();
        private Material coloredMaterial = null;
        private GameObject lastCollidingObject = null;
        private ObjectHighlighter objectHighlighter = null;

        /// <summary>
        /// Last point hit by the raycast.
        /// </summary>
        public Vector3 HitPoint { get; protected set; }

        /// <summary>
        /// Normal of the surface in the last point hit by the raycast.
        /// </summary>
        public Vector3 HitNormal { get; protected set; }

        /// <summary>
        /// Distance of the last point hit by the raycast.
        /// </summary>
        public float HitDistance { get; protected set; }

        /// <summary>
        /// Check if the current position is allowed or not.
        /// </summary>
        public bool AllowedPosition
        {
            get
            {
                if (collisionState == null)
                {
                    return false;
                }
                //collisionState.IsColliding && hitEntity != null && collisionState.CollidingObject == hitEntity && !hitEntity.isStatic
                return entityValidator.IsValid(aliasObject, objectEntity, GetHitEntity(), GetCollidingEntity(), collisionState);
            }
        }

        /// <summary>
        /// Check if the current object state has pending issues or not.
        /// </summary>
        public bool HasPendingIssues
        {
            get
            {
                return entityValidator.HasPendingIssues(aliasObject, objectEntity, bearingSurfaceEntity, GetCollidingEntity());
            }
        }

        public GameObject OriginalObject { get; protected set; } = null;


        public event Action<GameObject, Vector3, Vector3, Vector3, Vector3> Positioned;



        public void StartPositioning(GameObject controlledObject)
        {
            if (controlledObject != null && !isPositioning)
            {
                currentDistance = defaultDistance;
                CancelPositioning();
                OriginalObject = controlledObject;
                CreateObjectAlias();
                wasTargetObjectActive = OriginalObject.activeSelf;
                OriginalObject.SetActive(false);
                isPositioning = true;
                if (uiRoot != null)
                {
                    uiRoot.SetActive(false);
                }
            }
        }


        public void StopPositioning()
        {
            //if (allowPositioningOnCollision || collisionState != null && !collisionState.IsColliding)
            if (isPositioning && AllowedPosition)
            {
                SetObjectPosition(HitPoint, 0);
                OnPositioned(OriginalObject, OriginalObject.transform.position, OriginalObject.transform.eulerAngles, aliasObjectRigidbody.position, aliasObject.transform.eulerAngles);
                OriginalObject.transform.position = aliasObjectRigidbody.position;
                OriginalObject.transform.rotation = aliasObject.transform.rotation;
                CancelPositioning();
            }
        }


        protected void OnPositioned(GameObject targetObject, Vector3 prevPosition, Vector3 prevEulerAngles, Vector3 position, Vector3 eulerAngles)
        {
            Positioned?.Invoke(targetObject, prevPosition, prevEulerAngles, position, eulerAngles);
        }


        public void Clear()
        {
            CancelPositioning();
            objectHighlighter.UnhighlightAll();
            collisionState.Clear();
            lastCollidingObject = null;
            ResetSelectionData();
        }


        public void CancelPositioning()
        {
            if (isPositioning)
            {
                DestroyObjectAlias();
                isPositioning = false;
                OriginalObject.SetActive(wasTargetObjectActive);
                if (uiRoot != null)
                {
                    uiRoot.SetActive(true);
                }
                OriginalObject = null;
            }
        }


        private void CreateObjectAlias()
        {
            objectEntity = OriginalObject.GetComponentInParent<EntityData>();
            anchorFace = objectEntity.anchorSide;
            coloredMaterial = RenderingUtility.CreateColoredMaterial(collisionPositionColor);
            aliasObject = Instantiate(OriginalObject);
            PhysicsUtility.SetColliderConvex(aliasObject, true);
            originalHeadingOffset = aliasObject.transform.eulerAngles.y;
            aliasObject.transform.rotation = Quaternion.identity;
            Bounds bounds = PhysicsUtility.GetColliderBounds(aliasObject);
            aliasObjBoundAnchor = MathUtility.GetBoundsFaceCenter(bounds, anchorFace) - aliasObject.transform.position;
            aliasObject.transform.SetParent(transform);
            currentHeadingOffset = 0;// aliasObject.transform.eulerAngles.y;
            collisionOffset = 0;
            RenderingUtility.ChangeLayerRecursive(aliasObject, "Ignore Raycast");
            RenderingUtility.BackupMaterials(aliasObject, originalMaterialMap);
            RenderingUtility.ReplaceMaterials(aliasObject, coloredMaterial);
            collisionState = aliasObject.AddComponent<CollisionState>();
            aliasObjectRigidbody = aliasObject.AddComponent<Rigidbody>();
            aliasObjectRigidbody.useGravity = false;
            aliasObjectRigidbody.freezeRotation = true;
            aliasObjectRigidbody.drag = 1000;
            //rb.isKinematic = true;
        }


        private void DestroyObjectAlias()
        {
            Destroy(aliasObject);
            aliasObject = null;
            aliasObjectRigidbody = null;
            //materialMap.Clear();
            Destroy(coloredMaterial);
            coloredMaterial = null;
            objectEntity = null;
        }


        private void ChangeLayerRecursive(GameObject obj)
        {
            int layer = LayerMask.NameToLayer("Ignore Raycast");
            obj.layer = layer;
            Transform[] childrenTransform = obj.GetComponentsInChildren<Transform>();
            foreach (Transform tr in childrenTransform)
            {
                tr.gameObject.layer = layer;
            }
        }


        private bool PickEntity()
        {
            hitObject = null;
            if (Pick())
            {
                hitEntity = hitObject.GetComponentInParent<EntityData>();
                if (entityValidator.CanBeSelected(hitEntity))
                {
                    hitObject = hitEntity?.gameObject;
                    Debug.Log($"Picked entity {hitEntity.name}");
                    return true;
                }
            }
            ResetSelectionData();

            return false;
        }


        private EntityData GetHitEntity()
        {
            if (hitObject != null)
            {
                hitEntity = hitObject.GetComponentInParent<EntityData>();
                if (!entityValidator.CanBeLeantAgainst(objectEntity, hitEntity))
                {
                    hitEntity = null;
                }
                hitObject = hitEntity?.gameObject;
            }
            else
            {
                hitEntity = null;
            }
            return hitEntity;
        }



        private EntityData GetCollidingEntity()
        {
            collidingEntity = null;
            foreach (GameObject obj in collisionState.CollidingObjects)
            {
                EntityData entityData = obj.GetComponentInParent<EntityData>();
                if (entityData != null && entityData != objectEntity)
                {
                    collidingEntity = entityData;
                    break;
                }
            }
            return collidingEntity;
        }


        private bool Pick()
        {
            // Do a raycast into the world based on the user's
            // head position and orientation.
            Vector3 origin = transform.position;
            //Vector3 direction = transform.forward;
            //if (transform.parent != null)
            //{
            //    Vector3 offset = transform.parent.position - origin;
            //    if (Vector3.SqrMagnitude(offset) > float.Epsilon)
            //    {
            //        direction = offset.normalized;
            //    }

            //}
            RaycastHit hitInfo;
            pickingRay = raycastingCamera.ScreenPointToRay(Input.mousePosition);
            //bool hit = Physics.Raycast(transform.position, direction, out hitInfo, pickingDistance);
            bool hit = Physics.Raycast(pickingRay, out hitInfo, pickingDistance);
            if (hit)
            {
                HitPoint = hitInfo.point;
                HitNormal = hitInfo.normal;
                HitDistance = hitInfo.distance;
                hitObject = hitInfo.collider.gameObject;
                bearingSurfaceEntity = hitObject?.GetComponentInParent<EntityData>();
            }
            else
            {
                hitObject = null;
                bearingSurfaceEntity = null;
            }
            return hit;
        }


        private void ResetSelectionData()
        {
            hitObject = null;
            hitEntity = null;
            bearingSurfaceEntity = null;
        }


        private void Update()
        {
            bool leftMouseButtonUp = false;
            bool rightMouseButtonUp = false;
            Vector3 mPos = Input.mousePosition;
            bool mouseInsideScreen = (mPos.x >= 0 && mPos.x < Screen.width && mPos.y >= 0 && mPos.y < Screen.height);
            if (mouseInsideScreen)
            {
                leftMouseButtonUp = Input.GetMouseButtonUp(0);
                rightMouseButtonUp = Input.GetMouseButtonUp(1);
                float rotationAxis = Input.GetAxis(rotationInputAxis);
                if (isPositioning)
                {
                    if (rightMouseButtonUp)
                    {
                        CancelPositioning();
                        return;
                    }
                    if (leftMouseButtonUp)
                    {
                        if (AllowedPosition)
                        {
                            StopPositioning();
                        }
                        return;
                    }
                    currentHeadingOffset += rotationAngleFactor * rotationAxis;
                    collisionState.collisionDisplacement = collisionOffset;
                    bool hit = Pick();
                    if (hit)
                    {
#if UNITY_EDITOR // debug only
                        if (GetHitEntity() != bearingSurfaceEntity)
                        {
                            Debug.Log($"{hitEntity?.name}!={bearingSurfaceEntity?.name}");
                        }
#endif
                        SetHitPosition();
                    }
                }
                else
                {
                    GameObject prevHitObject = hitObject;
                    bool picked = PickEntity();

                    if (leftMouseButtonUp && picked && hitObject != null)
                    {
                        if (prevHitObject != null)
                        {
                            HighlightFocusedObject(prevHitObject, false);
                        }
                        pickingOffset = hitObject.transform.position - transform.position;
                        StartPositioning(hitObject);
                    }
                    else if (hitObject != prevHitObject)
                    {
                        if (prevHitObject != null)
                        {
                            HighlightFocusedObject(prevHitObject, false);
                        }
                        if (hitObject != null && !leftMouseButtonUp)
                        {
                            HighlightFocusedObject(hitObject, true);
                        }
                    }
                }
            }
        }


        private void HighlightFocusedObject(GameObject obj, bool highlighted)
        {
            objectHighlighter.HighlightObject(obj, highlighted, highlightColor);
        }


        private void HighlightCollidingObject(GameObject obj, bool highlighted)
        {
            objectHighlighter.HighlightObject(obj, highlighted, collidingColor);
        }


        private void LateUpdate()
        {
            if (isPositioning)
            {
                bool allowedPos = AllowedPosition;
                lastCollidingObject = collisionState.CollidingObject;
                if (!allowedPos && collisionState.IsColliding && lastCollidingObject != null)
                {
                    HighlightCollidingObject(lastCollidingObject, true);
                }
                else
                {
                    objectHighlighter.UnhighlightAll();
                }

                if (allowedPos)
                {
                    if (HasPendingIssues)
                    {
                        coloredMaterial.color = issuesPositionColor;
                    }
                    else
                    {
                        coloredMaterial.color = allowedPositionColor;
                    }
                }
                else
                {
                    coloredMaterial.color = collisionPositionColor;
                }
            }
            else
            {
                if (lastCollidingObject != null)
                {
                    objectHighlighter.UnhighlightAll();
                }
                if (lastCollidingObject != null)
                {
                    lastCollidingObject = null;
                }
            }
        }


        private void SetObjectPosition(Vector3 pos, float collisionOffset)
        {
            //Vector3 offset = Quaternion.Inverse(aliasObject.transform.rotation) * aliasObjBoundAnchor;
            //Vector3 offset = aliasObject.transform.rotation * aliasObjBoundAnchor;
            Vector3 offset = Vector3.zero;
            aliasObjectRigidbody.position = pos - offset + HitNormal * collisionOffset;
        }


        private void SetHitPosition()
        {
            if (alignToNormal)
            {
                MathUtility.SetTransformAxis(aliasObject.transform, anchorFace, -HitNormal);
                //eulerAngles.x = 0;
                //eulerAngles.z = 0;
                Vector3 eulerAngles = aliasObject.transform.eulerAngles;
                eulerAngles.y = aliasObject.transform.eulerAngles.y + currentHeadingOffset;
                aliasObject.transform.eulerAngles = eulerAngles;
            }
            else
            {
                Vector3 eulerAngles = aliasObject.transform.eulerAngles;
                eulerAngles.y = originalHeadingOffset + currentHeadingOffset;
                aliasObject.transform.eulerAngles = eulerAngles;
            }
            SetObjectPosition(HitPoint, collisionOffset);
            currentDistance = HitDistance;
        }


        private void OnDisable()
        {
            CancelPositioning();
        }


        private void Awake()
        {
            raycastingCamera = GetComponentInParent<Camera>();
            if (raycastingCamera == null)
            {
                raycastingCamera = Camera.main;
            }
            if (objectHighlighter == null)
            {
                objectHighlighter = GetComponent<ObjectHighlighter>();
            }
            if (objectHighlighter == null)
            {
                objectHighlighter = gameObject.AddComponent<ObjectHighlighter>();
            }
        }
    }
}
