using UnityEngine;
using AsImpL;

public class ObjLoader : MonoBehaviour
{
    [SerializeField] private string filePath = "models/OBJ_test/objtest_zup.obj";
    [SerializeField] private Transform parentTransform = null;
    [SerializeField] private ImportOptions importOptions = new ImportOptions();
    private ObjectImporter objImporter;

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

    public void Import()
    {
        objImporter.ImportModelAsync("MyObject", filePath, parentTransform, importOptions);
    }

    private void Awake()
    {
#if (UNITY_ANDROID || UNITY_IPHONE)
                filePath = Application.persistentDataPath + "/" + filePath;
#endif
        objImporter = gameObject.AddComponent<ObjectImporter>();
    }
}
