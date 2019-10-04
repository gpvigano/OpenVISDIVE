
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace OpenVISDIVE
{
    [System.Serializable]
    [XmlType(TypeName = "cmdCreateEntity")]
    public class CreateEntityCommand : ObjectCommand
    {
        public GameObject createdObject = null;

        public CreateEntityCommand(ScenarioController scenarioController, GameObject targetObject) : base(scenarioController, targetObject)
        {
        }

        protected override void Execute()
        {
            createdObject = scenarioController.CreateEntity(targetObject);
        }


        protected override void Restore()
        {
            scenarioController.DeleteEntity(createdObject);
        }
    }
}
