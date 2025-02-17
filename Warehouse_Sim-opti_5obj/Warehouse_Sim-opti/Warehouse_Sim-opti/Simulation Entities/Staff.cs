using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse_Sim_opti.Simulation_Modules;

namespace Warehouse_Sim_opti
{
    internal class Staff
    {
        #region Dynamics
        public bool Status = true;
        #endregion
        public void BeIdle() { Status = true; }
        public void BeBusy() { Status = false; }
    }
}
