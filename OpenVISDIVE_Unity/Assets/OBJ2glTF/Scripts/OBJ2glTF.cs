using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityGLTF;
using AsImpL;

public class OBJ2glTF : MonoBehaviour
{
    public enum ImportUnitEnum { Millimeters = 0, Centimeters, Meters, Kilometers, Inches, Feet, Miles, NauticMiles, Custom };

    [SerializeField] private Transform sceneRoot = null;

    [Header("Import")]
    [SerializeField] private string filePath = "models/OBJ/objtest_yup.obj";
    [SerializeField] private bool importAsRTT = false;
    [SerializeField] private bool importZUp = false;
    [SerializeField] private ImportUnitEnum importUnit = ImportUnitEnum.Meters;
    [SerializeField] private float importScale = 1.0f;
    [Header("Export")]
    [SerializeField] private string exportedFilePath = "models/glTF";
    [SerializeField] private bool exportAsBin = false;
    [SerializeField] private string exportedFileNameOverride = "";

    [Header("UI")]
    [SerializeField] private Button importButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private Dropdown unitsDropdown;
    [SerializeField] private InputField scaleInputField;
    [SerializeField] private Toggle rttToggle;
    [SerializeField] private Toggle zupToggle;
    [SerializeField] private InputField importInputField;
    [SerializeField] private InputField exportInputField;
    [SerializeField] private InputField exportNameOverrideInputField;

    private ObjectImporter objImporter;
    private ImportOptions importOptions = new ImportOptions();
    private string objectName;
    private bool autoExport = false;
    private bool guiLock = false;
    private bool isAwakened = false;

    public bool ExportAsBin
    {
        get
        {
            return exportAsBin;
        }

        set
        {
            exportAsBin = value;
        }
    }

    public string ExportedFileName
    {
        get
        {
            return exportedFileNameOverride;
        }

        set
        {
            exportedFileNameOverride = value;
        }
    }

    public string FilePath
    {
        get
        {
            return filePath;
        }

        set
        {
            filePath = value;
        }
    }

    public bool ImportAsRTT
    {
        get
        {
            return importAsRTT;
        }

        set
        {
            importAsRTT = value;
        }
    }

    public bool ImportZUp
    {
        get
        {
            return importZUp;
        }

        set
        {
            importZUp = value;
        }
    }

    public string ImportScale
    {
        get
        {
            return importScale.ToString();
        }

        set
        {
            float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out importScale);
            SyncUnitValues();
            if (!guiLock && unitsDropdown != null)
            {
                guiLock = true;
                unitsDropdown.value = (int)importUnit;
                guiLock = false;
            }
        }
    }


    public ImportUnitEnum ImportUnit
    {
        get
        {
            return importUnit;
        }

        set
        {
            importUnit = value;
            importScale = GetImportScale();
            if (!guiLock && unitsDropdown != null)
            {
                guiLock = true;
                scaleInputField.text = importScale.ToString(CultureInfo.InvariantCulture.NumberFormat);
                guiLock = false;
            }
        }
    }

    public string ExportedFilePath
    {
        get
        {
            return exportedFilePath;
        }

        set
        {
            exportedFilePath = value;
        }
    }

    public void OnUiUnitSelection(int index)
    {
        ImportUnit = (ImportUnitEnum)index;
    }


    public void ClearScene()
    {
        for (int i = 0; i < sceneRoot.childCount; i++)
        {
            Transform childTransform = sceneRoot.transform.GetChild(i);
            Destroy(childTransform.gameObject);
        }
    }


    public void Import()
    {
        importOptions.litDiffuse = importAsRTT;
        importOptions.modelScaling = importScale;
        importOptions.zUp = importZUp;
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogWarning("File path missing");
            return;
        }
        objectName = Path.GetFileNameWithoutExtension(filePath);
        objImporter.ImportModelAsync(objectName, filePath, sceneRoot, importOptions);
    }


    public string RetrieveTexturePath(UnityEngine.Texture texture)
    {
        return texture.name;
    }


    public void Export()
    {
        if (sceneRoot.transform.childCount == 0)
        {
            Debug.LogError("Nothing to export");
            return;
        }
        string fileName = exportedFileNameOverride;
        if (string.IsNullOrEmpty(objectName))
        {
            objectName = sceneRoot.transform.GetChild(0).gameObject.name;
        }
        if (string.IsNullOrEmpty(fileName))
        {
            int idx = objectName.LastIndexOfAny(new char[] { '\\', '/' });
            if (idx >= 0)
            {
                fileName = objectName.Substring(idx + 1);
            }
            else
            {
                fileName = objectName;
            }
        }
        Export(sceneRoot, exportedFilePath, fileName);
    }


    public void MultiExport(Transform rootTransform)
    {
        for (int i = 0; i < rootTransform.childCount; i++)
        {
            Transform childTransform = rootTransform.transform.GetChild(i);
            Export(childTransform, exportedFilePath, childTransform.gameObject.name);
            //Export(childTransform, exportedFilePath + @"\" + childTransform.gameObject.name, childTransform.gameObject.name);
        }
    }


    public void Export(Transform rootTransform, string filePath, string fileName)
    {
        if (rootTransform == null)
        {
            throw new ArgumentNullException(nameof(rootTransform));
        }

        Transform[] childrenTransforms = new Transform[rootTransform.childCount];
        for (int i = 0; i < rootTransform.childCount; i++)
        {
            childrenTransforms[i] = rootTransform.transform.GetChild(i);
        }

        GLTFSceneExporter exporter = new GLTFSceneExporter(childrenTransforms, RetrieveTexturePath);
        Debug.LogFormat("Exporting {0} to {1}", fileName, filePath);
        if (exportAsBin)
        {
            exporter.SaveGLB(filePath, fileName);
        }
        else
        {
            exporter.SaveGLTFandBin(filePath, fileName);
        }
        //string appPath = Application.dataPath;
        //string wwwPath = appPath.Substring(0, appPath.LastIndexOf("Assets")) + "out";
        //exporter.SaveGLTFandBin(Path.Combine(wwwPath, exportedFileName), exportedFileName);
    }


    private void OnImportComplete()
    {
        //        foreach (Renderer renderer in sceneRoot.GetComponentsInChildren<Renderer>())
        //        {
        //#if UNITY_5_6_OR_NEWER
        //            RendererExtensions.UpdateGIMaterials(renderer);
        //#else
        //            DynamicGI.UpdateMaterials(renderer);
        //#endif
        //        }
        // if more than one file must be exported export them automatically
        // else leave this task to the user
        if (autoExport && sceneRoot.transform.childCount > 1)
        {
            MultiExport(sceneRoot);
        }
        exportButton.interactable = true;
        importButton.interactable = true;
        autoExport = false;

    }


    private void UpdateUI()
    {
        if (unitsDropdown != null)
        {
            unitsDropdown.value = (int)importUnit;
        }
        if (scaleInputField != null)
        {
            scaleInputField.text = importScale.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }
        if (rttToggle != null)
        {
            rttToggle.isOn = importAsRTT;
        }
        if (zupToggle != null)
        {
            zupToggle.isOn = importZUp;
        }
        if (importInputField != null)
        {
            importInputField.text = filePath;
        }
        if (exportedFilePath != null)
        {
            exportInputField.text = exportedFilePath;
        }
        if (exportNameOverrideInputField != null)
        {
            exportNameOverrideInputField.text = exportedFileNameOverride;
        }
    }

    private void OnImportStart()
    {
        if (autoExport)
        {
            OnValidate();
            UpdateUI();
        }
        exportButton.interactable = false;
        importButton.interactable = false;
    }


    private void OnImportError(string obj)
    {
        exportButton.interactable = true;
        importButton.interactable = true;
    }


    private void OnEnable()
    {
        objImporter.ImportingStart += OnImportStart;
        objImporter.ImportingComplete += OnImportComplete;
        objImporter.ImportError += OnImportError;
    }


    private void OnDisable()
    {
        objImporter.ImportingStart -= OnImportStart;
        objImporter.ImportingComplete -= OnImportComplete;
        objImporter.ImportError -= OnImportError;
    }


    private void Awake()
    {
#if (UNITY_ANDROID || UNITY_IPHONE)
        filePath = Application.persistentDataPath + "/" + filePath;
#endif
        objImporter = gameObject.GetComponent<ObjectImporter>();
        isAwakened = true;
    }

    private void Start()
    {
#if UNITY_STANDALONE && !UNITY_EDITOR
        string[] args = Environment.GetCommandLineArgs();
        List<ModelImportInfo> modelImportInfos = new List<ModelImportInfo>();
        if (args != null && args.Length > 1)
        {
            int numArgs = args.Length - 1;
            for (int i = 0; i < numArgs; i++)
            {
                if (args[i + 1].StartsWith("-"))
                {
                    if (args[i + 1] == "-scale")
                    {
                        if (i + 1 < numArgs)
                        {
                            float.TryParse(args[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out importScale);
                        }
                        i++;
                    }
                    if (args[i + 1] == "-out")
                    {
                        if (i + 1 < numArgs)
                        {
                            exportedFilePath = args[i + 2];
                        }
                        i++;
                    }
                    if (args[i + 1] == "-rtt")
                    {
                        importAsRTT = true;
                    }
                    if (args[i + 1] == "-zup")
                    {
                        importZUp = true;
                    }
                    continue;
                }
                ModelImportInfo modelToImport = new ModelImportInfo();
                modelToImport.path = args[i + 1];
                modelToImport.name = Path.GetFileNameWithoutExtension(modelToImport.path);
                modelToImport.loaderOptions = new ImportOptions();
                modelToImport.loaderOptions.modelScaling = importScale;
                modelToImport.loaderOptions.litDiffuse = importAsRTT;
                modelToImport.loaderOptions.zUp = importZUp;
                modelToImport.loaderOptions.reuseLoaded = false;
                modelImportInfos.Add(modelToImport);
            }
            autoExport = true;
            ImportModelListAsync(modelImportInfos.ToArray());
        }
#endif
#if UNITY_ANDROID
        filePath = Application.persistentDataPath + "/models/OBJ/objtest_yup.obj";
        exportedFilePath = Application.persistentDataPath + "/models/glTF";
        Import();
#endif
        UpdateUI();
    }

    /// <summary>
    /// Load a set of files with their own import options
    /// </summary>
    /// <param name="modelsInfo">List of file import entries</param>
    public void ImportModelListAsync(ModelImportInfo[] modelsInfo)
    {
        if (modelsInfo == null || modelsInfo.Length == 0)
        {
            return;
        }
        if (modelsInfo.Length == 1)
        {
            filePath = modelsInfo[0].path;
        }
        for (int i = 0; i < modelsInfo.Length; i++)
        {
            if (modelsInfo[i].skip) continue;
            string objName = modelsInfo[i].name;
            string modelFilePath = modelsInfo[i].path;
            if (string.IsNullOrEmpty(modelFilePath))
            {
                Debug.LogWarning("File path missing");
                continue;
            }
            //#if (UNITY_ANDROID || UNITY_IPHONE)
            //          filePath = Application.persistentDataPath + "/" + filePath;
            //#endif
            ImportOptions options = modelsInfo[i].loaderOptions;
            if (options == null)
            {
                options = new ImportOptions();
                options.litDiffuse = importAsRTT;
                options.modelScaling = importScale;
                options.zUp = importZUp;
            }
            if (importScale != 0 && options.localScale == Vector3.zero)
            {
                options.localScale.Set(importScale, importScale, importScale);
            }
            options.reuseLoaded = true;
            options.inheritLayer = true;
            objImporter.ImportModelAsync(objName, modelFilePath, sceneRoot, options);
        }
    }


    private float GetImportScale()
    {
        float unitScale = GetUnitByEnum(importUnit);
        return unitScale > 0 ? unitScale : importScale;
    }


    private float GetUnitByEnum(ImportUnitEnum unit)
    {
        switch (unit)
        {
            case ImportUnitEnum.Millimeters: return 0.001f;
            case ImportUnitEnum.Centimeters: return 0.01f;
            case ImportUnitEnum.Meters: return 1f;
            case ImportUnitEnum.Kilometers: return 1000f;
            case ImportUnitEnum.Inches: return 0.0254f;
            case ImportUnitEnum.Feet: return 0.3048f;
            case ImportUnitEnum.Miles: return 1609.34f;
            case ImportUnitEnum.NauticMiles: return 1852f;
        }
        return -1f;
    }

    private void OnValidate()
    {
        if(!isAwakened)
        {
            return;
        }
        if (objImporter == null)
        {
            objImporter = gameObject.AddComponent<ObjectImporter>();
        }
        if (exportButton == null)
        {
            exportButton = GameObject.Find("ExportButton").GetComponent<Button>();
        }
        if (importButton == null)
        {
            importButton = GameObject.Find("ImportButton").GetComponent<Button>();
        }
        if (unitsDropdown == null)
        {
            unitsDropdown = GameObject.Find("UnitsDropdown").GetComponent<Dropdown>();
        }
        if (scaleInputField == null)
        {
            scaleInputField = GameObject.Find("ScaleInputField").GetComponent<InputField>();
        }
        if (rttToggle == null)
        {
            rttToggle = GameObject.Find("ToggleRTT").GetComponent<Toggle>();
        }
        if (zupToggle == null)
        {
            zupToggle = GameObject.Find("ToggleZup").GetComponent<Toggle>();
        }
        if (importInputField == null)
        {
            importInputField = GameObject.Find("ImportInputField").GetComponent<InputField>();
        }
        if (exportInputField == null)
        {
            exportInputField = GameObject.Find("ExportInputField").GetComponent<InputField>();
        }
        if (exportNameOverrideInputField == null)
        {
            exportNameOverrideInputField = GameObject.Find("ExportNameOverrideInputField").GetComponent<InputField>();
        }

        SyncUnitValues();
    }


    private void SyncUnitValues()
    {
        float unitScale = GetUnitByEnum(importUnit);
        if (unitScale != importScale || unitScale < 0)
        {
            importUnit = ImportUnitEnum.Custom;
        }
        if (importUnit > 0)
        {
            for (int i = 0; i < (int)ImportUnitEnum.Custom; i++)
            {
                ImportUnitEnum unit = (ImportUnitEnum)i;
                if (Mathf.Abs(importScale - GetUnitByEnum(unit)) < float.Epsilon)
                {
                    importUnit = unit;
                }
            }
        }
    }

}
