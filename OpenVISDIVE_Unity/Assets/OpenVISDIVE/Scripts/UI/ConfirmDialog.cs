using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenVISDIVE
{
    public class ConfirmDialog : Dialog
    {
        [SerializeField]
        private Button okButton = null;

        [SerializeField]
        private Button cancelButton = null;

        public event Action ConfirmEvent;
        public event Action CancelEvent;

        //public override void Show()
        //{
        //    if (okButton != null && okButton.onClick.GetPersistentEventCount() == 0)
        //    {
        //        okButton.onClick.AddListener(OnConfirm);
        //    }
        //    if (cancelButton != null && cancelButton.onClick.GetPersistentEventCount() == 0)
        //    {
        //        cancelButton.onClick.AddListener(OnCancel);
        //    }
        //    base.Show();
        //}

        public virtual void OnConfirm()
        {
            ConfirmEvent?.Invoke();
            Hide();
        }


        public virtual void OnCancel()
        {
            Hide();
            CancelEvent?.Invoke();
        }


        protected override void OnValidate()
        {
            base.OnValidate();
            if (dialogObject != null)
            {
                if (okButton == null)
                {
                    Transform okButtonTransform = dialogObject.transform.Find("OkButton");
                    okButton = okButtonTransform?.GetComponent<Button>();
                }
                if (cancelButton == null)
                {
                    Transform cancelButtonTransform = dialogObject.transform.Find("CancelButton");
                    cancelButton = cancelButtonTransform?.GetComponent<Button>();
                }
            }
        }
    }

}
