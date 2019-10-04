using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenVISDIVE;

namespace OpenVISDIVE
{
    /// <summary>
    /// Controller for simulation and events history.
    /// </summary>
    public class SimulationController : AbstractController
    {
        [SerializeField]
        private SimController simController = null;

        public void LoadHistory()
        {
            simController.LoadSimulation();
            supervisor.ResetCommandList();
        }
    }
}
