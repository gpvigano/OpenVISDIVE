using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
//using AssetBundles;

/// <summary>
/// Virtual Reality Simulation Toolkit main namespace.
/// </summary>
namespace OpenVISDIVE
{
    public abstract class DataSync : MonoBehaviour
    {

        [Header("Paths", order = 10)]

        [Tooltip("Scenario data path (relative to data path, or persistent data path for mobile platform)")]
        public string scenarioDataPath = "../";

        [Tooltip("Scenario file for data storage")]
        public string scenarioDataFile = "../scenario.xml";

        /// <summary>
        /// Return true if the entity data has Z as vertical axis.
        /// </summary>
        /// <remarks>This must be bound to the data source settings.</remarks>
        public abstract bool OriginalUpAxisIsZ
        {
            get;
            set;
        }

        public virtual float Progress
        {
            get
            {
                if (loadingSingleRepresentation)
                {
                    return Busy ? 0 : 1f;
                }
                int entityCount = EntityCount;
                if (!Busy || entityCount == 0)
                {
                    return 0;
                }
                float updateProgress = updatedEntities / (float)entityCount;
                if (representationCount > 0)
                {
                    float representationProgress = createdRepresentations / (float)representationCount;
                    return representationProgress * 0.5f + updateProgress * 0.5f;
                }
                else
                {
                    return updateProgress;
                }
            }
        }

        public bool Busy { get { return isBusy; } }

        public string ScenarioFullPath { get; set; } = null;

        protected Dictionary<string, GameObject> entityObjects = new Dictionary<string, GameObject>();

        protected bool isBusy = false;
        protected int createdRepresentations = 0;
        protected int updatedEntities = 0;
        protected int representationCount = 0;
        protected bool loadingSingleRepresentation = false;

        private bool assetManagerInitialized = false;

        /// <summary>
        /// Event triggered <b>before</b> loading the scenario
        /// </summary>
        public event Action StartLoadingScenario;

        /// <summary>
        /// Event triggered <b>after</b> the scenario has been loaded
        /// </summary>
        public event Action ScenarioLoaded;
        ////public event Action OnScenarioUnloaded;
        ////public event Action OnSavingScenario;
        ////public event Action OnScenarioSaved;

        public event Action StartLoadingRepresentation;
        public event Action RepresentationLoaded;

        /// <summary>
        /// Get the list of variants as read from the data source
        /// </summary>
        public abstract string[] ActiveVariants { get; }

        public abstract int EntityCount { get; }

        /// <summary>
        /// Clear scenario data without affecting the scene
        /// </summary>
        public abstract void ClearScenarioData();

        /// <summary>
        /// Check if the entity with the given identifier is defined in the scenario data
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public abstract bool EntityInScenario(string entityId);


        /// <summary>
        /// Get the current scenario full path
        /// </summary>
        /// <returns>Full path (inside the data or prsistent dat path)</returns>
        public string GetScenarioPath()
        {
            if (!string.IsNullOrEmpty(ScenarioFullPath))
            {
                return ScenarioFullPath;
            }
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            string data_path = Application.persistentDataPath;
#else
            string data_path = Application.dataPath;
#endif
            string path = data_path + "/" + scenarioDataPath;
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            path += scenarioDataFile;
            return path;
        }

        /// <summary>
        /// Load the scenario data from the currently defined location and update the scene
        /// </summary>
        public virtual void LoadDataToScene()
        {
            if (string.IsNullOrEmpty(scenarioDataFile))
            {
                Debug.LogWarning("Project URL not defined");
                return;
            }
            StartCoroutine(LoadScenario());
        }

        /// <summary>
        /// Update the scenario data and save it to the currently defined location
        /// </summary>
        public virtual void SaveDataFromScene()
        {
            if (string.IsNullOrEmpty(scenarioDataFile))
            {
                Debug.LogWarning("Project URL not defined");
                return;
            }
            InitScenarioData();
            StartCoroutine(SaveScenario());
        }

        /// <summary>
        /// Read the scenario data from the currently defined location
        /// </summary>
        public bool ReadScenario()
        {
            if (!ReadScenario(GetScenarioPath()))
            {
                Debug.LogWarning("Failed to load scenario from " + GetScenarioPath());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Read the scenario data from the given URL or file name
        /// </summary>
        /// <param name="url">URL (or file name)</param>
        /// <returns>True on success, false on error.</returns>
        public abstract bool ReadScenario(string url);

        /// <summary>
        /// Update the scenario data from the scene components
        /// </summary>
        public abstract void UpdateScenarioData();

        /// <summary>
        /// Write the scenario data to the currently defined location
        /// </summary>
        public bool WriteScenario()
        {
            if (!WriteScenario(GetScenarioPath()))
            {
                Debug.LogWarning("Failed to save scenario to " + GetScenarioPath());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Destroy all the entity objects in the scene (and their children)
        /// </summary>
        public void CleanUpScene()
        {
            foreach (var entry in entityObjects)
            {
                Destroy(entry.Value);
            }
            entityObjects.Clear();
            createdRepresentations = 0;
            updatedEntities = 0;
            representationCount = 0;
        }

        /*
        public void CleanUpScene(bool keepExisting)
        {
            if (!keepExisting)
            {
                foreach (var entry in entityObjects)
                {
                    Destroy(entry.Value);
                }
                entityObjects.Clear();
                return;
            }
            // the following code is not used for now
            // It would be useful to update objects without destroying and recreating them

            // create a list of identifiers initially filled with all those already assigned
            // to objects in the scene, then the identifiers present in the scenario are removed,
            // in this way we get only the identifiers that must be removed from the scene
            var deadObjectIdList = new List<string>();
            foreach (var entry in entityObjects)
            {
                if (!EntityInScenario(entry.Key))
                {
                    deadObjectIdList.Add(entry.Key);
                }
            }

            // delete objects not present in the scenario
            // objects attached to another object that will be destroyed will be destroyed along with it
            foreach (var id in deadObjectIdList)
            {
                bool destroy = true;
                if (entityObjects[id].transform.parent)
                {
                    var parentEntities = entityObjects[id].transform.parent.GetComponentsInParent<EntityData>();
                    foreach (var entity in parentEntities)
                    {
                        if (deadObjectIdList.Contains(entity.id))
                        {
                            destroy = false;
                            break;
                        }
                    }
                }
                // do not destroy children (?)
                //entityObjects[id].transform.DetachChildren();
                //contentObjects[id].SetActive(false);
                // remove the object from the scene and schedule it for destruction
                if (destroy)
                {
                    Destroy(entityObjects[id]);
                }
                // update the related id-object mapping
                entityObjects.Remove(id);
            }
        }
        */


        /// <summary>
        /// Write the scenario data to the given URL or file name
        /// </summary>
        /// <param name="url">URL (or file name)</param>
        /// <returns>True on success, false on error.</returns>
        public abstract bool WriteScenario(string url);


        public IEnumerator LoadRepresentation(EntityData entityInfo)
        {
            isBusy = true;
            loadingSingleRepresentation = true;
            Debug.Log("Loading representation...");
            OnStartLoadingRepresentation();

            yield return new WaitForEndOfFrame();

            EntityRepresentation representation = entityInfo.activeRepresentation;
            if (representation != null)
            {
                switch (entityInfo.activeRepresentation.assetType)
                {
                    case EntityRepresentation.AssetType.Model:
                        ModelLoader importer = GetComponent<ModelLoader>();
                        if (importer == null)
                        {
                            importer = gameObject.AddComponent<ModelLoader>();
                            //importer.CreatedModel += OnModelCreated;
                            //importer.ImportedModel += OnModelImported;
                            //importer.ImportError += OnModelError;
                        }
                        importer.ImportModelAsync(representation.name, representation.assetName, representation.transform, representation.importOptions);
                        while (importer.Running)
                        {
                            yield return null;
                        }
                        break;
                    case EntityRepresentation.AssetType.AssetBundle:
                    case EntityRepresentation.AssetType.Prefab:
                    case EntityRepresentation.AssetType.Primitive:
                    case EntityRepresentation.AssetType.None:
                        //throw new NotImplementedException();
                        break;
                }

                yield return new WaitForEndOfFrame();
                Debug.Log("Representation loaded.");
                loadingSingleRepresentation = false;
                isBusy = false;

                OnRepresentationLoaded();
            }
        }


        protected virtual void OnStartLoadingRepresentation()
        {
            StartLoadingRepresentation?.Invoke();
        }


        protected virtual void OnRepresentationLoaded()
        {
            RepresentationLoaded?.Invoke();
        }

        /// <summary>
        /// Initialize scenario data (create it if not defined)
        /// </summary>
        protected abstract void InitScenarioData();

        /// <summary>
        /// Update the scene from the scenario data, if available
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected abstract IEnumerator UpdateScene();

        /// <summary>
        /// Update the scenario data from the scene components
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected abstract IEnumerator UpdateScenario();

        /// <summary>
        /// Load the scenario data from the currently defined location and update the scene
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected virtual IEnumerator LoadScenario()
        {
            isBusy = true;
            Debug.Log("Loading scenario...");
            if (StartLoadingScenario != null)
            {
                StartLoadingScenario();
            }
            yield return Initialize();
            Debug.Log("  Updating entities...");
            UpdateEntityObjectsList();
            Debug.Log("  Reading scenario...");
            if (!ReadScenario())
            {
                Debug.LogError("  Error reading scenario.");
                yield break;
            }

            CleanUpScene();
            yield return StartCoroutine(UpdateScene());
            //yield return UpdateScene();

            // refresh all the reflection probes in the scene that are set to be updated via scripting
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                var probes = obj.GetComponentsInChildren<ReflectionProbe>();
                foreach (var probe in probes)
                {
                    if (probe.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting)
                    {
                        probe.RenderProbe();
                    }
                    yield return null;
                }
            }
            isBusy = false;
            if (ScenarioLoaded != null)
            {
                ScenarioLoaded();
            }
        }


        /// <summary>
        /// Update the scenario data from the scene and save it to the currently defined location
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected virtual IEnumerator SaveScenario()
        {
            //yield return Initialize();
            UpdateScenarioData();
            //StartCoroutine(UpdateScenario());
            WriteScenario();
            yield return null;
        }

        /// <summary>
        /// Update the internal cache mapping the entity id to the related game object
        /// </summary>
        /// <param name="obj">The game object (including children) to be mapped</param>
        protected void UpdateEntityObjectsListFromObject(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            EntityData entity = obj.GetComponent<EntityData>();
            if (entity)
            {
                entityObjects.Add(entity.id, obj);
            }
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                UpdateEntityObjectsListFromObject(obj.transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Update the whole internal cache mapping the entity id to the related game object
        /// </summary>
        protected void UpdateEntityObjectsList()
        {
            entityObjects.Clear();
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                UpdateEntityObjectsListFromObject(obj);
            }
        }

        protected virtual IEnumerator Start()
        {
            //yield return Initialize();
            yield return StartCoroutine(Initialize());

            ////// list of variants (aka representation contexts)
            ////// if the first one is not available try the second one and so on
            ////string[] activeVariants = new string[2];
            ////activeVariants[0] = "context2";
            ////activeVariants[1] = "context1";

            ////// Set active variants.
            ////AssetBundleManager.ActiveVariants = activeVariants;
        }

        /// <summary>
        /// Initialize the asset bundle manager
        /// </summary>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected IEnumerator Initialize()
        {
            if (assetManagerInitialized)
            {
                yield break;
            }

            // TODO: implement initialization
            assetManagerInitialized = true;
        }

        /// <summary>
        /// Instantiate a new game object in an asynchronous way.
        /// </summary>
        /// <param name="assetBundleName">Asset bundle name</param>
        /// <param name="assetName">Asset name inside the asset bundle</param>
        /// <param name="reprId">representation identifier to be used for the EntityRepresentation component</param>
        /// <param name="entityId">Entity identifier to be used for the EntityData component</param>
        /// <returns>It must be called iteratively (e.g. with StartCoroutine())</returns>
        protected IEnumerator InstantiateGameObjectAsync(string assetBundleName, string assetName, string reprId, string entityId)
        {
            // This is simply to get the elapsed time for this phase of AssetLoading.
            float startTime = Time.realtimeSinceStartup;

            // TODO: implement instantiation from AddressableAsset
            // Load asset from assetBundle.
            //GameObject prefab = ...; 

            // Get the asset.
            UnityEngine.Object prefab = Resources.Load("AddressableAssets/" + assetBundleName + "/" + assetName);

            if (prefab != null)
            {
                if (!entityObjects.ContainsKey(entityId))
                {
                    Debug.LogWarning("Asset " + assetName + " not found.");
                    yield break;
                }
                GameObject go = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
                go.transform.SetParent(entityObjects[entityId].transform, false);
                if (string.IsNullOrEmpty(reprId))
                {
                    reprId = DataUtil.CreateNewId(go, null);
                }
                var entityRepr = go.GetComponent<EntityRepresentation>();
                if (entityRepr == null)
                {
                    entityRepr = go.AddComponent<EntityRepresentation>();
                }
                go.name = reprId;
                entityRepr.assetType = EntityRepresentation.AssetType.AssetBundle;
                entityRepr.name = prefab.name;
                entityRepr.assetBundleName = assetBundleName;
                entityRepr.assetName = assetName;
                entityObjects[entityId].GetComponent<EntityData>().activeRepresentation = entityRepr;
                createdRepresentations++;
            }

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (prefab)
            {
                Debug.Log(assetName + " was loaded successfully in " + elapsedTime + " seconds");
            }
            else
            {
                Debug.LogWarning("Failed to load " + assetName + "  (" + elapsedTime + " seconds)");
            }
        }


        protected void OnModelCreated(GameObject obj, string absolutePath)
        {
            if (obj.transform.parent == null)
            {
                throw new System.NullReferenceException();
            }
            GameObject parentObj = obj.transform.parent.gameObject;
            EntityData entity = parentObj.GetComponent<EntityData>();
            if (entity == null)
            {
                throw new System.NullReferenceException();
            }
            // obj.name == representation.id
            //createdRepresentations++;
            //throw new System.NotImplementedException();
            EntityRepresentation entityRepr = obj.AddComponent<EntityRepresentation>();
            entityRepr.assetName = absolutePath;
        }


        protected void OnModelImported(GameObject obj, string absolutePath)
        {
            if (obj.transform.parent == null)
            {
                throw new System.NullReferenceException();
            }
            GameObject parentObj = obj.transform.parent.gameObject;
            EntityData entity = parentObj.GetComponent<EntityData>();
            if (entity == null)
            {
                throw new System.NullReferenceException();
            }
            var entityRepr = obj.GetComponent<EntityRepresentation>();
            if (entityRepr == null)
            {
                throw new System.NullReferenceException();
            }
            // obj.name == entityRepr.id
            entityRepr.assetType = EntityRepresentation.AssetType.Model;
            entityRepr.name = obj.name;
            entityRepr.assetName = absolutePath;
            createdRepresentations++;
        }


        protected void OnModelError(string absolutePath)
        {
            Debug.LogWarning("Failed to load representation from model " + absolutePath);
        }


        protected IEnumerator CreateRepresentationObject(
            EntityData entityData,
            string representationId,
            string representationAssetName,
            string representationAssetBundleName,
            EntityRepresentation.AssetType representationAssetType,
            EntityRepresentation.AssetPrimType representationAssetPrimType,
            ModelImportOptions importOptions
             )
        {
            bool created = false;
            EntityRepresentation entityRepresentation = null;
            //if (!string.IsNullOrEmpty(representationId))
            {
                PrimitiveType? primitiveCreated = null;
                EntityRepresentation.AssetPrimType primType = EntityRepresentation.AssetPrimType.None;
                switch (representationAssetType)
                {
                    //case EntityRepresentation.AssetType.AssetBundle:
                    //    if (!string.IsNullOrEmpty(representationAssetBundleName)
                    //        && !string.IsNullOrEmpty(representationAssetName))
                    //    {
                    //        yield return InstantiateGameObjectAsync(representationAssetBundleName, representationAssetName, representationId, entityData.id);
                    //        created = true;
                    //    }
                    //    break;
                    case EntityRepresentation.AssetType.Model:
                        if (!string.IsNullOrEmpty(representationAssetName))
                        {
                            ModelLoader importer = GetComponent<ModelLoader>();
                            if (importer == null)
                            {
                                importer = gameObject.AddComponent<ModelLoader>();
                                importer.CreatedModel += OnModelCreated;
                                importer.ImportedModel += OnModelImported;
                                importer.ImportError += OnModelError;
                            }
                            importer.ImportModelAsync(representationId, representationAssetName, entityObjects[entityData.id].transform, importOptions);
                            while (importer.Running)
                            {
                                yield return null;
                            }
                            EntityRepresentation entityRepr = null;
                            while (entityRepr == null && !importer.ErrorOccurred)
                            {
                                yield return null;
                                entityRepr = entityObjects[entityData.id].GetComponentInChildren<EntityRepresentation>();
                            }
                            if (entityRepr != null)
                            {
                                entityRepresentation = entityRepr;
                                while (entityRepr.assetType != EntityRepresentation.AssetType.Model)
                                {
                                    yield return null;
                                }
                                entityRepr.assetName = representationAssetName;
                                entityRepr.importOptions = importOptions;
                                entityObjects[entityData.id].GetComponent<EntityData>().activeRepresentation = entityRepr;

                                created = true;
                            }
                        }
                        break;
                    case EntityRepresentation.AssetType.AssetBundle:
                    case EntityRepresentation.AssetType.Prefab:
                        if (!string.IsNullOrEmpty(representationAssetName))
                        {
                            // TODO: this must be documented
                            string folder = "Representations/";
                            if (!string.IsNullOrEmpty(representationAssetBundleName))
                            {
                                folder += representationAssetBundleName + "/";
                            }
                            string resourceName = folder + representationAssetName;
                            GameObject go = null;
                            try
                            {
                                go = Instantiate(Resources.Load(resourceName), Vector3.zero, Quaternion.identity) as GameObject;
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to load resource {resourceName}: {e}");
                            }
                            if (go)
                            {
                                go.transform.SetParent(entityData.transform, false);
                                EntityRepresentation entityRepr = go.GetComponent<EntityRepresentation>();
                                if (entityRepr == null)
                                {
                                    entityRepr = go.AddComponent<EntityRepresentation>();
                                }
                                go.name = representationId;
                                entityRepr.name = representationAssetName;
                                entityRepr.assetName = representationAssetName;
                                created = true;
                                entityRepresentation = entityRepr;
                            }
                        }
                        break;
                    case EntityRepresentation.AssetType.Primitive:
                        primitiveCreated = ConvertEntityToUnityPrimType(representationAssetPrimType);
                        primType = representationAssetPrimType;
                        break;
                }
                if (primitiveCreated.HasValue)
                {
                    GameObject go = GameObject.CreatePrimitive(primitiveCreated.Value);
                    if (go)
                    {
                        go.transform.SetParent(entityData.transform, false);
                        EntityRepresentation entityRepr = go.GetComponent<EntityRepresentation>();
                        if (entityRepr == null)
                        {
                            entityRepr = go.AddComponent<EntityRepresentation>();
                        }
                        entityRepr.assetType = EntityRepresentation.AssetType.Primitive;
                        entityRepr.assetPrimType = primType;
                        entityRepr.name = representationId;
                        if (string.IsNullOrEmpty(entityRepr.name))
                        {
                            entityRepr.name = primitiveCreated.Value.ToString();
                        }
                        entityRepr.assetBundleName = null;
                        entityRepr.assetName = null;
                        created = true;
                        entityRepresentation = entityRepr;
                    }
                }
            }

            if (!created)
            {
                if (representationAssetType != EntityRepresentation.AssetType.None)
                {
                    if (string.IsNullOrEmpty(representationAssetName))
                    {
                        Debug.LogWarning($"Asset information missing for the representation {representationId}");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to load representation from asset {representationAssetName}");
                        // TODO: Add a symbolic representation (cube/text)?
                    }
                }
            }
            if (entityRepresentation)
            {
                entityData.activeRepresentation = entityRepresentation;
                createdRepresentations++;
            }
        }


        protected PrimitiveType ConvertEntityToUnityPrimType(EntityRepresentation.AssetPrimType assetPrimType)
        {
            switch (assetPrimType)
            {
                case EntityRepresentation.AssetPrimType.Cube:
                    return PrimitiveType.Cube;
                case EntityRepresentation.AssetPrimType.Sphere:
                    return PrimitiveType.Sphere;
                case EntityRepresentation.AssetPrimType.Cylinder:
                    return PrimitiveType.Cylinder;
                case EntityRepresentation.AssetPrimType.Capsule:
                    return PrimitiveType.Capsule;
                case EntityRepresentation.AssetPrimType.Quad:
                    return PrimitiveType.Quad;
                case EntityRepresentation.AssetPrimType.Plane:
                    return PrimitiveType.Plane;
            }
            Debug.LogError("Asset type " + assetPrimType.ToString() + " not mapped to a primitive.");
            return PrimitiveType.Cube;
        }


        protected OneToOneRelationship AddOneToOneRelationship(string ownerEntityId, string subjectEntityId, string objectEntityId)
        {
            GameObject ownerObject = entityObjects[ownerEntityId];
            OneToOneRelationship oneToOne = ownerObject.AddComponent<OneToOneRelationship>();
            oneToOne.subjectEntity = entityObjects[subjectEntityId].GetComponent<EntityData>();
            oneToOne.objectEntity = entityObjects[objectEntityId].GetComponent<EntityData>();
            // update cross references
            oneToOne.OnValidate();
            return oneToOne;
        }


        protected OneToManyRelationship AddOneToManyRelationship(string ownerEntityId, string subjectEntityId, List<string> objectEntitiesId)
        {
            GameObject ownerObject = entityObjects[ownerEntityId];
            OneToManyRelationship oneToMany = ownerObject.AddComponent<OneToManyRelationship>();
            SetupOneToManyRelationship(oneToMany, ownerEntityId, subjectEntityId, objectEntitiesId);
            return oneToMany;
        }


        protected CompositionRelationship AddCompositionRelationship(string ownerEntityId, string subjectEntityId, List<string> objectEntitiesId)
        {
            GameObject ownerObject = entityObjects[ownerEntityId];
            CompositionRelationship oneToMany = ownerObject.AddComponent<CompositionRelationship>();
            SetupOneToManyRelationship(oneToMany, ownerEntityId, subjectEntityId, objectEntitiesId);
            return oneToMany;
        }


        protected InclusionRelationship AddInclusionRelationship(string ownerEntityId, string subjectEntityId, List<string> objectEntitiesId)
        {
            GameObject ownerObject = entityObjects[ownerEntityId];
            InclusionRelationship oneToMany = ownerObject.AddComponent<InclusionRelationship>();
            SetupOneToManyRelationship(oneToMany, ownerEntityId, subjectEntityId, objectEntitiesId);
            return oneToMany;
        }


        protected void SetupOneToManyRelationship(OneToManyRelationship oneToMany, string ownerEntityId, string subjectEntityId, List<string> objectEntitiesId)
        {
            oneToMany.subjectEntity = entityObjects[subjectEntityId].GetComponent<EntityData>();
            oneToMany.objectEntities = new List<EntityData>(objectEntitiesId.Count);
            foreach (string id in objectEntitiesId)
            {
                oneToMany.objectEntities.Add(entityObjects[id].GetComponent<EntityData>());
            }
            // update cross references
            oneToMany.OnValidate();
        }


        protected ManyToManyRelationship AddManyToManyRelationship(string ownerEntityId, List<string> subjectEntitiesId, List<string> objectEntitiesId)
        {
            GameObject ownerObject = entityObjects[ownerEntityId];
            ManyToManyRelationship manyToMany = ownerObject.AddComponent<ManyToManyRelationship>();
            manyToMany.subjectEntities = new List<EntityData>(subjectEntitiesId.Count);
            foreach (string id in subjectEntitiesId)
            {
                manyToMany.subjectEntities.Add(entityObjects[id].GetComponent<EntityData>());
            }
            manyToMany.objectEntities = new List<EntityData>(objectEntitiesId.Count);
            foreach (string id in objectEntitiesId)
            {
                manyToMany.objectEntities.Add(entityObjects[id].GetComponent<EntityData>());
            }
            foreach (string id in objectEntitiesId)
            {
                manyToMany.objectEntities.Add(entityObjects[id].GetComponent<EntityData>());
            }
            // update cross references
            manyToMany.OnValidate();
            return manyToMany;
        }
    }
}
