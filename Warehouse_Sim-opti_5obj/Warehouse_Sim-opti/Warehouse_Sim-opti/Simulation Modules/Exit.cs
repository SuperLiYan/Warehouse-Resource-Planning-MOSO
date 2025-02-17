using O2DESNet;
using O2DESNet.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Sim_opti.Simulation_Modules
{
    internal class Exit:Sandbox
    {
        public int ArrivalBacthes = 0;
        public Action<Staff, Forklift> OnreturntoOutboundBufferArea;
        public void ReceiveBathces(Staff staff, Forklift forklift)
        {
            //Console.WriteLine($"Receive Bathces:{ClockTime}");
            ArrivalBacthes++;
            forklift.ReleaseBatches();
            Schedule(() => OnreturntoOutboundBufferArea.Invoke(staff, forklift), Uniform.Sample(DefaultRS, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(8)));
        }

        public void Test()
        {
            Console.WriteLine("============================================");
            Console.WriteLine("Exit test:");
            Console.WriteLine($"ArrivalBacthes:{ArrivalBacthes}");
        }
    }
}
