using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenVISDIVE
{
    public abstract class ObjectCommand : Command
    {
        public ScenarioController scenarioController = null;
        public GameObject targetObject = null;


        public ObjectCommand(ScenarioController scenarioController, GameObject targetObject)
        {
            this.scenarioController = scenarioController;
            this.targetObject = targetObject;
        }


        protected override bool Validate()
        {
            return (scenarioController != null && targetObject != null);
        }
    }
}
