using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenVISDIVE
{
    public class FileChooser : DirectoryChooser
    {
        [SerializeField]
        protected Sprite documentSprite = null;

        [SerializeField]
        private Transform fileListboxContent = null;

        [SerializeField]
        private Dropdown fileTypeDropdown = null;

        [SerializeField]
        private bool showDirectoriesInFileList = false;

        private List<string> filterDescriptions = new List<string>();
        private List<string> filterSearchPattern = new List<string>();
        private string searchPattern = null;
        private int currentFilterIndex = 0;

        public void SetFileTypes(string[] descriptions, string[] extensions, int defaultFilter = 0)
        {
            filterDescriptions.Clear();
            filterSearchPattern.Clear();
            filterDescriptions.AddRange(descriptions);
            filterSearchPattern.AddRange(extensions);
            fileTypeDropdown.ClearOptions();
            fileTypeDropdown.AddOptions(filterDescriptions);
            SetFileTypeFilter(defaultFilter);
        }


        public void ApplyFileTypes(string[] descriptions, string[] extensions, int defaultFilter = 0)
        {
            SetFileTypes(descriptions, extensions, defaultFilter);
            UpdateDialog();
        }


        public void SetFileTypeFilter(int index)
        {
            currentFilterIndex = index;
            searchPattern = filterSearchPattern[index];
            if (fileTypeDropdown != null && fileTypeDropdown.value != currentFilterIndex)
            {
                fileTypeDropdown.value = currentFilterIndex;
            }
        }


        public void ApplyFileTypeFilter(int index)
        {
            SetFileTypeFilter(index);
            UpdateDialog();
        }


        public void ClearFileListbox()
        {
            for (int i = 1; i < fileListboxContent.transform.childCount; i++)
            {
                Destroy(fileListboxContent.transform.GetChild(i).gameObject);
            }
        }


        public override void UpdateDialog()
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                UpdateDialog(currentPath);
            }
        }


        public override void UpdateDialog(string rootPath)
        {
            FileInfo fileInfo = null;
            rootPath = Path.GetFullPath(rootPath);
            DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
            if (!dirInfo.Exists)
            {
                fileInfo = new FileInfo(rootPath);
                rootPath = Path.GetDirectoryName(rootPath);
                dirInfo = new DirectoryInfo(rootPath);
            }
            if (!dirInfo.Exists)
            {
                Debug.LogError($"This path does not exist: {rootPath}");
                return;
            }
            base.UpdateDialog(rootPath);
            if (fileInfo != null)// && fileInfo.Exists)
            {
                pathInput.text = fileInfo.FullName;
            }

            if (fileListboxContent != null)
            {
                if (filterSearchPattern.Count == 0)
                {
                    SetFileTypes(new string[] { "All types (*.*)" }, new string[] { "*.*" });
                }
                ClearFileListbox();
                if (showDirectoriesInFileList)
                {
                    FillDirectoryListbox(dirInfo, fileListboxContent);
                }

                FileInfo[] fileList = string.IsNullOrEmpty(searchPattern) ? dirInfo.GetFiles() : dirInfo.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
                Button emptyButton = fileListboxContent.transform.GetChild(0).GetComponentInChildren<Button>();
                emptyButton.gameObject.SetActive(false);// fileList.Length == 0 && fileListboxContent.transform.childCount == 1);
                //emptyButton.gameObject.GetComponentsInChildren<Image>()[1].gameObject.SetActive(false);
                foreach (FileInfo file in fileList)
                {
                    GameObject newItem = Instantiate(fileListboxContent.transform.GetChild(0).gameObject, fileListboxContent.transform);
                    newItem.SetActive(true);
                    string fileName = file.Name;
                    newItem.name = fileName;
                    newItem.GetComponentInChildren<Text>().text = fileName;
                    newItem.GetComponentsInChildren<Image>()[1].gameObject.SetActive(true);
                    newItem.GetComponentsInChildren<Image>()[1].sprite = documentSprite;
                    Button button = newItem.GetComponentInChildren<Button>();
                    button.interactable = true;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectFile(fileName));
                }
            }
        }


        public void SelectFile(string fileName)
        {
            currentPath = Path.GetFullPath(currentPath);
            string separator = "" + Path.DirectorySeparatorChar;
            if (!currentPath.EndsWith(separator))
            {
                currentPath += separator;
            }
            pathInput.text = currentPath + fileName;
        }


        public void OnFileChosen()
        {
            string fileSelectionPath = Path.GetFullPath(pathInput.text);
            Debug.Log($"File selected:\n{fileSelectionPath}");
            Hide();
            OnPathChosen(fileSelectionPath);
        }


        protected override void OnValidate()
        {
            base.OnValidate();

            if (dialogObject != null)
            {
                if (fileListboxContent == null)
                {
                    fileListboxContent = dialogObject.transform.Find("FileListbox/Viewport/Content");
                }
                if (fileTypeDropdown == null)
                {
                    fileTypeDropdown = dialogObject.transform.Find("FileTypeDropdown")?.GetComponent<Dropdown>();
                }
            }
        }

    }
}
