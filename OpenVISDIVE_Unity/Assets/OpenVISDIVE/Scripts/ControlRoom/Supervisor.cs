using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenVISDIVE
{
    /// <summary>
    /// Global control over the entire environment.
    /// </summary>
    public class Supervisor : MonoBehaviour
    {
        [SerializeField]
        private UIController uiController = null;

        private List<Command> commands = new List<Command>();
        private int currentCommand = -1;


        public void ResetCommandList()
        {
            commands.Clear();
            currentCommand = -1;
        }


        public void Do(Command cmd)
        {
            if (cmd.TryExecute())
            {
                if (currentCommand < commands.Count - 1)
                {
                    commands.RemoveRange(currentCommand + 1, commands.Count - currentCommand - 1);
                }
                commands.Add(cmd);
                currentCommand = commands.Count - 1;
                uiController.SetUndoButtonActive(true);
                uiController.SetRedoButtonActive(false);
            }
        }


        public void Undo()
        {
            if (currentCommand >= 0)
            {
                commands[currentCommand].TryRestore();
                currentCommand--;
                if (currentCommand < 0)
                {
                    uiController.SetUndoButtonActive(false);
                }
                uiController.SetRedoButtonActive(true);
            }
            else
            {
                Debug.Log("What is done is done...");
            }
        }


        public void Redo()
        {
            if (currentCommand < commands.Count - 1)
            {
                currentCommand++;
                commands[currentCommand].TryExecute();
                if (currentCommand >= commands.Count - 1)
                {
                    uiController.SetRedoButtonActive(false);
                }
                uiController.SetUndoButtonActive(true);
            }
            else
            {
                Debug.Log("Nothing to redo...");
            }
        }

        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }


        private void Start()
        {
            uiController.SetUndoButtonActive(false);
            uiController.SetRedoButtonActive(false);
        }


        private void OnValidate()
        {
            if (uiController == null)
            {
                uiController = GetComponent<UIController>();
            }
        }
    }
}
