using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet.Distributions;
using Warehouse_Sim_opti.Simulation_Entities;
using Warehouse_Sim_opti.Simulation_Model;

namespace Warehouse_Sim_opti.Simulation_Modules
{
    internal class InboundBufferArea:OnCallArea
    {
        #region Dynamics
        int IndexofFirstAvailableStaffofForklifts = 0;
        int IndexofFirstAvailableForklift = 0;
        public List<Batch> StackedBatches = new List<Batch>();
        public List<Box> StackedBoxes = new List<Box>();
        public int NumberofEnteringButNotLoadedBoxes = 0;
        public bool IfShelfAreahasFreeCapacity = true;
        #endregion

        public void GetMessageFromGate(int newArrivalBoxesNumber)
        {
            NumberofEnteringButNotLoadedBoxes += newArrivalBoxesNumber;
        }

        public event Action<int> RequestIfShelfAreahasFreeCapacity;

        public void GetMessageFromShelfArea(bool ifShelfAreahasFreeCapacity)
        {
            IfShelfAreahasFreeCapacity = ifShelfAreahasFreeCapacity;
        }

        public event Action<Staff, Forklift> StaffandForkliftOnReturnToGate;
        public void StaffandForkliftArrivalFromGate(Staff staff, Forklift forklift)
        {
            //Console.WriteLine($"StaffandForkliftArrivalFromGate:{ClockTime}");
            ArrivalBatchesNumber += forklift.loadedBatches.Count;
            StackedBatches = StackedBatches.Concat(forklift.loadedBatches).ToList<Batch>();
            RecordQuantity(ClockTime, StackedBoxes, StackedBatches, forklift.loadedBatches.Count);
            forklift.ReleaseBatches();
            StartUnpacking();

            Random random = new Random((int)DateTime.Now.Ticks);
            Schedule(() => StaffandForkliftOnReturnToGate.Invoke(staff, forklift), 
                Uniform.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(6)));
        }

        public void StartUnpacking()
        {
            while (StackedBatches.Count >= 1 && ManpowerPoolofUnpacking.Exists(man => man.Status is true))
            {
                //Console.WriteLine($"StartUnpacking:{ClockTime}");
                int IndexofFirstAvailableStaff = ManpowerPoolofUnpacking.FindIndex(man => man.Status == true);
                ManpowerPoolofUnpacking[IndexofFirstAvailableStaff].BeBusy();
                
                Batch batch = new Batch(StackedBatches[0].BatchNumber, StackedBatches[0].ProductID, StackedBatches[0].ArrivalBoxes, StackedBatches[0].TimeIn);

                Schedule(() => CompleteUnpacking(ManpowerPoolofUnpacking[IndexofFirstAvailableStaff], batch), 
                    Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromMinutes(3)));
                
                StackedBatches.RemoveAt(0);//FIFO
            }
        }

        public void CompleteUnpacking(Staff staff, Batch batch)
        {
            //Console.WriteLine($"CompleteUnpacking:{ClockTime}");
            StackedBoxes = batch.ArrivalBoxes.Concat(StackedBoxes).ToList<Box>(); 
            staff.BeIdle();

            GetNextTaskorRestart();
        }

        double weightofconsolidatedBoxes = 0;
        List<Box> consolidatedBoxes = new List<Box>();

        public void ConsolidateandTryLoadingBoxes()
        {
            IndexofFirstAvailableStaffofForklifts = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
            IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity);

            while (StackedBoxes.Count >= 1 && IndexofFirstAvailableStaffofForklifts != -1 && IndexofFirstAvailableForklift != -1)
            {
                if (ForkliftFleet[IndexofFirstAvailableForklift].ExisitingLoad + weightofconsolidatedBoxes + StackedBoxes[0].Weight <= ForkliftFleet[IndexofFirstAvailableForklift].MaxCapacity)
                {
                    consolidatedBoxes.Add(StackedBoxes[0]);
                    weightofconsolidatedBoxes += StackedBoxes[0].Weight;
                    NumberofEnteringButNotLoadedBoxes --;
                    StackedBoxes.RemoveAt(0);

                    if (StackedBoxes.Count == 0 && StackedBatches.Count == 0 && NumberofEnteringButNotLoadedBoxes == 0)//stop loading for this forklift duet to no enough boxes;
                    {
                        //Console.WriteLine($"ConsolidateandTryLoadingBoxes:{ClockTime}");
                        StartLoadingBoxes(ManpowerPoolofForklifts[IndexofFirstAvailableStaffofForklifts], ForkliftFleet[IndexofFirstAvailableForklift], consolidatedBoxes);

                        IndexofFirstAvailableStaffofForklifts = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
                        IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity);
                        consolidatedBoxes.Clear();
                        weightofconsolidatedBoxes = 0;
                    }
                }
                else if (ForkliftFleet[IndexofFirstAvailableForklift].ExisitingLoad + weightofconsolidatedBoxes + StackedBoxes[0].Weight > ForkliftFleet[IndexofFirstAvailableForklift].MaxCapacity)//stop loading for this forklift duet to no enough capacity;
                {
                    //Console.WriteLine($"ConsolidateandTryLoadingBoxes:{ClockTime}");
                    ForkliftFleet[IndexofFirstAvailableForklift].BeFull();
                    StartLoadingBoxes(ManpowerPoolofForklifts[IndexofFirstAvailableStaffofForklifts], ForkliftFleet[IndexofFirstAvailableForklift], consolidatedBoxes);

                    IndexofFirstAvailableStaffofForklifts = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
                    IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity);
                    consolidatedBoxes.Clear();
                    weightofconsolidatedBoxes = 0;
                }
            }
        }

        public void StartLoadingBoxes(Staff staff, Forklift forklift, List<Box> consolidatedBoxes)
        {
            //Console.WriteLine($"StartLoadingBoxes:{ClockTime}");
            if (forklift.OnLoading is false)
            {
                LoadedBoxesNumber += consolidatedBoxes.Count;
                int consolidatedBoxesnumber = consolidatedBoxes.Count;

                TimeSpan workingDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), consolidatedBoxesnumber * TimeSpan.FromSeconds(10));
                forklift.LoadBoxes(consolidatedBoxes);

                staff.BeBusy(); forklift.BeOnLoading();
                forklift.UpdateExpectedFinishingLoadingTime(ClockTime + workingDuration);
                Schedule(() => FinishLoadingBoxes(staff, forklift), workingDuration);

                //Console.WriteLine($"{forklift.LoadedBoxes.Count},{ClockTime},{ManpowerPool.Where(man => man.Status is true).ToList<Staff>().Count}," +
                //$"{ForkliftFleet.Where(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity).ToList<Forklift>().Count},{NumberofEnteringButNotLoadedBoxes},{StackedBatches.Count}");
            }
            else
            {
                Schedule(() => StartLoadingBoxes(staff, forklift, consolidatedBoxes), forklift.ExpectedFinishingLoadingTime);               
            }
        }

        public void FinishLoadingBoxes(Staff staff, Forklift forklift)
        {
            //Console.WriteLine($"FinishLoadingBoxes:{ClockTime}");
            staff.BeIdle(); forklift.BeNotOnLoading();

            //Console.WriteLine(forklift.ExpectedFinishingLoadingTime);

            //if (forklift.IfFull is true || NumberofEnteringButNotLoadedBoxes == 0)
            //{
            //    TransferringToShelfArea(staff, forklift);
            //}

            GetNextTaskorRestart();
        }

        public void GetNextTaskorRestart()
        {
            double currentHour = ClockTime.Hour;
            if (Simulation_System.TimeCondition(currentHour)) 
            {
                StartUnpacking();// next unpacking
                ConsolidateandTryLoadingBoxes();//next consolidating
                AttempttoTransferringToShelfArea();
            }
        }


        public void AttempttoTransferringToShelfArea()
        {
            IndexofFirstAvailableStaffofForklifts = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
            List<Forklift> availableForklifts = ForkliftFleet.Where(forklift => forklift.Status == true && forklift.OnLoading is false && 0 < forklift.ExisitingLoad).ToList();

            if (IndexofFirstAvailableStaffofForklifts != -1 && availableForklifts.Count > 0)
            {
                List<Forklift> forkliftsofFullCapacity = availableForklifts.Where(forklift => forklift.IfFull is true).ToList();
                if (forkliftsofFullCapacity.Count > 1)
                {
                    //Console.WriteLine($"AttempttoTransferringToShelfArea:{ClockTime}");
                    IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift == forkliftsofFullCapacity[0]);

                    RequestIfShelfAreahasFreeCapacity.Invoke(ForkliftFleet[IndexofFirstAvailableForklift].LoadedBoxes.Count);
                    if(IfShelfAreahasFreeCapacity)
                        TransferringToShelfArea(ManpowerPoolofForklifts[IndexofFirstAvailableStaffofForklifts], ForkliftFleet[IndexofFirstAvailableForklift]);
                }
                else if (NumberofEnteringButNotLoadedBoxes == 0)
                {
                    //Console.WriteLine($"AttempttoTransferringToShelfArea:{ClockTime}");
                    IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift == availableForklifts[0]);
                    RequestIfShelfAreahasFreeCapacity.Invoke(ForkliftFleet[IndexofFirstAvailableForklift].LoadedBoxes.Count);
                    if (IfShelfAreahasFreeCapacity)
                        TransferringToShelfArea(ManpowerPoolofForklifts[IndexofFirstAvailableStaffofForklifts], ForkliftFleet[IndexofFirstAvailableForklift]);
                }
            }
        }

        public event Action<Staff, Forklift> OnTransferringToShelfArea;
        public void TransferringToShelfArea(Staff staff, Forklift forklift)
        {
            RecordQuantity(ClockTime, StackedBoxes, StackedBatches, 0);
            //Console.WriteLine($"TransferringToShelfArea:{ClockTime}");
            DepartingBoxesNumber += forklift.LoadedBoxes.Count;

            staff.BeBusy();
            forklift.BeBusy();
            OnTransferringToShelfArea.Invoke(staff, forklift);
        }

        int ArrivalBatchesNumber = 0;
        int DepartingBoxesNumber = 0;
        int LoadedBoxesNumber = 0;
        int FinishedLoadingBoxesNumber = 0;
        public void Test()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Inbound buffer area test:");
            Console.WriteLine($"BatchNumber:{StackedBatches.Count}");
            Console.WriteLine($"StaffNumberofForklifts:{ManpowerPoolofForklifts.Count}");
            Console.WriteLine($"StaffNumberofUnpacking:{ManpowerPoolofUnpacking.Count}");
            Console.WriteLine($"ForkliftNumber:{ForkliftFleet.Count}");
            Console.WriteLine($"ArrivalBatches:{ArrivalBatchesNumber}");
            Console.WriteLine($"StackedBoxesNumber:{StackedBoxes.Count}");
            Console.WriteLine($"DepartingBoxesNumber:{DepartingBoxesNumber}");
            Console.WriteLine($"LoadedBoxesNumber:{LoadedBoxesNumber}");
            Console.WriteLine($"FinishedLoadingBoxesNumber:{FinishedLoadingBoxesNumber}");
        }

        public InboundBufferArea(int numberInitializedStaffofForklifts, int numberInitializedStaffofUnpacking, int numberInitializedForklift, int seed) : base(seed)
        {
            Name = "InboundBufferArea";
            InitializeandReallocate(numberInitializedStaffofForklifts, numberInitializedStaffofUnpacking, 0, numberInitializedForklift);
            AfterStaffandForkliftArrival += GetNextTaskorRestart;

            IndexofFirstAvailableStaffofForklifts = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
            IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity);
        }
    }
}
