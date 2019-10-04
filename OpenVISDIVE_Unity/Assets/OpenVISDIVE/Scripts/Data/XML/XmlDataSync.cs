using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenVISDIVE
{
    public class XmlDataSync : DataSync
    {
#if UNITY_EDITOR
        [Header("Debugging")]
        [Tooltip("Delay added for debugging the loading process (only in Editor)")]
        public float progressDelay = 0f;
#endif

        [Header("Data")]
        public VrXmlSceneData scenarioData = null;

        /// <summary>
        /// Bound to VrXmlSceneData.upAxisIsZ
        /// </summary>
        public override bool OriginalUpAxisIsZ
        {
            get
            {
                if (scenarioData != null)
                {
                    return scenarioData.upAxisIsZ;
                }
                return false;
            }
            set
            {
                if (scenarioData != null)
                {
                    scenarioData.upAxisIsZ = value;
                }
            }
        }

        protected override void InitScenarioData()
        {
            if (scenarioData == null)
            {
                scenarioData = new VrXmlSceneData();
            }
        }

        public override string[] ActiveVariants
        {
            get
            {
                if (scenarioData == null)
                {
                    return null;
                }
                return scenarioData.activeVariants.ToArray();
            }
        }

        public override int EntityCount
        {
            get
            {
                return scenarioData!=null ? scenarioData.entityList.Count : 0;
            }
        }


        public override bool EntityInScenario(string entityId)
        {
            if (scenarioData == null)
            {
                return false;
            }
            return scenarioData.entityList.Exists(x => x.id == entityId);
        }

        public override bool ReadScenario(string url)
        {
            return DataUtil.ReadFromXml(ref scenarioData, url);
        }

        public override bool WriteScenario(string url)
        {
            if (scenarioData == null)
            {
                Debug.LogWarning(GetType() + " - No data to write.");
                return false;
            }
            return DataUtil.WriteToXml(scenarioData, url);
        }


        /// <summary>
        /// Update the given game object with the given XML entity data
        /// </summary>
        /// <param name="destGameObject">The given game object to update</param>
        /// <param name="data">The given source XML entity data</param>
        protected void UpdateObject(GameObject destGameObject, VrXmlEntityData data)
        {
            destGameObject.name = data.name;
            EntityData entityData = destGameObject.GetComponent<EntityData>();
            if (entityData == null)
            {
                entityData = destGameObject.AddComponent<EntityData>();
            }
            entityData.id = data.id;
            entityData.name = data.name;
            entityData.type = data.type;
            entityData.description = data.description;

            if (!string.IsNullOrEmpty(data.localTransform.relToId))
            {
                destGameObject.transform.SetParent(entityObjects[data.localTransform.relToId].transform, false);
            }
            else
            {
                destGameObject.transform.SetParent(null, false);
            }

            destGameObject.transform.localPosition = CsConversion.VecToVecRL(data.localTransform.position.Get(), scenarioData.upAxisIsZ);
            if (data.localTransform.useRotationMatrix)
            {
                Matrix4x4 rotMatrix = (Matrix4x4)(data.localTransform.rotationMatrix);
                destGameObject.transform.localRotation.SetLookRotation(
                    CsConversion.RotMatForward(rotMatrix, scenarioData.upAxisIsZ),
                    CsConversion.RotMatUp(rotMatrix, scenarioData.upAxisIsZ));
            }
            else
            {
                destGameObject.transform.localEulerAngles = (Vector3)data.localTransform.eulerAngles;
            }
            destGameObject.transform.localScale = data.localTransform.scale.Get();
        }

        protected VrXmlRelationship ConvertObjectRelationship(Relationship rel)
        {
            VrXmlRelationship xmlRel = null;
            var oneToOne = rel as OneToOneRelationship;
            var oneToMany = rel as OneToManyRelationship;
            var manyToMany = rel as ManyToManyRelationship;
            if (rel is CompositionRelationship)
            {
                xmlRel = new VrXmlCompositionRelationship();
            }
            else if (rel is InclusionRelationship)
            {
                xmlRel = new VrXmlInclusionRelationship();
            }
            else if (rel is OneToManyRelationship)
            {
                xmlRel = new VrXmlOneToOneRelationship();
            }
            else if (rel is OneToManyRelationship)
            {
                xmlRel = new VrXmlOneToManyRelationship();
            }
            else if (rel is ManyToManyRelationship)
            {
                xmlRel = new VrXmlManyToManyRelationship();
            }
            var xmlOneToOne = xmlRel as VrXmlOneToOneRelationship;
            var xmlOneToMany = xmlRel as VrXmlOneToManyRelationship;
            var xmlManyToMany = xmlRel as VrXmlManyToManyRelationship;
            if (oneToOne)
            {
                xmlOneToOne.subjectEntityId = oneToOne.subjectEntity.id;
                xmlOneToOne.objectEntityId = oneToOne.objectEntity.id;
            }
            else if (oneToMany)
            {
                xmlOneToMany.subjectEntityId = oneToMany.subjectEntity.id;
                xmlOneToMany.objectEntitiesId.AddRange(oneToMany.objectEntities.Select(ent => ent.id));
            }
            else if (manyToMany)
            {
                xmlManyToMany.subjectEntitiesId.AddRange(manyToMany.subjectEntities.Select(ent => ent.id));
                xmlManyToMany.objectEntitiesId.AddRange(manyToMany.objectEntities.Select(ent => ent.id));
            }
            xmlRel.id = rel.id;
            xmlRel.typeName = rel.typeName;
            xmlRel.ownerEntityId = rel.ownerEntity ? rel.ownerEntity.id : null;
            return xmlRel;
        }

        protected VrXmlHistoryData ConvertObjectHistory(EntityHistory history)
        {
            VrXmlHistoryData xmlHistory = new VrXmlHistoryData();
            xmlHistory.entityId = history.GetComponent<EntityData>().id;
            xmlHistory.fileName = history.historyFileName;
            xmlHistory.upAxisIsZ = history.sourceUpAxisIsZ;
            return xmlHistory;
        }

        protected VrXmlRepresentation.AssetType ConvertRepresentationPrimType(EntityRepresentation.AssetPrimType assetPrimType)
        {
            switch (assetPrimType)
            {
                case EntityRepresentation.AssetPrimType.Capsule:
                    return VrXmlRepresentation.AssetType.Capsule;
                case EntityRepresentation.AssetPrimType.Cube:
                    return VrXmlRepresentation.AssetType.Cube;
                case EntityRepresentation.AssetPrimType.Cylinder:
                    return VrXmlRepresentation.AssetType.Cylinder;
                case EntityRepresentation.AssetPrimType.Plane:
                    return VrXmlRepresentation.AssetType.Plane;
                case EntityRepresentation.AssetPrimType.Quad:
                    return VrXmlRepresentation.AssetType.Quad;
                case EntityRepresentation.AssetPrimType.Sphere:
                    return VrXmlRepresentation.AssetType.Sphere;
                case EntityRepresentation.AssetPrimType.None:
                    return VrXmlRepresentation.AssetType.None;
            }
            return VrXmlRepresentation.AssetType.None;
        }

        protected VrXmlRepresentation.AssetType ConvertMeshToAssetType(Mesh mesh)
        {
            if (mesh)
            {
                switch (mesh.name)
                {
                    case "Cube":
                        return VrXmlRepresentation.AssetType.Cube;
                    case "Sphere":
                        return VrXmlRepresentation.AssetType.Sphere;
                    case "Cylinder":
                        return VrXmlRepresentation.AssetType.Cylinder;
                    case "Capsule":
                        return VrXmlRepresentation.AssetType.Capsule;
                    case "Quad":
                        return VrXmlRepresentation.AssetType.Quad;
                    case "Plane":
                        return VrXmlRepresentation.AssetType.Plane;
                }
            }
            return VrXmlRepresentation.AssetType.None;
        }


        protected void UpdateEntity(GameObject sourceGameObject, VrXmlEntityData data)
        {
            bool zUp = OriginalUpAxisIsZ;
            var entityInfo = sourceGameObject.GetComponent<EntityData>();
            if (entityInfo == null)
            {
                Debug.LogWarning("Failed to get entity data from object " + sourceGameObject.name);
                return;
            }
            data.id = entityInfo.id;
            data.name = sourceGameObject.name;
            data.type = entityInfo.type;
            if (entityInfo.activeRepresentation)
            {
                if (data.representation == null)
                {
                    data.representation = new VrXmlRepresentation();
                }
                data.representation.assetBundleName = null;
                data.representation.assetName = null;
                data.representation.localTransform.FromTransform(entityInfo.activeRepresentation.transform, zUp);
                switch (entityInfo.activeRepresentation.assetType)
                {
                    case EntityRepresentation.AssetType.AssetBundle:
                        data.representation.assetType = VrXmlRepresentation.AssetType.AssetBundle;
                        data.representation.assetBundleName = entityInfo.activeRepresentation.assetBundleName;
                        data.representation.assetName = entityInfo.activeRepresentation.assetName;
                        break;
                    case EntityRepresentation.AssetType.Prefab:
                        data.representation.assetType = VrXmlRepresentation.AssetType.Prefab;
                        data.representation.assetName = entityInfo.activeRepresentation.assetName;
                        break;
                    case EntityRepresentation.AssetType.Model:
                        data.representation.assetType = VrXmlRepresentation.AssetType.Model;
                        data.representation.assetName = entityInfo.activeRepresentation.assetName;
                        data.representation.importOptions = entityInfo.activeRepresentation.importOptions;
                        break;
                    case EntityRepresentation.AssetType.Primitive:
                        data.representation.assetType = ConvertRepresentationPrimType(entityInfo.activeRepresentation.assetPrimType);
                        break;
                    case EntityRepresentation.AssetType.None:
                        data.representation.assetType = VrXmlRepresentation.AssetType.None;
                        break;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(entityInfo.assetName))
                {
                    if (data.representation == null)
                    {
                        data.representation = new VrXmlRepresentation();
                    }
                    if (!string.IsNullOrEmpty(entityInfo.assetBundleName))
                    {
                        data.representation.assetType = VrXmlRepresentation.AssetType.AssetBundle;
                    }
                    else
                    {
                        data.representation.assetType = VrXmlRepresentation.AssetType.Prefab;
                    }
                    data.representation.assetBundleName = entityInfo.assetBundleName;
                    data.representation.assetName = entityInfo.assetName;
                }
                else if (entityInfo.GetComponent<MeshFilter>())
                {
                    if (data.representation == null)
                    {
                        data.representation = new VrXmlRepresentation();
                    }
                    data.representation.assetBundleName = null;
                    data.representation.assetName = null;
                    Mesh mesh = entityInfo.GetComponent<MeshFilter>().sharedMesh;
                    data.representation.assetType = ConvertMeshToAssetType(mesh);
                }
            }

            data.description = entityInfo.description;
            data.localTransform.FromTransform(sourceGameObject.transform, zUp);
            scenarioData.relationshipList.RemoveAll(x => x.ownerEntityId == entityInfo.id);
            foreach (var rel in sourceGameObject.GetComponents<Relationship>())
            {
                scenarioData.relationshipList.Add(ConvertObjectRelationship(rel));
            }
        }

        protected Relationship ConvertEntityRelationship(VrXmlRelationship xmlRel)
        {
            GameObject ownerObject = string.IsNullOrEmpty(xmlRel.ownerEntityId) ? gameObject : entityObjects[xmlRel.ownerEntityId];
            Relationship rel = null;
            if (xmlRel is VrXmlCompositionRelationship)
            {
                var xmlOneToMany = xmlRel as VrXmlCompositionRelationship;
                rel = AddCompositionRelationship(xmlOneToMany.ownerEntityId, xmlOneToMany.subjectEntityId, xmlOneToMany.objectEntitiesId);
            }
            else if (xmlRel is VrXmlInclusionRelationship)
            {
                var xmlOneToMany = xmlRel as VrXmlInclusionRelationship;
                rel = AddInclusionRelationship(xmlOneToMany.ownerEntityId, xmlOneToMany.subjectEntityId, xmlOneToMany.objectEntitiesId);
            }
            else if (xmlRel is VrXmlOneToOneRelationship)
            {
                var xmlOneToOne = xmlRel as VrXmlOneToOneRelationship;
                rel = AddOneToOneRelationship(xmlOneToOne.ownerEntityId, xmlOneToOne.subjectEntityId, xmlOneToOne.objectEntityId);
            }
            else if (xmlRel is VrXmlOneToManyRelationship)
            {
                var xmlOneToMany = xmlRel as VrXmlOneToManyRelationship;
                rel = AddOneToManyRelationship(xmlOneToMany.ownerEntityId, xmlOneToMany.subjectEntityId, xmlOneToMany.objectEntitiesId);
            }
            else if (xmlRel is VrXmlManyToManyRelationship)
            {
                var xmlManyToMany = xmlRel as VrXmlManyToManyRelationship;
                rel = AddManyToManyRelationship(xmlManyToMany.ownerEntityId, xmlManyToMany.subjectEntitiesId, xmlManyToMany.objectEntitiesId);
            }

            //// update cross references
            //rel.OnValidate();
            return rel;
        }

        protected EntityHistory ConvertEntityHistory(VrXmlHistoryData xmlHistory)
        {
            GameObject ownerObject = string.IsNullOrEmpty(xmlHistory.entityId) ? gameObject : entityObjects[xmlHistory.entityId];
            EntityHistory history = ownerObject.AddComponent<EntityHistory>();
            history.historyFileName = xmlHistory.fileName;
            history.sourceUpAxisIsZ = xmlHistory.upAxisIsZ;
            return history;
        }

        protected PrimitiveType ConvertAssetTypeToPrimType(VrXmlRepresentation.AssetType assetType)
        {
            switch (assetType)
            {
                case VrXmlRepresentation.AssetType.Cube:
                    return PrimitiveType.Cube;
                case VrXmlRepresentation.AssetType.Sphere:
                    return PrimitiveType.Sphere;
                case VrXmlRepresentation.AssetType.Cylinder:
                    return PrimitiveType.Cylinder;
                case VrXmlRepresentation.AssetType.Capsule:
                    return PrimitiveType.Capsule;
                case VrXmlRepresentation.AssetType.Quad:
                    return PrimitiveType.Quad;
                case VrXmlRepresentation.AssetType.Plane:
                    return PrimitiveType.Plane;
            }
            Debug.LogError("Asset type " + assetType.ToString() + " not mapped to a primitive.");
            return PrimitiveType.Cube;
        }

        protected EntityRepresentation.AssetPrimType ConvertAssetTypeToAssetPrimType(VrXmlRepresentation.AssetType assetType)
        {
            switch (assetType)
            {
                case VrXmlRepresentation.AssetType.Cube:
                    return EntityRepresentation.AssetPrimType.Cube;
                case VrXmlRepresentation.AssetType.Sphere:
                    return EntityRepresentation.AssetPrimType.Sphere;
                case VrXmlRepresentation.AssetType.Cylinder:
                    return EntityRepresentation.AssetPrimType.Cylinder;
                case VrXmlRepresentation.AssetType.Capsule:
                    return EntityRepresentation.AssetPrimType.Capsule;
                case VrXmlRepresentation.AssetType.Quad:
                    return EntityRepresentation.AssetPrimType.Quad;
                case VrXmlRepresentation.AssetType.Plane:
                    return EntityRepresentation.AssetPrimType.Plane;
            }
            Debug.LogError("Asset type " + assetType.ToString() + " not mapped to an asset primitive type.");
            return EntityRepresentation.AssetPrimType.None;
        }

        protected IEnumerator CreateRepresentationObject(EntityData entityData, VrXmlRepresentation representation)
        {
            string representationId = representation.id;
            string representationAssetName = representation.assetName;
            string representationAssetBundleName = representation.assetBundleName;
            EntityRepresentation.AssetType representationAssetType = EntityRepresentation.AssetType.None;
            EntityRepresentation.AssetPrimType representationAssetPrimType = EntityRepresentation.AssetPrimType.None;
            ModelImportOptions importOptions = representation.importOptions;
            switch (representation.assetType)
            {
                case VrXmlRepresentation.AssetType.Model:
                    representationAssetType = EntityRepresentation.AssetType.Model;
                    break;
                case VrXmlRepresentation.AssetType.AssetBundle:
                case VrXmlRepresentation.AssetType.Prefab:
                    representationAssetType = EntityRepresentation.AssetType.Prefab;
                    break;
                case VrXmlRepresentation.AssetType.Cube:
                case VrXmlRepresentation.AssetType.Sphere:
                case VrXmlRepresentation.AssetType.Cylinder:
                case VrXmlRepresentation.AssetType.Capsule:
                case VrXmlRepresentation.AssetType.Quad:
                case VrXmlRepresentation.AssetType.Plane:
                    representationAssetType = EntityRepresentation.AssetType.Primitive;
                    break;
            }
            if (representationAssetType == EntityRepresentation.AssetType.Primitive)
            {
                representationAssetPrimType = ConvertAssetTypeToAssetPrimType(representation.assetType);
            }

            yield return CreateRepresentationObject(
                entityData,
                representationId,
                representationAssetName,
                representationAssetBundleName,
                representationAssetType,
                representationAssetPrimType,
                importOptions);
        }


        protected override IEnumerator UpdateScene()
        {
            Debug.Log("Updating scenario...");
            // scan each data info in the scenario and add missing objects in the scene
            foreach (VrXmlEntityData entity in scenarioData.entityList)
            {
                if (!entityObjects.ContainsKey(entity.id))
                {
                    GameObject go = new GameObject();//GameObject.CreatePrimitive(PrimitiveType.Cube);
                    EntityData entityData = go.AddComponent<EntityData>();
                    entityData.id = entity.id;
                    entityObjects.Add(entity.id, go);
                }
                if (entity.representation != null)
                {
                    representationCount++;
                }
            }
            yield return new WaitForSecondsRealtime(0.2f);

            Debug.Log("Updating entities...");
            // scan each data info in the scenario and (add and) update objects in the scene
            foreach (VrXmlEntityData entity in scenarioData.entityList)
            {
                if (entityObjects.ContainsKey(entity.id))
                {
                    UpdateObject(entityObjects[entity.id], entity);
#if UNITY_EDITOR
                    if (progressDelay > 0) yield return new WaitForSecondsRealtime(progressDelay);
#endif

                    if (entity.representation != null)
                    {
                        yield return CreateRepresentationObject(entityObjects[entity.id].GetComponent<EntityData>(), entity.representation);
                    }
                    updatedEntities++;
                }
            }

            Debug.Log("Updating relationships...");
            // scan each relationship in the scenario and (add and) update components in the scene
            foreach (VrXmlRelationship entity in scenarioData.relationshipList)
            {
                ConvertEntityRelationship(entity);
            }

            if (scenarioData.simulation != null)
            {
                Debug.Log("Loading simulation...");
                SimController simController = GetComponent<SimController>();
                if (simController == null)
                {
                    simController = gameObject.AddComponent<SimController>();
                }
                simController.simulationHistory.historyUri = scenarioData.simulation.historyUri;
                simController.simulationHistory.historyName = scenarioData.simulation.historyName;
                simController.simulationHistory.description = scenarioData.simulation.description;
                simController.simulationHistory.details = scenarioData.simulation.details;
                // scan each history in the scenario and (add and) update components in the scene
                foreach (VrXmlHistoryData history in scenarioData.simulation.historyList)
                {
                    ConvertEntityHistory(history);
                }
                simController.LoadSimulation();
                if (scenarioData.simulation.autoStart)
                {
                    simController.PlaySimulation();
                }
            }
            Debug.Log("Scenario updated.");
        }

        protected override IEnumerator UpdateScenario()
        {
            UpdateScenarioData();
            yield return null;
        }

        public override void ClearScenarioData()
        {
            if (scenarioData != null)
            {
                scenarioData.entityList.Clear();
                scenarioData.relationshipList.Clear();
                if (scenarioData.simulation != null && scenarioData.simulation.historyList != null)
                {
                    scenarioData.simulation.historyList.Clear();
                }
            }
        }

        public override void UpdateScenarioData()
        {
            UpdateEntityObjectsList();
            ClearScenarioData();
            SimController simController = GetComponent<SimController>();
            if (simController)
            {
                simController.StopSimulation();
                if (scenarioData.simulation == null)
                {
                    scenarioData.simulation = new VrXmlSimulationData();
                }
                scenarioData.simulation.historyUri = simController.simulationHistory.historyUri;
                scenarioData.simulation.historyName = simController.simulationHistory.historyName;
                scenarioData.simulation.description = simController.simulationHistory.description;
                scenarioData.simulation.details = simController.simulationHistory.details;
            }

            foreach (var entry in entityObjects)
            {
                var data = scenarioData.entityList.Find(x => x.id == entry.Key);
                if (data == null)
                {
                    data = new VrXmlEntityData();
                    scenarioData.entityList.Add(data);
                }
                UpdateEntity(entry.Value, data);
                // load simulation
                EntityHistory history = entry.Value.GetComponent<EntityHistory>();
                if (history)
                {
                    scenarioData.simulation.historyList.Add(ConvertObjectHistory(history));
                }
            }

        }
    }
}
