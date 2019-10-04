using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenVISDIVE
{
    public class DirectoryChooser : PathChooser
    {
        [SerializeField]
        protected InputField pathInput = null;

        [SerializeField]
        protected Transform folderListboxContent = null;

        [SerializeField]
        protected Sprite folderSprite = null;


        public void ClearDirectoryListbox()
        {
            for (int i = 1; i < folderListboxContent.transform.childCount; i++)
            {
                Destroy(folderListboxContent.transform.GetChild(i).gameObject);
            }
        }


        public override void UpdateDialog(string rootPath)
        {
            currentPath = Path.GetFullPath(rootPath);
            if (folderListboxContent != null)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(currentPath);
                if (!dirInfo.Exists)
                {
                    Debug.LogError($"This path does not exist: {currentPath}");
                    return;
                }
                ClearDirectoryListbox();
                pathInput.text = currentPath;
                Button upButton = folderListboxContent.transform.GetChild(0).GetComponentInChildren<Button>();
                upButton.onClick.RemoveAllListeners();
                upButton.onClick.AddListener(() => SelectDirectory(".."));
                FillDirectoryListbox(dirInfo, folderListboxContent);
            }
        }

        public void SelectDirectory(string dirName)
        {
            string separator = "" + Path.DirectorySeparatorChar;
            if (!currentPath.EndsWith(separator))
            {
                currentPath += separator;
            }
            currentPath += dirName;
            currentPath = Path.GetFullPath(currentPath);
            UpdateDialog(currentPath);
        }


        public virtual void OnDirectoryChosen()
        {
            currentPath = Path.GetFullPath(pathInput.text);
            Debug.Log($"Path selected:\n{currentPath}");
            Hide();
            OnPathChosen(currentPath);
        }


        protected void FillDirectoryListbox(DirectoryInfo dirInfo, Transform listboxContent)
        {
            DirectoryInfo[] subDirInfo = dirInfo.GetDirectories();
            foreach (DirectoryInfo dir in subDirInfo)
            {
                // skip hidden folders
                if ((dir.Attributes & FileAttributes.Hidden) == 0)
                {
                    GameObject newItem = Instantiate(listboxContent.transform.GetChild(0).gameObject, listboxContent.transform);
                    newItem.SetActive(true);
                    string dirName = dir.Name;
                    newItem.name = dirName;
                    newItem.GetComponentInChildren<Text>().text = dirName;
                    newItem.GetComponentsInChildren<Image>()[1].sprite = folderSprite;
                    Button button = newItem.GetComponentInChildren<Button>();
                    button.interactable = true;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectDirectory(dirName));
                }
            }
        }


        protected override void OnValidate()
        {
            base.OnValidate();

            if (dialogObject != null)
            {
                if (pathInput == null)
                {
                    Transform tr = dialogObject.transform.Find("PathInputField");
                    pathInput = tr?.GetComponent<InputField>();
                }
                if (folderListboxContent == null)
                {
                    folderListboxContent = dialogObject.transform.Find("FolderListbox/Viewport/Content");
                }
            }
        }

    }
}
