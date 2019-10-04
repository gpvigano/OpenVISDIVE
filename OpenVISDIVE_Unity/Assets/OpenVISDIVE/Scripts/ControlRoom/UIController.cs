using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenVISDIVE;

namespace OpenVISDIVE
{
    /// <summary>
    /// Controller for the user interface.
    /// Control the UI panels flow according to the internal state,
    /// references are kept to UI elements in order to control them.
    /// </summary>
    public class UIController : AbstractController
    {
        [SerializeField]
        private DirectoryChooser directoryChooser = null;

        [SerializeField]
        private FileChooser fileChooser = null;

        [SerializeField]
        private ConfirmDialog confirmDialog = null;

        [SerializeField]
        private RootPathSettings pathSettings = null;

        [SerializeField]
        private GameObject menuRootObject = null;

        [SerializeField]
        private Button undoButton = null;

        [SerializeField]
        private Button redoButton = null;

        private List<GameObject> menuHistory = new List<GameObject>();

        public string RootPath
        {
            get
            {
                return pathSettings != null ? pathSettings.RootPath : "";
            }
        }

        public event Action<string> FolderSelected;
        public event Action<string> FileSelected;
        public event Action Confirmed;
        public event Action Cancelled;


        public void SetUndoButtonActive(bool active)
        {
            undoButton.interactable = active;
        }


        public void SetRedoButtonActive(bool active)
        {
            redoButton.interactable = active;
        }


        public void SetFileTypeFilter(string[] descriptions, string[] extensions, int defaultFilter = 0)
        {
            fileChooser.SetFileTypes(descriptions, extensions, defaultFilter);
        }


        public void OpenMenu(string menuName)
        {
            Transform menuTransform = menuRootObject.transform.Find(menuName);
            if (menuTransform != null)
            {
                OpenMenu(menuTransform.gameObject);
            }
            else
            {
                Debug.LogError($"Menu {menuName} not found.");
            }
        }


        public void OpenMenu(GameObject menuObject)
        {
            raycastPositioner.enabled = false;
            // show the menu canvas
            menuRootObject.SetActive(true);
            // show the selected menu
            menuObject.SetActive(true);
            menuHistory.Add(menuObject);
            // close the other menus
            for (int i = 0; i < menuRootObject.transform.childCount; i++)
            {
                Transform menu = menuRootObject.transform.GetChild(i);
                if (menu != menuObject.transform && menu.gameObject.activeSelf)
                {
                    menu.gameObject.SetActive(false);
                }
            }
        }


        public void CloseAllMenus()
        {
            raycastPositioner.enabled = true;
            // clear menu history
            menuHistory.Clear();
            // close all the menus
            for (int i = 0; i < menuRootObject.transform.childCount; i++)
            {
                Transform menu = menuRootObject.transform.GetChild(i);
                menu.gameObject.SetActive(false);
            }
            // hide the menu canvas
            menuRootObject.SetActive(false);
        }


        public void OpenPreviousMenu()
        {
            // in the history there is also the current menu
            if (menuHistory.Count < 2)
            {
                CloseAllMenus();
                return;
            }
            GameObject prevMenu = menuHistory[menuHistory.Count - 2];
            menuHistory.RemoveRange(menuHistory.Count - 2, 2);
            OpenMenu(prevMenu);
        }


        public void OpenDirectoryChooserDialog(string relativePath)
        {
            directoryChooser.CurrentPath = pathSettings.FullPath(relativePath);
            OpenDirectoryChooserDialog();
        }


        public void OpenDirectoryChooserDialog()
        {
            OpenDialog(directoryChooser);
        }


        public void CloseDirectoryChooserDialog()
        {
            CloseDialog(directoryChooser);
        }


        public void OpenFileChooserDialog(string relativePath)
        {
            fileChooser.CurrentPath = pathSettings.FullPath(relativePath);
            OpenDialog(fileChooser);
        }


        public void OpenFileChooserDialog()
        {
            OpenDialog(fileChooser);
        }


        public void CloseFileChooserDialog()
        {
            if (fileChooser != null)
            {
                fileChooser.Hide();
            }
        }


        public void OpenDialog(Dialog dialog)
        {
            CloseAllMenus();
            raycastPositioner.enabled = false;
            dialog.DialogClosed += OnDialogClosed;
            dialog.Show();
        }

        private void OnDialogClosed(Dialog dialog)
        {
            dialog.DialogClosed -= OnDialogClosed;
            raycastPositioner.enabled = true;
        }

        public void CloseDialog(Dialog dialog)
        {
            dialog.Hide();
        }


        public void OpenConfirmDialog()
        {
            OpenDialog(confirmDialog);
        }


        public void CloseConfirmDialog()
        {
            CloseDialog(confirmDialog);
        }


        public void OnDirectoryChosen(string folderSelectionPath)
        {
            FolderSelected?.Invoke(folderSelectionPath);
        }


        public void OnFileChosen(string fileSelectionPath)
        {
            FileSelected?.Invoke(fileSelectionPath);
        }


        public void OnConfirmed()
        {
            Confirmed?.Invoke();
        }


        public void OnCancelled()
        {
            Cancelled?.Invoke();
        }


        private void OnEnable()
        {
            if (directoryChooser != null)
            {
                directoryChooser.PathChosen += OnDirectoryChosen;
            }
            if (fileChooser != null)
            {
                fileChooser.PathChosen += OnFileChosen;
            }
            if (confirmDialog != null)
            {
                confirmDialog.ConfirmEvent += OnConfirmed;
                confirmDialog.CancelEvent += OnCancelled;
            }
        }


        private void OnDisable()
        {
            if (directoryChooser != null)
            {
                directoryChooser.PathChosen -= OnDirectoryChosen;
            }
            if (fileChooser != null)
            {
                fileChooser.PathChosen -= OnFileChosen;
            }
            if (confirmDialog != null)
            {
                confirmDialog.ConfirmEvent -= OnConfirmed;
                confirmDialog.CancelEvent -= OnCancelled;
            }
        }


        protected override void OnValidate()
        {
            base.OnValidate();
            uiController = this;
            if (pathSettings == null)
            {
                pathSettings = GetComponent<RootPathSettings>();
            }
        }


        private void Start()
        {
            // disable menu buttons without a function defined
            if (menuRootObject != null)
            {
                Button[] menuButtons = menuRootObject.GetComponentsInChildren<Button>();
                foreach (Button button in menuButtons)
                {
                    if (button.onClick.GetPersistentEventCount() == 0)
                    {
                        button.interactable = false;
                    }
                }
                menuRootObject.SetActive(false);
            }
            // ensure all dialogs are closed
            CloseDirectoryChooserDialog();
            CloseFileChooserDialog();
            CloseConfirmDialog();
            // initialize file/directory choose dialogs
            string rootPath = Path.GetFullPath(RootPath);
            if (directoryChooser != null)
            {
                directoryChooser.CurrentPath = rootPath;
            }
            if (fileChooser != null)
            {
                fileChooser.CurrentPath = rootPath;
            }
        }
    }
}

