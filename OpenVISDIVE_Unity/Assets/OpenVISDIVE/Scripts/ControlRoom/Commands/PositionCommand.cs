using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace OpenVISDIVE
{
    [System.Serializable]
    [XmlType(TypeName = "cmdPosition")]
    public class PositionCommand : ObjectCommand
    {
        public Vector3 position;

        public Vector3 eulerAngles;

        protected Vector3 previousPosition;

        protected Vector3 previousEulerAngles;

        public PositionCommand(ScenarioController scenarioController, GameObject targetObject,
            Vector3 prevPosition, Vector3 prevEulerAngles, Vector3 position, Vector3 eulerAngles) : base(scenarioController, targetObject)
        {
            this.previousPosition = prevPosition;
            this.previousEulerAngles = prevEulerAngles;
            this.position = position;
            this.eulerAngles = eulerAngles;
        }

        protected override void Execute()
        {
            targetObject.transform.position = position;
            targetObject.transform.eulerAngles = eulerAngles;
        }

        protected override void Restore()
        {
            targetObject.transform.position = previousPosition;
            targetObject.transform.eulerAngles = previousEulerAngles;
        }
    }
}
