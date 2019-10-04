using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenVISDIVE
{
    public abstract class PathChooser : Dialog
    {
        [SerializeField]
        protected string currentPath = null;

        public string CurrentPath
        {
            get => currentPath;
            set
            {
                currentPath = value;
                if(Active)
                {
                    UpdateDialog(currentPath);
                }
            }
        }

        public event Action<string> PathChosen;

        public override void Show()
        {
            OpenDialog(currentPath);
        }

        public virtual void OpenDialog(string rootPath)
        {
            dialogObject.SetActive(true);
            dialogCanvas?.SetActive(true);
            UpdateDialog(rootPath);
            OnDialogOpened();
        }

        public abstract void UpdateDialog(string rootPath);

        protected virtual void OnPathChosen(string chosenPath)
        {
            PathChosen?.Invoke(chosenPath);
        }
    }
}
