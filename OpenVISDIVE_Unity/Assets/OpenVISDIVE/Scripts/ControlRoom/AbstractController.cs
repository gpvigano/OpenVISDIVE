using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenVISDIVE
{
    public class AbstractController : MonoBehaviour
    {
        [SerializeField]
        protected Supervisor supervisor = null;

        [SerializeField]
        protected UIController uiController = null;

        [SerializeField]
        protected RaycastPositioner raycastPositioner = null;

        [SerializeField]
        protected Camera mainCamera = null;

        protected virtual void Init()
        {
            if(mainCamera==null)
            {
                mainCamera = Camera.main;
            }
            if (supervisor == null)
            {
                supervisor = GetComponent<Supervisor>();
            }

            if (uiController == null)
            {
                uiController = GetComponent<UIController>();
            }

            if (raycastPositioner == null && mainCamera != null)
            {
                raycastPositioner = mainCamera.GetComponentInChildren<RaycastPositioner>();
            }
        }

        protected virtual void OnValidate()
        {
            Init();
        }

        protected virtual void Reset()
        {
            Init();
        }
    }
}
