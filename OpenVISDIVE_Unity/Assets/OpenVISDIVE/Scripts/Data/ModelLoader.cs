using System;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using AsImpL;
using UnityGLTF;
using UnityGLTF.Loader;

namespace OpenVISDIVE
{
    public class ModelLoader : MonoBehaviour
    {
        public string fullPath = null;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;
        public bool Multithreaded = true;
        public bool useCache = true;
        public bool PlayAnimationOnLoad = true;
        public int MaximumLod = 300;
        public int Timeout = 8;

        [SerializeField]
        private Shader shaderOverride = null;

        private ObjectImporter objLoader;
        private AsyncCoroutineHelper asyncCoroutineHelper;
        private bool errorOccurred = false;
        private GLTFSceneImporter sceneImporter = null;
        private string lastModelCacheKey = null;

        private Dictionary<string, GameObject> objectCache = new Dictionary<string, GameObject>();

        public bool ErrorOccurred
        {
            get
            {
                return errorOccurred;
            }
        }

        public bool Running
        {
            get
            {
                return (sceneImporter != null && sceneImporter.Running)
                    || (objLoader != null && !objLoader.AllImported);
            }
        }

        /// <summary>
        /// Event triggered when starting to import.
        /// </summary>
        public event Action ImportingStart;

        /// <summary>
        /// Event triggered when finished importing.
        /// </summary>
        public event Action ImportingComplete;

        /// <summary>
        /// Event triggered when a single model has been created and before it is imported.
        /// </summary>
        public event Action<GameObject, string> CreatedModel;

        /// <summary>
        /// Event triggered when a single model has been successfully imported.
        /// </summary>
        public event Action<GameObject, string> ImportedModel;

        /// <summary>
        /// Event triggered when an error occurred importing a model.
        /// </summary>
        public event Action<string> ImportError;



        /// <summary>
        /// Request to load a file asynchronously.
        /// </summary>
        /// <param name="objName"></param>
        /// <param name="filePath"></param>
        /// <param name="parentTransform"></param>
        /// <param name="options"></param>
        public void ImportModelAsync(string objName, string filePath, Transform parentTransform, ModelImportOptions options)
        {
            filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
            RootPathSettings rootPath = GetComponent<RootPathSettings>();
            if (!Path.IsPathRooted(filePath) && rootPath != null)
            {
                filePath = rootPath + filePath;
            }
            if (options != null && options.reuseLoaded)
            {
                useCache = true;
            }
            //useCache = false;
            lastModelCacheKey = CreateModelKey(filePath, options);
            if (useCache && objectCache.ContainsKey(lastModelCacheKey))
            {
                if (objectCache[lastModelCacheKey] == null)
                {
                    objectCache.Remove(lastModelCacheKey);
                }
                else
                {
                    GameObject cachedObj = Instantiate(objectCache[lastModelCacheKey], parentTransform);
                    cachedObj.name = objName;
                    OnImportingStart();
                    OnCreatedModel(cachedObj, filePath);
                    OnImportedModel(cachedObj, filePath);
                    OnImportingComplete();
                    return;
                }
            }

            if (filePath.EndsWith(".obj", true, CultureInfo.InvariantCulture))
            {
                ImportOptions objOptions = new ImportOptions();
                if (options != null)
                {
                    objOptions.buildColliders = options.buildColliders;
                    objOptions.convertToDoubleSided = options.convertToDoubleSided;
                    objOptions.inheritLayer = options.inheritLayer;
                    objOptions.litDiffuse = options.litDiffuse;
                    objOptions.localEulerAngles = options.localEulerAngles;
                    objOptions.localPosition = options.localPosition;
                    objOptions.localScale = options.localScale;
                    objOptions.modelScaling = options.modelScaling;
                }
                else
                {
                    //TODO: define global default options
                    objOptions.buildColliders = true;
                    objOptions.convertToDoubleSided = false;
                    objOptions.inheritLayer = false;
                    objOptions.litDiffuse = false;
                    objOptions.localEulerAngles = Vector3.zero;
                    objOptions.localPosition = Vector3.zero;
                    objOptions.localScale = Vector3.one;
                    objOptions.modelScaling = 0.01f;
                }
                objOptions.colliderConvex = true;
                objOptions.reuseLoaded = false;
                objOptions.use32bitIndices = true;
                objOptions.zUp = options.zUp;
                objOptions.hideWhileLoading = true;

                objLoader.ImportModelAsync(objName, filePath, parentTransform, objOptions);
            }
            else if (filePath.EndsWith(".gltf", true, CultureInfo.InvariantCulture))
            {
                ImportGltfAsync(objName, filePath, parentTransform, options);
            }
        }


        private async void ImportGltfAsync(string objName, string filePath, Transform parentTransform, ModelImportOptions importOptions)
        {
            GameObject newObj = new GameObject(objName);
            if (parentTransform != null) newObj.transform.SetParent(parentTransform, false);
            OnCreatedModel(newObj, filePath);
            bool error = false;
            OnImportingStart();
            // https://stackoverflow.com/questions/14455293/how-and-when-to-use-async-and-await
            try
            {
                await LoadGltfAsync(newObj, filePath, importOptions);
            }
            catch (Exception e)
            {
                error = true;
                Debug.LogWarning($"Failed loading {filePath}: {e}");
            }
            if (error)
            {
                OnImportError($"Failed loading {filePath}");
            }
            else
            {
                if (newObj.transform.childCount > 0)
                {
                    GameObject childObj = newObj.transform.GetChild(0).gameObject;
                    newObj.name = childObj.name;
                    if (importOptions != null)
                    {
                        childObj.transform.localPosition = importOptions.localPosition;
                        childObj.transform.localRotation = Quaternion.Euler(importOptions.localEulerAngles); ;
                        childObj.transform.localScale = importOptions.localScale;
                        if (importOptions.inheritLayer)
                        {
                            childObj.layer = childObj.transform.parent.gameObject.layer;
                            MeshRenderer[] mrs = childObj.transform.GetComponentsInChildren<MeshRenderer>(true);
                            for (int i = 0; i < mrs.Length; i++)
                            {
                                mrs[i].gameObject.layer = childObj.transform.parent.gameObject.layer;
                            }
                        }
                        Shader unlitTextureShader = Shader.Find("Unlit/Texture");
                        Shader unlitTransparentShader = Shader.Find("Unlit/Transparent");
                        if (importOptions.litDiffuse && unlitTextureShader != null && unlitTransparentShader != null)
                        {
                            MeshRenderer[] mrs = childObj.transform.GetComponentsInChildren<MeshRenderer>(true);
                            for (int i = 0; i < mrs.Length; i++)
                            {
                                Material mtl = mrs[i].sharedMaterial;
                                Texture2D tex = mtl.GetTexture("_MainTex") as Texture2D;
                                if (tex != null)
                                {
                                    //if (tex.format == TextureFormat.ARGB32)
                                    if (mtl.IsKeywordEnabled("_ALPHABLEND_ON"))
                                    {
                                        mtl.shader = unlitTransparentShader;
                                    }
                                    else
                                    {
                                        mtl.shader = unlitTextureShader;
                                    }
                                }
                            }
                        }
                    }
                }
                OnImportedModel(newObj, filePath);
                OnImportingComplete();
            }
        }



        public async Task LoadGltfAsync(GameObject newObject, string fullPath, ModelImportOptions importOptions)
        {
            asyncCoroutineHelper = newObject.GetComponent<AsyncCoroutineHelper>() ?? gameObject.AddComponent<AsyncCoroutineHelper>();
            //GLTFSceneImporter sceneImporter = null;
            ILoader loader = null;
            try
            {
                // Path.Combine treats paths that start with the separator character
                // as absolute paths, ignoring the first path passed in. This removes
                // that character to properly handle a filename written with it.
                fullPath = fullPath.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                string directoryPath = URIHelper.GetDirectoryName(fullPath);
                loader = new FileLoader(directoryPath);
                sceneImporter = new GLTFSceneImporter(
                    Path.GetFileName(fullPath),
                    loader,
                    asyncCoroutineHelper
                    );
                sceneImporter.SceneParent = newObject.transform;
                sceneImporter.Collider = Collider;
                if (importOptions != null)
                {
                    if (importOptions.buildColliders)
                    {
                        sceneImporter.Collider = GLTFSceneImporter.ColliderType.MeshConvex;
                        //if (importOptions.colliderConvex)
                        //{
                        //    sceneImporter.Collider = GLTFSceneImporter.ColliderType.MeshConvex;
                        //}
                        //else
                        //{
                        //    sceneImporter.Collider = GLTFSceneImporter.ColliderType.Mesh;
                        //}
                    }
                    else
                    {
                        sceneImporter.Collider = GLTFSceneImporter.ColliderType.None;
                    }
                }
                sceneImporter.MaximumLod = MaximumLod;
                sceneImporter.Timeout = Timeout;
                sceneImporter.isMultithreaded = Multithreaded;
                sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;
                await sceneImporter.LoadSceneAsync();

                // Override the shaders on all materials if a shader is provided
                if (shaderOverride != null)
                {
                    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.sharedMaterial.shader = shaderOverride;
                    }
                }

                if (PlayAnimationOnLoad)
                {
                    Animation[] animations = sceneImporter.LastLoadedScene.GetComponents<Animation>();
                    foreach (Animation anim in animations)
                    {
                        anim.Play();
                    }
                }
            }
            finally
            {
                if (loader != null)
                {
                    sceneImporter?.Dispose();
                    sceneImporter = null;
                    loader = null;
                }
            }
        }


        private string CreateModelKey(string filePath, ModelImportOptions options)
        {
            string searchKey = filePath;
            if (options != null)
            {
                searchKey = searchKey
                   + options.litDiffuse
                   + options.localEulerAngles
                   + options.localPosition
                   + options.localScale
                   + options.modelScaling
                   + options.zUp;
            }
            return searchKey;

        }


        private void Awake()
        {
            objLoader = GetComponent<ObjectImporter>();
            if (objLoader == null)
            {
                objLoader = gameObject.AddComponent<ObjectImporter>();
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnEnable()
        {
            objLoader.ImportingStart += OnImportingStart;
            objLoader.ImportingComplete += OnImportingComplete;
            objLoader.CreatedModel += OnCreatedModel;
            objLoader.ImportedModel += OnImportedModel;
            objLoader.ImportError += OnImportError;
        }

        private void OnDisable()
        {
            objLoader.ImportingStart -= OnImportingStart;
            objLoader.ImportingComplete -= OnImportingComplete;
            objLoader.CreatedModel -= OnCreatedModel;
            objLoader.ImportedModel -= OnImportedModel;
            objLoader.ImportError -= OnImportError;
        }

        private void OnImportError(string msg)
        {
            errorOccurred = true;
            ImportError?.Invoke(msg);
        }

        private void OnImportedModel(GameObject obj, string path)
        {
            if (useCache && !objectCache.ContainsKey(lastModelCacheKey))
            {
                objectCache.Add(lastModelCacheKey, obj);
            }
            ImportedModel?.Invoke(obj, path);
        }

        private void OnCreatedModel(GameObject obj, string path)
        {
            CreatedModel?.Invoke(obj, path);
        }

        private void OnImportingComplete()
        {
            ImportingComplete?.Invoke();
        }

        private void OnImportingStart()
        {
            errorOccurred = false;
            ImportingStart?.Invoke();
        }
    }
}
