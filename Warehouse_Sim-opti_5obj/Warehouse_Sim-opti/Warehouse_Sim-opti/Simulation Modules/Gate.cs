using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Distributions;
using Warehouse_Sim_opti.Simulation_Entities;
using Warehouse_Sim_opti.Simulation_Model;

namespace Warehouse_Sim_opti.Simulation_Modules
{
    internal class Gate:OnCallArea
    {
        #region Dynamics
        public List<Batch> StackedBatches = new List<Batch>();
        #endregion

        public event Action<int> MessageInboundBufferArea;
        public void ShipmentArrival(Shipment shipment)
        {
            //Console.WriteLine($"ShipmentArrival:{ClockTime}");
            //ArrivalBatchesNumber += shipment.ArrivalBatches.Count;
            StackedBatches = shipment.ArrivalBatches.Concat(StackedBatches).ToList<Batch>();
            RecordQuantity(ClockTime, null, StackedBatches, shipment.ArrivalBatches.Sum(batch => batch.Quantity));
            MessageInboundBufferArea.Invoke(shipment.ArrivalBatches.Sum(batch => batch.ArrivalBoxes.Count));

            TryStartLoadingBatchesorRestart();
        }

        public void TryStartLoadingBatchesorRestart()
        {
            //Console.WriteLine($"TryStartLoadingBatchesorRestart:{ClockTime}");
            //Console.WriteLine($"{StackedBatches.Count},{ClockTime}");
            double currentHour = ClockTime.Hour;
            if (Simulation_System.TimeCondition(currentHour))
            {
                int IndexofFirstAvailableStaff = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
                int IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true);

                List<Batch> loadedBatches = new List<Batch>();
                double weightofconsolidatedBoxes = 0;

                while (StackedBatches.Count >= 1 && IndexofFirstAvailableStaff != -1 && IndexofFirstAvailableForklift != -1)
                {
                    if (ForkliftFleet[IndexofFirstAvailableForklift].ExisitingLoad + weightofconsolidatedBoxes + StackedBatches[0].Weight <= ForkliftFleet[IndexofFirstAvailableForklift].MaxCapacity)
                    {
                        loadedBatches.Add(StackedBatches[0]);
                        weightofconsolidatedBoxes += StackedBatches[0].Weight;
                        StackedBatches.RemoveAt(0);

                        if (StackedBatches.Count == 0)
                        {
                            StartLoadingBatches(ManpowerPoolofForklifts[IndexofFirstAvailableStaff], ForkliftFleet[IndexofFirstAvailableForklift], loadedBatches);

                            IndexofFirstAvailableStaff = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
                            IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true);

                            loadedBatches.Clear();
                            weightofconsolidatedBoxes = 0;
                        }
                    }
                    else if (ForkliftFleet[IndexofFirstAvailableForklift].ExisitingLoad + weightofconsolidatedBoxes + StackedBatches[0].Weight > ForkliftFleet[IndexofFirstAvailableForklift].MaxCapacity)
                    {
                        StartLoadingBatches(ManpowerPoolofForklifts[IndexofFirstAvailableStaff], ForkliftFleet[IndexofFirstAvailableForklift], loadedBatches);

                        IndexofFirstAvailableStaff = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
                        IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true);

                        loadedBatches.Clear();
                        weightofconsolidatedBoxes = 0;
                    }
                }
            }           
        }

        public void StartLoadingBatches(Staff staff, Forklift forklift, List<Batch> loadedBatches)
        {
            //Console.WriteLine($"StartLoadingBatches:{ClockTime}");
            int loadedBatchesNumber = loadedBatches.Count;
            TimeSpan workingDuration = loadedBatchesNumber * TimeSpan.FromSeconds(60);
            forklift.LoadBatches(loadedBatches);

            staff.BeBusy(); forklift.BeBusy();

            Schedule(()=> FinishLoadingBatchesandTraveltoInboundbufferarea(staff, forklift), workingDuration);
        }

        public event Action<Staff, Forklift> StaffandForkliftOnTraveling;
        public void FinishLoadingBatchesandTraveltoInboundbufferarea(Staff staff, Forklift forklift)
        {
            RecordQuantity(ClockTime, null, StackedBatches, 0);
            //Console.WriteLine($"FinishLoadingBatchesandTraveltoInboundbufferarea:{ClockTime}");
            Schedule(() => StaffandForkliftOnTraveling.Invoke(staff, forklift), 
                Uniform.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(6)));
        }

        //int ArrivalBatchesNumber = 0;
        //int DepartingBatchesNumber = 0;
        public void Test()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Gate test:");
            //foreach (Batch batch in StackedBatches)
            //{
            //    Console.WriteLine($"{batch.BatchNumber},{batch.ProductID},{batch.TimeIn}");
            //}

            Console.WriteLine($"ExistingBatchNumber:{StackedBatches.Count}");
            Console.WriteLine($"StaffNumber:{ManpowerPoolofForklifts.Count}");
            Console.WriteLine($"ForkliftNumber:{ForkliftFleet.Count}");
            //Console.WriteLine($"ArrivalBatchesNumber:{ArrivalBatchesNumber}");
            //Console.WriteLine($"DepartingBatchesNumber:{DepartingBatchesNumber}");
        }

        public Gate(int numberInitializedStaff, int numberInitializedForklift, int seed) :base(seed)
        {
            Name = "Gate";
            InitializeandReallocate(numberInitializedStaff, 0, 0, numberInitializedForklift);
            AfterStaffandForkliftArrival += TryStartLoadingBatchesorRestart;
        }
    }
}
