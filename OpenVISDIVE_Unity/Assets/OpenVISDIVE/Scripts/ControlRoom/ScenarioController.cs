using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenVISDIVE;

namespace OpenVISDIVE
{
    public class ScenarioController : AbstractController
    {
        [SerializeField]
        protected ScenarioData scenarioData = null;
        [SerializeField]
        protected DataSync dataSync = null;
        [SerializeField]
        protected EntityValidator entityValidator = null;

        protected enum OperationEnum { None, Load, Save, New, Position }
        protected string scenarioFileDescription = "XML document";
        protected string scenarioFilePattern = "*.xml";
        protected OperationEnum pendingOperation = OperationEnum.None;
        protected GameObject scenarioRootObject;
        protected GameObject scenarioNewObject;

        private bool buildingSelectionMode = false;
        private bool componentSelectionMode;


        public bool Modified { get; set; } = false;

        public bool BuildingSelectionMode
        {
            get => buildingSelectionMode;
            set
            {
                Debug.Log(buildingSelectionMode ? "Building selection" : "Machine selection");
                buildingSelectionMode = value;
            }
        }

        public bool ComponentSelectionMode
        {
            get => componentSelectionMode;
            set
            {
                Debug.Log(componentSelectionMode ? "Component selection" : "Group selection");
                componentSelectionMode = value;
            }
        }


public GameObject SelectedObject
        {
            get => raycastPositioner?.OriginalObject;
        }


        public void ChooseScenario()
        {
            uiController.FileSelected += OnScenarioFileSelected;
            uiController.SetFileTypeFilter(
                new string[] { "All types (*.*)", scenarioFileDescription },
                new string[] { "*.*", scenarioFilePattern }, 1);
            pendingOperation = OperationEnum.Load;
            uiController.OpenFileChooserDialog("scenario");
        }


        public void SaveScenarioAs()
        {
            uiController.FileSelected += OnScenarioFileSelected;
            uiController.SetFileTypeFilter(
                new string[] { scenarioFileDescription },
                new string[] { scenarioFilePattern }, 0);
            pendingOperation = OperationEnum.Save;
            uiController.OpenFileChooserDialog("scenario");
        }


        private void OnScenarioFileSelected(string scenarioPath)
        {
            uiController.FileSelected -= OnScenarioFileSelected;
            switch (pendingOperation)
            {
                case OperationEnum.Load:
                    LoadScenario(scenarioPath);
                    break;
                case OperationEnum.Save:
                    SaveScenario(scenarioPath);
                    break;
            }
            pendingOperation = OperationEnum.None;
        }


        private void OnCancelled()
        {
            uiController.Confirmed -= OnConfirmed;
            uiController.Cancelled -= OnCancelled;
            pendingOperation = OperationEnum.None;
        }

        private void OnConfirmed()
        {
            uiController.Confirmed -= OnConfirmed;
            uiController.Cancelled -= OnCancelled;
            switch (pendingOperation)
            {
                case OperationEnum.New:
                    NewScenario();
                    break;
            }
            pendingOperation = OperationEnum.None;
        }


        public void ConfirmNewScenario()
        {
            if (Modified)
            {
                pendingOperation = OperationEnum.New;
                uiController.Confirmed += OnConfirmed;
                uiController.Cancelled += OnCancelled;
                uiController.OpenConfirmDialog();
            }
            else
            {
                NewScenario();
            }
        }


        public void NewScenario()
        {
            raycastPositioner.CancelPositioning();
            uiController.CloseAllMenus();
            dataSync.CleanUpScene();
            supervisor.ResetCommandList();
        }


        public void DoCreateEntity(GameObject prefab)
        {
            supervisor.Do(new CreateEntityCommand(this, prefab));
        }


        public void DoDeleteEntity(GameObject entityObj)
        {
            supervisor.Do(new DestroyEntityCommand(this, entityObj));
        }


        public void DoPosition(GameObject entityObj, Vector3 prevPosition, Vector3 prevEulerAngles, Vector3 position, Vector3 eulerAngles)
        {
            supervisor.Do(new PositionCommand(this, entityObj, prevPosition, prevEulerAngles, position, eulerAngles));
        }


        public GameObject CreateScenario(string scenarioName)
        {
            scenarioRootObject = new GameObject(scenarioName);
            scenarioData = scenarioRootObject.AddComponent<ScenarioData>();
            scenarioData.name = scenarioName;
            dataSync.scenarioDataFile = scenarioName + ".xml";
            return scenarioRootObject;
        }


        public GameObject CreateEntity(GameObject prefab)
        {
            if (scenarioRootObject == null)
            {
                scenarioRootObject = CreateScenario("Scenario");
            }
            //dataSync.CreateEntity(prefab, scenarioRootObject.transform);
            // Create a custom game object
            GameObject entObj = Instantiate(prefab, scenarioRootObject.transform);
            EntityData entityInfo = entObj.GetComponent<EntityData>();
            entityInfo.id = DataUtil.CreateNewEntityId(entityInfo);
            entObj.name = prefab.name;
            if (!entityInfo.isStatic)
            {
                scenarioNewObject = entObj;
                pendingOperation = OperationEnum.Position;
                dataSync.RepresentationLoaded += OnRepresentationLoaded;
            }
            LoadRepresentation(entityInfo);
            scenarioData.AddEntity(entityInfo);
            Modified = true;

            return entObj;
        }


        public void DestroyEntity(GameObject entityObject)
        {
            EntityData entity = entityObject.GetComponent<EntityData>();
            scenarioData.DestroyEntity(entity);
            Destroy(entityObject);
        }


        public void DeleteEntity(GameObject entityObject)
        {
            if (entityObject==SelectedObject)
            {
                StartCoroutine(DestroySelection());
            }
            else
            {
                DestroyEntity(entityObject);
            }
        }


        public virtual void LoadScenario(string scenarioPath)
        {
            dataSync.ScenarioFullPath = scenarioPath;
            LoadScenario();
        }


        public virtual void LoadScenario()
        {
            if (scenarioRootObject == null)
            {
                scenarioRootObject = CreateScenario("Scenario");
            }
            dataSync.LoadDataToScene();
            supervisor.ResetCommandList();
            Modified = false;
        }


        public virtual void LoadCatalog()
        {
            throw new NotImplementedException();
        }


        public virtual void SaveScenario(string scenarioPath)
        {
            dataSync.ScenarioFullPath = scenarioPath;
            SaveScenario();
        }


        public virtual void SaveScenario()
        {
            dataSync.SaveDataFromScene();
            Modified = false;
        }


        public virtual void LoadRepresentation(EntityData entityInfo)
        {
            StartCoroutine(dataSync.LoadRepresentation(entityInfo));
        }



        public void SetBuildingSelectionMode(bool buildingMode)
        {
            Debug.Log(buildingMode ? "Building selection" : "Module selection");
            BuildingSelectionMode = buildingMode;
        }


        public void SetComponentSelectionMode(bool componentMode)
        {
            Debug.Log(componentMode ? "Component selection" : "Group selection");
            ComponentSelectionMode = componentMode;
        }


        protected override void Init()
        {
            if(dataSync==null)
            {
                dataSync = FindObjectOfType<DataSync>();
            }
            if(entityValidator == null)
            {
                entityValidator = FindObjectOfType<EntityValidator>();
            }
        }

        private void OnRepresentationLoaded()
        {
            dataSync.RepresentationLoaded -= OnRepresentationLoaded;
            StartCoroutine(StartPositioningDelayed());
        }


        private IEnumerator StartPositioningDelayed()
        {
            raycastPositioner.enabled = true;
            yield return new WaitForEndOfFrame();
            //while (scenarioNewObject.GetComponentInChildren<Renderer>() == null)
            //{
            //    yield return new WaitForEndOfFrame();
            //}
            //yield return new WaitForSeconds(1f);

            // TODO: manage the decision whether or not interactively positioning the object
            if (!scenarioNewObject.GetComponent<EntityData>().isStatic)
            {
                raycastPositioner.StartPositioning(scenarioNewObject);
            }
            scenarioNewObject = null;
        }


        private IEnumerator DestroySelection()
        {
                GameObject entityObject = SelectedObject;
                raycastPositioner.Clear();
                yield return new WaitForEndOfFrame();
                DestroyEntity(entityObject);
        }


        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Delete) && SelectedObject!=null)
            {
                DoDeleteEntity(SelectedObject);
            }
        }


        private void Start()
        {
            raycastPositioner.Positioned += DoPosition;
        }
    }
}
