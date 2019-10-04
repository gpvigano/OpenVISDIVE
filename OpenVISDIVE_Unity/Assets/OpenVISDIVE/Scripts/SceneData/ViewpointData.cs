using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace OpenVISDIVE
{
    [System.Serializable]
    public class ViewpointData
    {
        public string name;
        public Vector3 position;
        public Vector3 eulerAngles;
    }
}
