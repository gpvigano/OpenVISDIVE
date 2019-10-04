using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace OpenVISDIVE
{
    [System.Serializable]
    [XmlType(TypeName = "cmdDestroyEntity")]
    public class DestroyEntityCommand : ObjectCommand
    {
        public DestroyEntityCommand(ScenarioController scenarioController, GameObject targetObject) : base(scenarioController, targetObject)
        {
        }

        protected override void Execute()
        {
            scenarioController.DeleteEntity(targetObject);
        }

        protected override void Restore()
        {
            throw new NotImplementedException();
        }
    }
}
