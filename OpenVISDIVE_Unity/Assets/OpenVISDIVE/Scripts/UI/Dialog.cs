using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenVISDIVE
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField]
        protected GameObject dialogObject = null;

        [SerializeField]
        protected GameObject dialogCanvas = null;


        public bool Active
        {
            get => dialogObject ? dialogObject.activeInHierarchy : false;
        }


        public event Action<Dialog> DialogOpened;
        public event Action<Dialog> DialogClosed;


        public virtual void Show()
        {
            dialogObject.SetActive(true);
            dialogCanvas?.SetActive(true);
            UpdateDialog();
            OnDialogOpened();
        }


        public virtual void UpdateDialog()
        {
        }


        public virtual void Hide()
        {
            dialogObject.SetActive(false);
            dialogCanvas?.SetActive(false);
            OnDialogClosed();
        }


        protected virtual void OnDialogOpened()
        {
            DialogOpened?.Invoke(this);
        }


        protected virtual void OnDialogClosed()
        {
            DialogClosed?.Invoke(this);
        }


        protected virtual void OnValidate()
        {
            if (dialogObject == null)
            {
                dialogObject = gameObject;
            }
            if (dialogCanvas == null && dialogObject != null
                && dialogObject.transform.parent != null
                && dialogObject.transform.parent.GetComponent<Canvas>() != null)
            {
                dialogCanvas = dialogObject.transform.parent.gameObject;
            }
        }

    }
}
