using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace OpenVISDIVE
{
    [System.Serializable]
    [XmlInclude(typeof(ObjectCommand))]
    [XmlInclude(typeof(PositionCommand))]
    [XmlInclude(typeof(CreateEntityCommand))]
    [XmlInclude(typeof(DestroyEntityCommand))]
    public abstract class Command
    {
        public string name;

        public bool TryExecute()
        {
            if(!Validate())
            {
                return false;
            }
            Execute();
            return true;
        }

        public bool TryRestore()
        {
            if(!Validate())
            {
                return false;
            }
            Restore();
            return true;
        }

        abstract protected bool Validate();
        abstract protected void Execute();
        abstract protected void Restore();
    }
}
