using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace OpenVISDIVE
{
    [System.Serializable]
    [XmlType(TypeName = "modelImportOptions")]
    /// <summary>
    /// Options to define how the model will be loaded and imported.
    /// </summary>
    public class ModelImportOptions
    {
        [Tooltip("load the OBJ file assumitg its vertical axis is Z instead of Y")]
        public bool zUp = true;

        [Tooltip("Consider diffuse map as already lit (disable lighting) if no other texture is present")]
        public bool litDiffuse = false;

        [Tooltip("Consider to double-sided (duplicate and flip faces and normals")]
        public bool convertToDoubleSided = false;

        [Tooltip("Rescaling for the model (1 = no rescaling)")]
        public float modelScaling = 1f;

        [Tooltip("Reuse a model in memory if already loaded")]
        public bool reuseLoaded = false;

        [Tooltip("Inherit parent layer")]
        public bool inheritLayer = false;

        [Tooltip("Generate mesh colliders")]
        public bool buildColliders = false;

        [Tooltip("Mesh colliders as trigger (only active if colliderConvex = true)")]
        public bool colliderTrigger = false;

        [Header("Local Transform for the imported game object")]
        [Tooltip("Position of the object")]
        public Vector3 localPosition = Vector3.zero;

        [Tooltip("Rotation of the object\n(Euler angles)")]
        public Vector3 localEulerAngles = Vector3.zero;

        [Tooltip("Scaling of the object\n([1,1,1] = no rescaling)")]
        public Vector3 localScale = Vector3.one;
    }
}
