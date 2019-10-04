using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OpenVISDIVE
{
    /// <summary>
    /// Scene configuration and editing controller.
    /// </summary>
    public class SceneController : AbstractController
    {
        public enum AxisLockEnum
        {
            PosX = 0x01,
            PosY = 0x02,
            PosZ = 0x04,
            RotX = 0x08,
            RotY = 0x10,
            RotZ = 0x20
        }

        public enum DefaultViewpointEnum
        {
            Corner,
            Left,
            Right,
            Top,
            Bottom,
            Front,
            Back,
        }

        [SerializeField]
        private GameObject currentSelection = null;

        [SerializeField]
        private ViewpointData[] defaultViewpoint = new ViewpointData[7];

        [SerializeField]
        private ViewpointData[] userViewpoint = new ViewpointData[10];

        private int lockedAxes = 0;

        public GameObject CurrentSelection { get => currentSelection; set => currentSelection = value; }

        public void LockPosAxisX(bool axisLock)
        {
            LockAxis(AxisLockEnum.PosX, axisLock);
        }


        public void LockPosAxisY(bool axisLock)
        {
            LockAxis(AxisLockEnum.PosY, axisLock);
        }


        public void LockPosAxisZ(bool axisLock)
        {
            LockAxis(AxisLockEnum.PosZ, axisLock);
        }


        public void LockRotAxisX(bool axisLock)
        {
            LockAxis(AxisLockEnum.RotX, axisLock);
        }


        public void LockRotAxisY(bool axisLock)
        {
            LockAxis(AxisLockEnum.RotY, axisLock);
        }


        public void LockRotAxisZ(bool axisLock)
        {
            LockAxis(AxisLockEnum.RotZ, axisLock);
        }


        public void LockAxis(AxisLockEnum axis, bool axisLock)
        {
            if (axisLock)
            {
                lockedAxes = lockedAxes | (int)axis;
            }
            else
            {
                lockedAxes = lockedAxes & ~(int)axis;
            }
        }


        public void LockSelectionChildren(bool locked)
        {
            Debug.Log(locked ? "Children locked" : "Children unlocked");
        }


        public void SetOrthoView(bool on)
        {
            mainCamera.orthographic = on;
        }


        public void SetDefaultViewpoint(int which)
        {
            mainCamera.transform.position = defaultViewpoint[which].position;
            mainCamera.transform.eulerAngles = defaultViewpoint[which].eulerAngles;
        }


        public void SetUserViewpoint(int which)
        {
            mainCamera.transform.position = userViewpoint[which].position;
            mainCamera.transform.eulerAngles = userViewpoint[which].eulerAngles;
        }


        public void FitView()
        {
            throw new NotImplementedException();
        }


        public void ZoomIn()
        {
            throw new NotImplementedException();
        }


        public void ZoomOut()
        {
            throw new NotImplementedException();
        }


        public void Screenshot()
        {
            string fileName = "OpenDiFac-" + DateTime.Now.ToString("s").Replace('T', '_').Replace(':', '-') + ".png";
            ScreenCapture.CaptureScreenshot(fileName);
#if UNITY_STANDALONE || UNITY_EDITOR
            string basePath = Application.dataPath;
#else
            string basePath = Application.persistentDataPath;
#endif
            string fullPath = basePath + "/"  + fileName;
            Debug.Log($"Screenshot saved to {fullPath}");
#if UNITY_EDITOR
            EditorUtility.RevealInFinder(Application.dataPath);
#endif
        }


        public void TapeMeasure()
        {
            throw new NotImplementedException();
        }


        protected override void Init()
        {
            base.Init();
            if(defaultViewpoint==null) defaultViewpoint = new ViewpointData[7];
            if(userViewpoint==null) userViewpoint = new ViewpointData[10];
            for(int i=0; i< defaultViewpoint.Length; i++)
            {
                if (defaultViewpoint[i]!=null)
                {
                    defaultViewpoint[i] = new ViewpointData();
                }
                defaultViewpoint[i].name = ((DefaultViewpointEnum)i).ToString();
            }
            for(int i=0; i< userViewpoint.Length; i++)
            {
                if (userViewpoint[i] != null)
                {
                    userViewpoint[i] = new ViewpointData();
                }
                userViewpoint[i].name = i.ToString();
            }
            //defaultViewpoint[(int)DefaultViewpointEnum.Corner].position = 
        }

    }
}
