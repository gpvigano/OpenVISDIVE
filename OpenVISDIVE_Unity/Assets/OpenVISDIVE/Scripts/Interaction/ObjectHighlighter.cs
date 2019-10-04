using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenVISDIVE
{
    using MaterialMap = Dictionary<Renderer, Material>;
    public class ObjectHighlighter : MonoBehaviour
    {
        private Dictionary<GameObject, MaterialMap> highlightedObjects = new Dictionary<GameObject, MaterialMap>();

        void Update()
        {

        }

        public void HighlightObject(GameObject obj, bool highlighted, Color hlColor)
        {
            if (obj == null)
            {
                throw new NullReferenceException();
            }
            bool alreadyHighlighted = highlightedObjects.ContainsKey(obj);
            UnhighlightAll(obj);
            // unhighlight all
            if (highlighted && !alreadyHighlighted)
            {
                MaterialMap origMaterialMap = new MaterialMap();
                MaterialMap highlightMaterialMap = new MaterialMap();
                RenderingUtility.CloneMaterials(obj, origMaterialMap, highlightMaterialMap);
                RenderingUtility.ChangeColor(obj, hlColor, true, true);
                highlightedObjects.Add(obj, origMaterialMap);
            }

            if (!highlighted && highlightedObjects.ContainsKey(obj))
            {
                RenderingUtility.ReplaceMaterials(obj, highlightedObjects[obj]);
                highlightedObjects[obj].Clear();
                highlightedObjects.Remove(obj);
            }
        }


        public void UnhighlightAll(GameObject excludedObj = null)
        {
            if (highlightedObjects.Count > 0)
            {
                MaterialMap origMaterialMap = null;
                foreach (var item in highlightedObjects)
                {
                    if (item.Key == excludedObj && excludedObj != null)
                    {
                        origMaterialMap = item.Value;
                    }
                    if (item.Key != excludedObj && item.Key != null && item.Value != null)
                    {
                        RenderingUtility.ReplaceMaterials(item.Key, item.Value);
                    }
                }
                highlightedObjects.Clear();
                if (excludedObj != null && origMaterialMap != null)
                {
                    highlightedObjects.Add(excludedObj, origMaterialMap);
                }
            }
        }
    }
}
