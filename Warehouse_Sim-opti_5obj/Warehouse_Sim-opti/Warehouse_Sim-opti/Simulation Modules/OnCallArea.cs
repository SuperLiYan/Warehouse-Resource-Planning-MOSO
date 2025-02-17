using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;
using Warehouse_Sim_opti.Simulation_Entities;

namespace Warehouse_Sim_opti.Simulation_Modules
{
    class OnCallArea:Sandbox
    {
        #region Statics
        /// <value>
        /// The name of this on-call area.
        /// </value>
        public string Name { get; set; }
        #endregion

        #region Dynamics
        public List<Tuple<DateTime, int>> QuantityRecords = new List<Tuple<DateTime, int>>();
        /// <value>
        /// The composition of manpower pool of this on-call area during a period.
        /// </value>
        //public List<Staff> ManpowerPool = new List<Staff>();
        public List<Staff> ManpowerPoolofForklifts = new List<Staff>();
        public List<Staff> ManpowerPoolofUnpacking = new List<Staff>();
        public List<Staff> ManpowerPoolofPacking = new List<Staff>();
        /// <value>
        /// The composition of forklift fleet of this on-call area during a period.
        /// </value>
        public List<Forklift> ForkliftFleet = new List<Forklift>();
        #endregion

        /// <summary>
        /// Recieve a staff after a task.
        /// </summary>
        public void StaffArrival(Staff staff)
        {
            staff.BeIdle();
            AfterStaffArrival.Invoke();
        }
        public event Action AfterStaffArrival;

        /// <summary>
        /// Recieve a forklift after a task.
        /// </summary>
        public void ForkliftArrival(Forklift forklift)
        {
            forklift.BeIdle();
            AfterForkliftArrival.Invoke();
        }
        public event Action AfterForkliftArrival;

        /// <summary>
        /// Recieve a forklift and a staff after a task.
        /// </summary>
        public void StaffandForkliftArrival(Staff staff, Forklift forklift)
        {
            staff.BeIdle(); forklift.BeIdle();
            AfterStaffandForkliftArrival.Invoke();
        }
        public event Action AfterStaffandForkliftArrival;

        public void InitializeandReallocate(int numberInitializedStaffofForklifts, int numberInitializedStaffofUnpacking, int numberInitializedStaffofPacking, int numberInitializedForklift)//先判断 再增减；添加全局变量
        {
            //List<Staff> notAilableStaffs = ManpowerPool.Where(staff => staff.Status is false).ToList();
            List<Staff> notAilableStaffsofForklifts = ManpowerPoolofForklifts.Where(staff => staff.Status is false).ToList();
            List<Staff> notAilableStaffsofUnpacking = ManpowerPoolofUnpacking.Where(staff => staff.Status is false).ToList();
            List<Staff> notAilableStaffsofPacking = ManpowerPoolofPacking.Where(staff => staff.Status is false).ToList();

            List<Forklift> notAilableForklifts = ForkliftFleet.Where(forklift => forklift.Status is false &&
            forklift.OnLoading is false).ToList();

            int numberofNotAilableStaffsofForklifts = notAilableStaffsofForklifts.Count;
            int numberofNotAilableStaffsofUnpacking = notAilableStaffsofUnpacking.Count;
            int numberofNotAilableStaffsofPacking = notAilableStaffsofPacking.Count;

            int numberofNotAilableForklifts = notAilableForklifts.Count;

            if (numberofNotAilableStaffsofForklifts + numberofNotAilableStaffsofUnpacking + numberofNotAilableStaffsofPacking > 0 || numberofNotAilableForklifts > 0)
            {
                //Console.WriteLine($"NotAilableStaffs:{numberofNotAilableStaffs},NotAilableForklifts:{numberofNotAilableForklifts},{Name},{ClockTime}");

                ManpowerPoolofForklifts.RemoveAll(staff => notAilableStaffsofForklifts.Contains(staff) is false);
                for (int s = 0; s < (numberInitializedStaffofForklifts - numberofNotAilableStaffsofForklifts); s++) { ManpowerPoolofForklifts.Add(new Staff()); }

                ManpowerPoolofUnpacking.RemoveAll(staff => notAilableStaffsofUnpacking.Contains(staff) is false);
                for (int s = 0; s < (numberInitializedStaffofUnpacking - numberofNotAilableStaffsofUnpacking); s++) { ManpowerPoolofUnpacking.Add(new Staff()); }

                ManpowerPoolofPacking.RemoveAll(staff => notAilableStaffsofPacking.Contains(staff) is false);
                for (int s = 0; s < (numberInitializedStaffofPacking - numberofNotAilableStaffsofPacking); s++) { ManpowerPoolofPacking.Add(new Staff()); }

                ForkliftFleet.RemoveAll(forklift => notAilableForklifts.Contains(forklift) is false);
                for (int f = 0; f < (numberInitializedForklift- numberofNotAilableForklifts); f++) { ForkliftFleet.Add(new Forklift(200)); }
            }
            else
            {
                ManpowerPoolofForklifts.Clear(); for (int s = 0; s < numberInitializedStaffofForklifts; s++) { ManpowerPoolofForklifts.Add(new Staff()); }
                ManpowerPoolofUnpacking.Clear(); for (int s = 0; s < numberInitializedStaffofUnpacking; s++) { ManpowerPoolofUnpacking.Add(new Staff()); }
                ManpowerPoolofPacking.Clear(); for (int s = 0; s < numberInitializedStaffofPacking; s++) { ManpowerPoolofPacking.Add(new Staff()); }

                ForkliftFleet.Clear(); for (int f = 0; f < numberInitializedForklift; f++) { ForkliftFleet.Add(new Forklift(200)); }
            }
        }

        public void RecordQuantity(DateTime clockTime, List<Box> stackedBoxes, List<Batch> stackedBatches, int receivingQuantity)
        {
            ReceivingBoxesQuantity += receivingQuantity;
            int stackedQuantity = stackedBoxes == null ? 0: stackedBoxes.Count;
            stackedQuantity += stackedBatches.Sum(batch => batch.Quantity);

            QuantityRecords.Add(new Tuple<DateTime, int>(clockTime, stackedQuantity));
        }

        int ReceivingBoxesQuantity = 0;
        public TimeSpan AverageStackingTime()
        {
            TimeSpan totalStackingTime = TimeSpan.FromMinutes(0);
            for (int record = 1; record < QuantityRecords.Count; record++)
            {
                totalStackingTime += (QuantityRecords[record].Item1 - QuantityRecords[record - 1].Item1) * QuantityRecords[record - 1].Item2;
            }

            //Console.WriteLine($"{Name}:{(totalStackingTime / ReceivingBoxesQuantity).TotalMinutes.ToString("000.000")}," +
            //    $"{ManpowerPool.Count}, {ForkliftFleet.Count}");
            return totalStackingTime / ReceivingBoxesQuantity;
        }

        public OnCallArea(int seed) : base(seed)
        {
            QuantityRecords.Add(new Tuple<DateTime, int>(Input.StartTime, 0));
        }
    }
}
