using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GLTF.Schema;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityGLTF.Extensions;

namespace UnityGLTF
{
    public class KHR_materials_unlit : IExtension
    {
        public IExtension Clone(GLTFRoot root)
        {
            return new KHR_materials_unlit();
        }

        public JProperty Serialize()
        {
            JProperty jProperty =
                new JProperty("KHR_materials_unlit");

            return jProperty;
        }
    }
}
