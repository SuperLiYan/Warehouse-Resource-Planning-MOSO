using Warehouse_Sim_opti.Simulation_Model;
using Warehouse_Sim_opti.Simulation_Entities;
using O2DESNet.Distributions;

namespace Warehouse_Sim_opti.Simulation_Modules
{
    internal class OutboundBufferArea:OnCallArea
    {
        #region Dynamics
        int IndexofFirstAvailableStaffofForklifts = 0;
        int IndexofFirstAvailableStaffofPacking = 0;
        int IndexofFirstAvailableForklift = 0;
        public List<SalesOrder> SalesOrders = new List<SalesOrder>();
        public List<Batch> StackedBatches = new List<Batch>();
        public List<Box> StackedBoxes = new List<Box>();
        public List<PickupTask> PickupTasks = new List<PickupTask>();
        public int NumberofOrderedButNotReturnedBoxes = 0;
        #endregion
        public int ArrivalSalesOrder = 0;
        public int ArrivalBoxes = 0;
        public void GetSalesOrder(SalesOrder order)
        {
            //Console.WriteLine($"Get SalesOrder:{ClockTime}");
            ArrivalSalesOrder += 1;
            NumberofOrderedButNotReturnedBoxes += order.QuantitySold;

            SalesOrders.Add(order);
            PickupTasks.Add(new PickupTask(order.SKU, order.QuantitySold));
            StartNextTaskorRestart();
        }

        public void StartNextTaskorRestart()//注意时间限值
        {
            double currentHour = ClockTime.Hour;
            if (Simulation_System.TimeCondition(currentHour))
            {
                AttempttoPick();
                StartPacking();
                StartLoadingBatches();
            }
        }

        #region Pick-up process
        public event Action<string, int> RequestIfShelfAreahasEnoughStorage;
        public bool IfShelfAreahasEnoughStorage = true;

        public void ReceiveIfShelfAreahasEnoughStorage(bool ifShelfAreahasEnoughStorage)
        {
            IfShelfAreahasEnoughStorage = ifShelfAreahasEnoughStorage;
        }

        public void AttempttoPick()//注意时间限值
        {
            if (PickupTasks.Count >= 1)
            {
                IndexofFirstAvailableStaffofForklifts = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
                IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity);

                if (IndexofFirstAvailableStaffofForklifts != -1 && IndexofFirstAvailableForklift != -1)
                {
                    //Console.WriteLine($"Attempt to Pick:{ClockTime}");
                    TraveltoPickup(ManpowerPoolofForklifts[IndexofFirstAvailableStaffofForklifts], ForkliftFleet[IndexofFirstAvailableForklift]);
                }
                    
            }
        }

        public Action<Staff, Forklift, string, int> OnTraveltoShelfArea;
        public void TraveltoPickup(Staff staff, Forklift forklift)
        {
            //Console.WriteLine($"Travel to Pick:{ClockTime}");
            string sku = PickupTasks[0].SKU;
            double envaluatedWeight = PickupTasks[0].PickupQuantity * Input.ProductionList[sku].Weight;
            if (envaluatedWeight <= forklift.MaxCapacity)
            {
                int pickingQuantity = PickupTasks[0].PickupQuantity;

                RequestIfShelfAreahasEnoughStorage.Invoke(PickupTasks[0].SKU, pickingQuantity);
                if (IfShelfAreahasEnoughStorage)
                {
                    staff.BeBusy(); forklift.BeBusy();
                    OnTraveltoShelfArea.Invoke(staff, forklift, sku, pickingQuantity);
                    PickupTasks.RemoveAt(0);
                }
            }
            else
            {
                int pickingQuantity = (int)(forklift.MaxCapacity / Input.ProductionList[sku].Weight);

                RequestIfShelfAreahasEnoughStorage.Invoke(PickupTasks[0].SKU, pickingQuantity);
                if (IfShelfAreahasEnoughStorage)
                {
                    staff.BeBusy(); forklift.BeBusy();
                    OnTraveltoShelfArea.Invoke(staff, forklift, sku, pickingQuantity);

                    PickupTasks[0].PickupQuantity -= pickingQuantity;
                    AttempttoPick();
                }
            }
        }

        public void StaffandForkliftReturnFromShelfArea(Staff staff, Forklift forklift)
        {
            //Console.WriteLine($"Return from ShelfArea:{ClockTime}");
            ArrivalBoxes += forklift.LoadedBoxes.Count;
            StackedBoxes = StackedBoxes.Concat(forklift.LoadedBoxes).ToList<Box>();
            RecordQuantity(ClockTime, StackedBoxes, StackedBatches, forklift.LoadedBoxes.Count);
            forklift.ReleaseBoxes();
            staff.BeIdle(); forklift.BeIdle();

            StackedBoxes.AddRange(forklift.LoadedBoxes);
            forklift.ReleaseBoxes();
            StartNextTaskorRestart();
        }
#endregion

#region Packing Process
        public void StartPacking()
        {
            IndexofFirstAvailableStaffofPacking = ManpowerPoolofPacking.FindIndex(man => man.Status == true);
            IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity);
            if (IndexofFirstAvailableStaffofPacking != -1 && IndexofFirstAvailableForklift != -1 && SalesOrders.Count >= 1)
            {
                string sku = SalesOrders[0].SKU;
                int boxQuantity = SalesOrders[0].QuantitySold;
                List<Box> boxeswithSameSKU = StackedBoxes.Where(box => box.SKU == sku).ToList();
                if (boxeswithSameSKU.Count >= boxQuantity)
                {
                    //Console.WriteLine($"Start Packing:{ClockTime}");
                    ManpowerPoolofPacking[IndexofFirstAvailableStaffofPacking].BeBusy();

                    Staff staff = ManpowerPoolofPacking[IndexofFirstAvailableStaffofPacking];
                    Forklift forklift = ForkliftFleet[IndexofFirstAvailableForklift];

                    string orderNumber = SalesOrders[0].OrderNumber;
                    List<Box> consolidatedBoxes = boxeswithSameSKU.GetRange(0, boxQuantity);                    
                    Batch outboundBatch = new Batch(orderNumber, sku, consolidatedBoxes, ClockTime); 
                    TimeSpan workDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromSeconds(10) * boxQuantity);

                    StackedBoxes.RemoveAll(box => consolidatedBoxes.Contains(box));
                    SalesOrders.RemoveAt(0);

                    Schedule(()=>CompletePacking(staff, outboundBatch), workDuration);
                }
            }
        }

        public void CompletePacking(Staff staff, Batch batch)
        {
            //Console.WriteLine($"Complete Packing:{ClockTime}");
            staff.BeIdle();
            StackedBatches.Add(batch);

            StartNextTaskorRestart();
        }
#endregion

#region Loading Process
        public void StartLoadingBatches()
        {
            if (StackedBatches.Count >= 1)
            {
                IndexofFirstAvailableStaffofForklifts = ManpowerPoolofForklifts.FindIndex(man => man.Status == true);
                IndexofFirstAvailableForklift = ForkliftFleet.FindIndex(forklift => forklift.Status == true && forklift.OnLoading is false && forklift.ExisitingLoad < forklift.MaxCapacity);

                if (IndexofFirstAvailableStaffofForklifts != -1 && IndexofFirstAvailableForklift != -1)
                {
                    //Console.WriteLine($"Start Loading Batches:{ClockTime}");
                    Staff staff = ManpowerPoolofForklifts[IndexofFirstAvailableStaffofForklifts];
                    Forklift forklift = ForkliftFleet[IndexofFirstAvailableForklift];

                    forklift.OnLoading = true;
                    forklift.BeBusy();
                    staff.BeBusy();

                    forklift.LoadBatches(StackedBatches[0]);

                    TimeSpan workDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromSeconds(60));
                    Schedule(()=> CompleteLoadingandTraveltoExit(staff, forklift), workDuration);

                    StackedBatches.RemoveAt(0);
                }
            }
        }

        public void CompleteLoadingandTraveltoExit(Staff staff, Forklift forklift)
        {
            //Console.WriteLine($"Complete Loading and Travel to Exit:{ClockTime}");
            RecordQuantity(ClockTime, StackedBoxes, StackedBatches, 0);
            forklift.OnLoading = false;
            Schedule(() => OnTraveltoExit.Invoke(staff, forklift), Uniform.Sample(DefaultRS, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(8)));
        }
             
        public Action<Staff, Forklift> OnTraveltoExit;
        public void StaffandForkliftReturnFromExit(Staff staff, Forklift forklift)
        {
            //Console.WriteLine($"Return From Exit:{ClockTime}");
            staff.BeIdle();
            forklift.BeIdle();
            StartNextTaskorRestart();
        }
#endregion
        public OutboundBufferArea(int numberInitializedStaffofForklifts, int numberInitializedStaffofPacking, int numberInitializedForklift, int seed) : base(seed)
        {
            Name = "OutboundBufferArea";
            InitializeandReallocate(numberInitializedStaffofForklifts, 0, numberInitializedStaffofPacking, numberInitializedForklift);
        }

        public void Test()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Outbound buffer area test:");
            Console.WriteLine($"ArrivalSalesOrder:{ArrivalSalesOrder}");
            Console.WriteLine($"StackedBoxes:{StackedBoxes.Count}");
            Console.WriteLine($"ArrivalBoxes:{ArrivalBoxes}");
            Console.WriteLine($"CurrentSalesOrder:{SalesOrders.Count}");

            foreach (var salesOrder in SalesOrders)
            {
                Console.WriteLine($"{salesOrder.OrderDate},{salesOrder.OrderNumber}");
            }

            Console.WriteLine($"CurrentBatches:{StackedBatches.Count}");
        }
    }

    public class PickupTask
    {
        public string SKU { get; set; }
        public int PickupQuantity { get; set; }

        public PickupTask(string sku, int pickupQuantity)
        {
            SKU = sku;
            PickupQuantity = pickupQuantity;
        }
    }
}
