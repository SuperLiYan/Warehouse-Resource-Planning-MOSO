using O2DESNet;
using System.Xml.Linq;
using Warehouse_Sim_opti.Simulation_Entities;
using Warehouse_Sim_opti.Tool;
using O2DESNet.Distributions;

namespace Warehouse_Sim_opti.Simulation_Modules
{
    internal class ShelfArea:Sandbox
    {
        public double[] LocationofInboudbufferAreaEntrance = new double[2] {0, 0};
        public double[] LocationofOutboudbufferAreaEntrance = new double[2] {52, 0};
        public List<Rack> Racks = new List<Rack>();
        public Dictionary<string, List<StorageInformation>> StorageInformations = new Dictionary<string, List<StorageInformation>>();
        public event Action<Staff, Forklift> StaffandForkliftOnReturningToInboundBufferArea;
        public event Action<Staff, Forklift> StaffandForkliftOnReturningToOutboundBufferArea;
        public double ForkliftSpeed = 1.0/6.0; // 1m/s
        public int StackedBoxesQuantity { get { return Racks.Sum(rack => rack.RestCapacity); } }
        public int AvailableCapacity { get { return Racks.Sum(rack => rack.AvailableCapacity) - 
                    RunningForklifts.Sum(forklift => forklift.LoadedBoxes.Count); } }
        public List<Forklift> RunningForklifts = new List<Forklift>();
        public Dictionary<string, int> OnPickingQuantity = new Dictionary<string, int>();
#region Putaway Process      
        public int ArrivalBoxesNumber = 0;
        public int PutDownBoxesNumber = 0;
        public int PickedBoxesNumber = 0;
        public void StaffandForkliftArrivalfromInboundbufferArea(Staff staff, Forklift forklift)
        {
            RunningForklifts.Add(forklift);
            //Console.WriteLine($"StaffandForkliftArrivalfromInboundbufferArea:{ClockTime}");
            ArrivalBoxesNumber += forklift.LoadedBoxes.Count;
            forklift.LoadedBoxes.OrderBy(box => int.Parse(box.SKU));//increase

            AttemptToPutawayNumber(staff, forklift, LocationofInboudbufferAreaEntrance);
        }

        public void AttemptToPutawayNumber(Staff staff, Forklift forklift, double[] currentLocation)
        {
            if (forklift.LoadedBoxes.Count >= 1)
            {
                //Console.WriteLine($"AttemptToPutawayNumber:{ClockTime},({currentLocation[0]},{currentLocation[1]})");
                string currentSKU = forklift.LoadedBoxes[0].SKU;
                Tuple<Rack, double> nextRackandDistance = NearestAvailableRack(currentLocation, currentSKU);//Get next rack and distance;

                int putDownNumber = Math.Min(forklift.LoadedBoxes.Count(Box => Box.SKU == currentSKU), nextRackandDistance.Item1.AvailableCapacity);
                List<Box> putDownBoxes = forklift.LoadedBoxes.GetRange(0, putDownNumber);

                TimeSpan TravelDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromSeconds(nextRackandDistance.Item2 / ForkliftSpeed));
                TimeSpan putDownDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), putDownNumber * TimeSpan.FromSeconds(10));
                TimeSpan putAwayDuration = TravelDuration + putDownDuration;//Put down duration;

                nextRackandDistance.Item1.BookCapacity(putDownBoxes);
                Schedule(() => PutawayNumber(staff, forklift, nextRackandDistance.Item1, putDownBoxes), putAwayDuration);
            }
            else
            {
                //Console.WriteLine($"AttempttoReturntoInboundBufferArea:{ClockTime},({currentLocation[0]},{currentLocation[1]})");
                double returnDistance = DistanceBetweenControlPoints(currentLocation, LocationofInboudbufferAreaEntrance);

                TimeSpan returnDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromSeconds(returnDistance / ForkliftSpeed));
                Schedule(() => CompletePutawayNumberandReturntoInboundBufferArea(staff, forklift), returnDuration);
            }
        }

        public void PutawayNumber(Staff staff, Forklift forklift, Rack currentRack, List<Box> putDownBoxes)
        {
            //Console.WriteLine($"PutawayNumber:{ClockTime},({currentRack.Location[0]},{currentRack.Location[1]}),{putDownBoxes.Count}");
            int putDownNumber = putDownBoxes.Count();
            PutDownBoxesNumber += putDownNumber;

            RecordInformationofPutDownBoxes(putDownBoxes, currentRack);
            forklift.PutDownBoxes(0, putDownNumber);
            currentRack.ReceiveBoxes(putDownBoxes);

			AttemptToPutawayNumber(staff, forklift, currentRack.Location);
		}

        public void CompletePutawayNumberandReturntoInboundBufferArea(Staff staff, Forklift forklift)
        {
            RunningForklifts.Remove(forklift);
            //Console.WriteLine($"CompletePutawayNumberandReturntoInboundBufferArea:{ClockTime}");
            forklift.ReleaseBoxes();
            StaffandForkliftOnReturningToInboundBufferArea.Invoke(staff, forklift);
        }

        public void RecordInformationofPutDownBoxes(List<Box> putDownBoxes, Rack currentRack)
        {
            string currentSKU = putDownBoxes[0].SKU;
            int putDownNumber = 0;

            for (int bo = 0; bo < putDownBoxes.Count; bo++)
            {
                if (currentSKU == putDownBoxes[bo].SKU && bo != putDownBoxes.Count - 1)
                {
                    putDownNumber++;
                }
                else if (currentSKU != putDownBoxes[bo].SKU && bo != putDownBoxes.Count - 1)
                {
                    StorageInformations[currentSKU].Add(new StorageInformation(currentSKU, putDownNumber, ClockTime, currentRack));
                    putDownNumber = 1;
                    currentSKU = putDownBoxes[bo].SKU;
                }
                else if (bo == putDownBoxes.Count - 1)
                {
                    putDownNumber++;
                    StorageInformations[currentSKU].Add(new StorageInformation(currentSKU, putDownNumber, ClockTime, currentRack));
                }
            }
        }

        public event Action<bool> FeedbackIfShelfAreahasFreeCapacity;
        public void GetRequestFormInboundBufferArea(int expectedQuantity)
        {
            if (AvailableCapacity >= expectedQuantity)
                FeedbackIfShelfAreahasFreeCapacity.Invoke(true);
            else
                FeedbackIfShelfAreahasFreeCapacity.Invoke(false);
        }
        #endregion

        #region Pick-up process
        public event Action<bool> FeedbackIfShelfAreahasEnoughStorage;
        public void ReceiveMessagefromOutboundbufferArea(string sku, int expectedPickingQuantity)
        {
            if (StorageInformations[sku].Sum(infor => infor.StackedProductionQuantity) - OnPickingQuantity[sku] >= expectedPickingQuantity)
                FeedbackIfShelfAreahasEnoughStorage.Invoke(true);
            else
                FeedbackIfShelfAreahasEnoughStorage.Invoke(false);
        }

        public void StaffandForkliftArrivalfromOutboundbufferArea(Staff staff, Forklift forklift, string pickedSKU, int remainedPickingQuantity)
        {
            OnPickingQuantity[pickedSKU] += remainedPickingQuantity;
            StorageInformation earliestStorage = StorageInformations[pickedSKU][0];//FIFO
            Rack nextRack = earliestStorage.StoringRack;

            int availablePickingQuantity = earliestStorage.StackedProductionQuantity;
            int pickingQuantity = Math.Min(remainedPickingQuantity, availablePickingQuantity);

            remainedPickingQuantity -= pickingQuantity;
            OnPickingQuantity[pickedSKU] -= pickingQuantity;
            earliestStorage.StackedProductionQuantity -= pickingQuantity;
            if (earliestStorage.StackedProductionQuantity == 0)
            {
                StorageInformations[pickedSKU].RemoveAt(0);
            }

            double distance = DistanceBetweenControlPoints(LocationofOutboudbufferAreaEntrance, nextRack.Location);
            TimeSpan workDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromSeconds(distance / ForkliftSpeed) + pickingQuantity * TimeSpan.FromSeconds(10));

            Schedule(()=> PickupBoxesAtRack(staff, forklift, nextRack, pickedSKU, remainedPickingQuantity, 
                availablePickingQuantity, pickingQuantity), workDuration);
        }

        public void PickupBoxesAtRack(Staff staff, Forklift forklift, Rack currentRack,
            string pickedSKU, int remainedPickingQuantity, int availablePickingQuantity, int pickingQuantity)
        {
            forklift.LoadBoxes(currentRack.PickupBoxes(pickedSKU, pickingQuantity));

            if (remainedPickingQuantity >= 1)
            {
                StorageInformation nextStorage = StorageInformations[pickedSKU][0];//FIFO
                Rack nextRack = nextStorage.StoringRack;

                availablePickingQuantity = nextStorage.StackedProductionQuantity;
                pickingQuantity = Math.Min(remainedPickingQuantity, availablePickingQuantity);

                remainedPickingQuantity -= pickingQuantity;
                OnPickingQuantity[pickedSKU] -= pickingQuantity;
                nextStorage.StackedProductionQuantity -= pickingQuantity;
                if (nextStorage.StackedProductionQuantity == 0)
                {
                    StorageInformations[pickedSKU].RemoveAt(0);
                }

                double distance = DistanceBetweenControlPoints(currentRack.Location, nextRack.Location) ;
                TimeSpan workDuration = Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromSeconds(distance / ForkliftSpeed) + pickingQuantity * TimeSpan.FromSeconds(10));

                Schedule(() => PickupBoxesAtRack(staff, forklift, nextRack, pickedSKU, remainedPickingQuantity,
                    availablePickingQuantity, pickingQuantity), workDuration);
            }
            else
            {
                //Console.WriteLine(forklift.LoadedBoxes.Count);
                double returnDistance = Exponential.Sample(new Random((int)DateTime.Now.Ticks), DistanceBetweenControlPoints(currentRack.Location, LocationofOutboudbufferAreaEntrance));
                Schedule(()=> StaffandForkliftOnReturningToOutboundBufferArea.Invoke(staff, forklift),
                    Exponential.Sample(new Random((int)DateTime.Now.Ticks), TimeSpan.FromSeconds(returnDistance / ForkliftSpeed)));
            }               
        }
        #endregion

        #region Initialize Inventory Process
        public void InitializeInventory()
        {
            int batchNumber = Input.Inventory.Count;
            int currentRack = 0;
            int CurrentLevel = 0;

            for (int ba = 0; ba < batchNumber; ba++)
            {
                int putDownNumber = 0;
                string sku = Input.Inventory[ba][1];
                int quantity = int.Parse(Input.Inventory[ba][2]);
                ProducationInformation boxProperties = Input.ProductionList[sku];

                DateTime timeIn = StringtoFormalDatetime.StringToDatetime(Input.Inventory[ba][3]);
                List<Box> boxesofBatch = new List<Box>();

                for (int bo = 0; bo < quantity; bo++)
                {

                    Box box = new Box(boxProperties.SKU, boxProperties.Length, boxProperties.Hight, boxProperties.Width, boxProperties.Weight, timeIn);

                    if (Racks[currentRack].Levels[CurrentLevel].StackedBoxes.Count < Racks[currentRack].Levels[CurrentLevel].MaxCapacity)
                    {
                        putDownNumber++;
                        //Console.WriteLine($"box:{box.SKU}");
                        Racks[currentRack].Levels[CurrentLevel].StackedBoxes.Add(box);                        
                    }
                    else if (Racks[currentRack].Levels[CurrentLevel].StackedBoxes.Count == Racks[currentRack].Levels[CurrentLevel].MaxCapacity)
                    {
                        CurrentLevel++;
                        if (CurrentLevel == Racks[currentRack].LevelNumber)
                        {
                            //Console.WriteLine($"Info1:{sku},{putDownNumber}");
                            //Console.WriteLine($"===================");
                            StorageInformations[sku].Add(new StorageInformation(sku, putDownNumber, timeIn, Racks[currentRack]));// record for pick-up
                            currentRack++;
                            CurrentLevel = 0;
                            putDownNumber = 0;
                        }
                        putDownNumber++;
                        //Console.WriteLine($"box:{box.SKU}");
                        Racks[currentRack].Levels[CurrentLevel].StackedBoxes.Add(box);
                    }
                }

                //Console.WriteLine($"Info2:{sku},{putDownNumber},{quantity}");
                //Console.WriteLine($"===================");
                StorageInformations[sku].Add(new StorageInformation(sku, putDownNumber, timeIn, Racks[currentRack]));// record for pick-up
            }
        }
        #endregion

        public ShelfArea(int areaNumber, int rackNumber, int seed = 1) : base(seed)
        {
            double xAxis = 0;
            string sku = "";
            for (int x = 0; x < areaNumber; x++)
            {
                xAxis += (x%2 != 0 ? 4:3);
                double yAxis = -7.5;
                for (int y = 0; y < rackNumber; y++)
                {
                    Rack rack = new Rack($"{x},{y}", new double[2] { xAxis, yAxis });
                    Racks.Add(rack);

                    yAxis += 3;
                }
            }

            StorageInformations.Add("91162940", new List<StorageInformation>());//initialize information of SKU
            StorageInformations.Add("91162116", new List<StorageInformation>());

            OnPickingQuantity.Add("91162940", 0);
            OnPickingQuantity.Add("91162116", 0);
        }

        public Tuple<Rack, double> NearestAvailableRack(double[] currentLocation, string currentSKU)
        {
            List<Rack> currentAvailableRacks = Racks.Where(rack => rack.AvailableCapacity > 0).ToList<Rack>();

            Rack nearestAvailableRack = currentAvailableRacks[0];
            double minDistance = double.MaxValue;

            foreach (Rack rack in currentAvailableRacks)
            {
                double xDistance = 0;
                double yDistance = 0;
                double currentDistance = 0;

                if (currentLocation[1] == rack.Location[1])
                {
                    currentDistance = Math.Abs(rack.Location[1] - currentLocation[1]);
                    //Console.WriteLine($"({currentLocation[0]},{currentLocation[1]}) to ({rack.Location[0]},{rack.Location[1]}):({xDistance},{yDistance})");
                }
                else
                {
                    xDistance = Math.Abs(rack.Location[0] - currentLocation[0]);
                    yDistance = Math.Min(9 - currentLocation[1] + 9 - rack.Location[1], currentLocation[1] + 9 + rack.Location[1] + 9);

                    currentDistance = xDistance + yDistance;
                    //Console.WriteLine($"({currentLocation[0]},{currentLocation[1]}) to ({rack.Location[0]},{rack.Location[1]}):({xDistance},{yDistance})");
                }

                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    nearestAvailableRack = rack;
                }
            }

            return new Tuple<Rack, double>(nearestAvailableRack, minDistance);
        }

        public double DistanceBetweenControlPoints(double[] currentLocation, double[] nextLocation)
        {
            double xDistance = 0;
            double yDistance = 0;
            double distance = 0;

            if (currentLocation[1] == nextLocation[1])
            {
                distance = Math.Abs(nextLocation[1] - currentLocation[1]);
                //Console.WriteLine($"({currentLocation[0]},{currentLocation[1]}) to ({nextLocation[0]},{nextLocation[1]}):({xDistance},{yDistance})");
            }
            else
            {
                xDistance = Math.Abs(nextLocation[0] - currentLocation[0]);
                yDistance = Math.Min(9 - currentLocation[1] + 9 - nextLocation[1], currentLocation[1] + 9 + nextLocation[1] + 9);

                distance = xDistance + yDistance;
                //Console.WriteLine($"({currentLocation[0]},{currentLocation[1]}) to ({nextLocation[0]},{nextLocation[1]}):({xDistance},{yDistance})");
            }

            return distance;
        }

        public void Test()
        {
            int numberofStackedBoxes = 0;
            foreach (Rack rack in Racks)
            {
                //Console.WriteLine($"{rack.RackName}:{rack.RestCapacity}");
                numberofStackedBoxes += rack.BusyCapacity;
            }

            Console.WriteLine("===========================================");
            Console.WriteLine("Shelf area test:");
            Console.WriteLine($"ArrivalBoxes:{ArrivalBoxesNumber}");
            Console.WriteLine($"PutDownBoxesNumber:{PutDownBoxesNumber}");
            Console.WriteLine($"BusyCapacity:{numberofStackedBoxes}");
            Console.WriteLine($"PickedBoxes:{PickedBoxesNumber}");
            Console.WriteLine($"Available Capacity:{AvailableCapacity}");
        }
    }

    class Rack
    {
        public string RackName;
        public string SKU { get; set; }
        public int MaxCapacity { get { return Levels.Sum(level=> level.MaxCapacity); } }
        public int RestCapacity { get { return Levels.Sum(level => level.RestCapacity); } }
		public int BookedCapacity { get { return Levels.Sum(level => level.BookedCapacity);} }
        public int BusyCapacity { get { return MaxCapacity - AvailableCapacity; } }
        public int AvailableCapacity { get { return Levels.Sum(level => level.AvailableCapacity); } }
        public double[] Location = new double[2];
        public int LevelNumber = 4;
        public List<Level> Levels = new List<Level>();

        public void BookCapacity(List<Box> receivedBoxes)
        {
			int receivedBoxesNumber = receivedBoxes.Count;
            if (AvailableCapacity >= receivedBoxesNumber)
            {
				foreach (Level level in Levels)
				{
                    int addingBoxesNumber = Math.Min(level.RestCapacity, receivedBoxesNumber);
                    level.BookedCapacity += addingBoxesNumber;
                    receivedBoxesNumber -= addingBoxesNumber;

                    if (receivedBoxesNumber == 0)
                        break;
                }
			}
		}

        public void ReceiveBoxes(List<Box> receivedBoxes)
        {
            int receivedBoxesNumber = receivedBoxes.Count;
            if (RestCapacity >= receivedBoxesNumber)
            {
                foreach (Level level in Levels)
                {
                    int addingBoxesNumber = Math.Min(level.RestCapacity, receivedBoxesNumber);
                    level.StackedBoxes = receivedBoxes.GetRange(0, addingBoxesNumber).Concat(level.StackedBoxes).ToList<Box>();

                    receivedBoxes.RemoveRange(0, addingBoxesNumber);
                    receivedBoxesNumber = receivedBoxes.Count;
                    level.BookedCapacity -= addingBoxesNumber;

                    if (receivedBoxesNumber == 0)
                        break;
                }
            }
        }

        public List<Box> PickupBoxes(string sku, int pickingQuantity)
        {
            int remainedpickingQuantity = pickingQuantity;
            List<Box> loadedboxes = new List<Box>();

            for (int l = 0; l < Levels.Count; l++)
            {
                List<Box> boxeswithSameSKU = Levels[l].StackedBoxes.Where(box => box.SKU == sku).ToList<Box>();
                if (boxeswithSameSKU.Count >= 1)
                {
                    int loadingQuantity = Math.Min(boxeswithSameSKU.Count, remainedpickingQuantity);
                    List<Box> _loadedBoxes = boxeswithSameSKU.GetRange(0, loadingQuantity);

                    loadedboxes.AddRange(_loadedBoxes);
                    Levels[l].StackedBoxes.RemoveAll(box=> _loadedBoxes.Contains(box));
                    remainedpickingQuantity -= loadingQuantity;

                    if (remainedpickingQuantity == 0)
                        break;
                }
            }

            //Console.WriteLine($"{pickingQuantity},{loadedboxes.Count},{sum}");
            return loadedboxes;
        }

        public Rack(string rackName,double[] location)
        {
            for (int l = 0; l < LevelNumber; l++){Levels.Add(new Level());}

            RackName = rackName;
            Location = location;
        }
    }

    class Level
    {
        public int BookedCapacity = 0;
        public int RestCapacity { get { return MaxCapacity - StackedBoxes.Count; } }
        public int AvailableCapacity { get { return RestCapacity - BookedCapacity; } }
        public int MaxCapacity = 32;
        public List<Box> StackedBoxes = new List<Box>();
    }

    class StorageInformation
    {
        public string SKU { get; set;}
        public int StackedProductionQuantity { get; set; }
        public DateTime StartStoringTime { get; set; }
        public Rack StoringRack { get; set; }

        public void PickUpSomeProduction(int pickUpNumber)
        {
            StackedProductionQuantity -= pickUpNumber;
        }

        public StorageInformation(string sku, int stackedProductionQuantity, DateTime startStoringTime, Rack storingRack)
        {
            SKU = sku;
            StackedProductionQuantity = stackedProductionQuantity;
            StartStoringTime = startStoringTime;
            StoringRack = storingRack;
        }
    }
}
