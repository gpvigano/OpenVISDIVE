using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OpenVISDIVE
{
    using MaterialMap = Dictionary<Renderer, Material>;

    /// <summary>
    /// Blend mode for Unity Material
    /// </summary>
    public enum RenderingMode { OPAQUE, CUTOUT, FADE, TRANSPARENT }

    /// <summary>
    /// Rendering utility methods.
    /// </summary>
    /// <remarks>Code derived from AsImpL (https://github.com/gpvigano/AsImpL)
    /// and GPVUDK (https://github.com/gpvigano/GPVUDK)</remarks>
    public static class RenderingUtility
    {
        /// <summary>
        /// Set the given game object as visible or not, enabling or disabling
        /// all its mesh renderers (children included)
        /// </summary>
        /// <param name="targetObject">The main game object to show/hide</param>
        /// <param name="visible">if true enable mesh renderers, if false disable them</param>
        public static void SetVisible(GameObject targetObject, bool visible)
        {
            foreach (MeshRenderer mr in targetObject.GetComponentsInChildren<MeshRenderer>())
            {
                mr.enabled = visible;
            }
        }


        /// <summary>
        /// Change the layer of a game object and all the game objects attached to it, recursively.
        /// </summary>
        /// <param name="obj">Game object to be changed</param>
        /// <param name="layer">Layer to be set</param>
        public static void ChangeLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            Transform[] childrenTransform = obj.GetComponentsInChildren<Transform>();
            foreach (Transform tr in childrenTransform)
            {
                tr.gameObject.layer = layer;
            }
        }


        /// <summary>
        /// Change the layer of a game object and all the game objects attached to it, recursively.
        /// </summary>
        /// <param name="obj">Game object to be changed</param>
        /// <param name="layerName">Name of the layer to be set</param>
        public static void ChangeLayerRecursive(GameObject obj, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            ChangeLayerRecursive(obj, layer);
        }


        /// <summary>
        /// Get the composite bounds from all the renderers of the given object.
        /// </summary>
        /// <param name="obj">Object with renderers.</param>
        /// <returns>Composite bounds from all the renderers.</returns>
        public static Bounds GetRendererBounds(GameObject obj)
        {
            Bounds bounds = new Bounds();
            bounds.center = obj.transform.position;
            foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(rend.bounds);
            }
            return bounds;
        }


        /// <summary>
        /// Store a map of renderers and their materials into a dictionary
        /// </summary>
        /// <param name="targetObject">Game object to scan</param>
        /// <param name="materialsBackup">Dictionary used to map renderers to their materials</param>
        public static void BackupMaterials(GameObject targetObject, MaterialMap materialsBackup)
        {
            materialsBackup.Clear();
            foreach (Renderer mr in targetObject.GetComponentsInChildren<Renderer>(true))
            {
                materialsBackup.Add(mr, mr.sharedMaterial);
            }
        }


        /// <summary>
        /// Replace the materials of a game object using a previously stored map (see <see cref="BackupMaterials"/>)
        /// </summary>
        /// <param name="targetObject">Game object to scan</param>
        /// <param name="materialsBackup">Dictionary that maps renderers to their materials</param>
        /// <remarks>If the given dictionary does not match the renderers the behaviour is undefined.</remarks>
        public static void ReplaceMaterials(GameObject targetObject, MaterialMap materialsBackup)
        {
            foreach (Renderer mr in targetObject.GetComponentsInChildren<Renderer>(true))
            {
                if (materialsBackup.ContainsKey(mr))
                {
                    mr.sharedMaterial = materialsBackup[mr];
                }
            }
        }


        /// <summary>
        /// Replace the materials in a game object with a copy of each one of them.
        /// Both the original materials and the cloned materials are stored in the given dictionaries.
        /// </summary>
        /// <param name="targetObject">Game object to scan</param>
        /// <param name="originalMaterials">Dictionary that maps renderers to their original materials</param>
        /// <param name="newMaterials">Dictionary that maps renderers to their cloned materials</param>
        /// <remarks>The given dictionaries are cleaned before they are filled.</remarks>
        public static void CloneMaterials(GameObject targetObject, MaterialMap originalMaterials, MaterialMap newMaterials)
        {
            originalMaterials.Clear();
            newMaterials.Clear();
            foreach (Renderer mr in targetObject.GetComponentsInChildren<Renderer>(true))
            {
                originalMaterials.Add(mr,mr.sharedMaterial);
                mr.sharedMaterial = Material.Instantiate(mr.sharedMaterial);
                newMaterials.Add(mr,mr.sharedMaterial);
            }
        }

        /// <summary>
        /// Change the color (also albedo and emission) of each material of a game object.
        /// </summary>
        /// <param name="targetObject">Game object to which colors are changed</param>
        /// <param name="newColor">Color to set for each material</param>
        /// <param name="affectMainColor">Affect main color</param>
        /// <param name="affectEmission">Affect Emissive color (if available)</param>
        /// <remarks>Original materials are changed here, not their copies</remarks>
        public static void ChangeColor(GameObject targetObject, Color newColor, bool affectMainColor, bool affectEmission)
        {
            foreach (Renderer mr in targetObject.GetComponentsInChildren<Renderer>(true))
            {
                ChangeColor(mr.sharedMaterial, newColor, affectMainColor, affectEmission);
            }
        }


        /// <summary>
        /// Change the color (also albedo and emission) of each material of a game object.
        /// </summary>
        /// <param name="targetObject">Game object to which colors are changed</param>
        /// <param name="newMaterial">Material tha replaces all present materials</param>
        /// <remarks>Original materials are changed here, not their copies</remarks>
        public static void ReplaceMaterials(GameObject targetObject, Material newMaterial)
        {
            foreach (Renderer mr in targetObject.GetComponentsInChildren<Renderer>(true))
            {
                mr.sharedMaterial = newMaterial;
            }
        }

        /// <summary>
        /// Change the color (also albedo and emission) of a material.
        /// </summary>
        /// <param name="targetMaterial">Material to which colors are changed</param>
        /// <param name="newColor">Color to set for each material</param>
        /// <param name="affectMainColor">Affect main color</param>
        /// <param name="affectEmission">Affect Emissive color (if available)</param>
        public static void ChangeColor(Material targetMaterial, Color newColor, bool affectMainColor, bool affectEmission)
        {
            targetMaterial.shader = Shader.Find("Standard");
            targetMaterial.color = newColor;
            if (affectMainColor && targetMaterial.HasProperty("_Color"))
            {
                targetMaterial.SetColor("_Color", newColor);
            }
            if (affectEmission && targetMaterial.HasProperty("_EmissionColor"))
            {
                targetMaterial.SetColor("_EmissionColor", newColor);
            }
            if(newColor.a<1f)
            {
                PrepareMaterialRenderingMode(targetMaterial, RenderingMode.FADE);
            }
        }


        /// <summary>
        /// Create a Standard Material (Fade) with the given color.
        /// </summary>
        /// <param name="materialColor">Main material color</param>
        /// <returns></returns>
        public static Material CreateColoredMaterial(Color materialColor)
        {
            Material mtl = new Material(Shader.Find("Standard"));
            PrepareMaterialRenderingMode(mtl, RenderingMode.FADE);
            mtl.color = materialColor;
            return mtl;
        }


        /// <summary>
        /// Prepare a Standard Material for the given mode.
        /// </summary>
        /// <remarks>Here is replicated what is done when choosing a blend mode from Inspector.</remarks>
        /// <param name="standardMaterial">material to be changed (assumed to have a "Standard" shader)</param>
        /// <param name="mode">mode to be set</param>
        public static void PrepareMaterialRenderingMode(Material standardMaterial, RenderingMode mode)
        {
            switch (mode)
            {
                case RenderingMode.OPAQUE:
                    standardMaterial.SetOverrideTag("RenderType", "Opaque");
                    standardMaterial.SetFloat("_Mode", 0);
                    standardMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
                    standardMaterial.SetInt("_DstBlend", (int)BlendMode.Zero);
                    standardMaterial.SetInt("_ZWrite", 1);
                    standardMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardMaterial.renderQueue = -1;
                    break;
                case RenderingMode.CUTOUT:
                    standardMaterial.SetOverrideTag("RenderType", "TransparentCutout");
                    standardMaterial.SetFloat("_Mode", 1);
                    standardMaterial.SetFloat("_Mode", 1);
                    standardMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
                    standardMaterial.SetInt("_DstBlend", (int)BlendMode.Zero);
                    standardMaterial.SetInt("_ZWrite", 1);
                    standardMaterial.EnableKeyword("_ALPHATEST_ON");
                    standardMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardMaterial.renderQueue = 2450;
                    break;
                case RenderingMode.FADE:
                    standardMaterial.SetOverrideTag("RenderType", "Transparent");
                    standardMaterial.SetFloat("_Mode", 2);
                    standardMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    standardMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    standardMaterial.SetInt("_ZWrite", 0);
                    standardMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardMaterial.EnableKeyword("_ALPHABLEND_ON");
                    standardMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardMaterial.renderQueue = 3000;
                    break;
                case RenderingMode.TRANSPARENT:
                    standardMaterial.SetOverrideTag("RenderType", "Transparent");
                    standardMaterial.SetFloat("_Mode", 3);
                    standardMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
                    standardMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    standardMaterial.SetInt("_ZWrite", 0);
                    standardMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardMaterial.renderQueue = 3000;
                    break;
            }
        }

    }

}
