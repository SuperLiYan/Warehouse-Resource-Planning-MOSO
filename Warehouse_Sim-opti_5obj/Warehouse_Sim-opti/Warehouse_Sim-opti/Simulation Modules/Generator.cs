using Warehouse_Sim_opti.Tool;
using O2DESNet;
using Warehouse_Sim_opti.Simulation_Entities;

namespace Warehouse_Sim_opti.Simulation_Modules
{
    class Input
    {
        /// <value>
        /// Simulation start time.
        /// </value>
        public static DateTime StartTime { get; set; }
        /// <value>
        /// Simulation stopping time.
        /// </value>
        public static DateTime EndTime { get; set; }
        /// <value>
        /// Inbound shipment infromation.
        /// </value>
        public static List<List<string>> InboundShipment = new List<List<string>>();
        /// <value>
        /// Sales order information.
        /// </value>
        public static List<List<string>> SalesOrder = new List<List<string>>();
        /// <value>
        /// Inventory information.
        /// </value>
        public static List<List<string>> Inventory = new List<List<string>>();
        /// <value>
        /// List of box size in warehouse.
        /// </value>
        public static Dictionary<string, ProducationInformation> ProductionList = new Dictionary<string, ProducationInformation>();
        /// <value>
        /// Get configuration of warehouse operation form tables.
        /// </value>
        public static void PropertiesSetting(DateTime starttime, DateTime endtime)
        {
            StartTime = starttime; EndTime = endtime;

            InboundShipment = Reader.CreatString(Reader.ReadCSV("..\\Input Files-plus\\Inbound Shipment(1).csv")); InboundShipment.RemoveAt(0);
            SalesOrder = Reader.CreatString(Reader.ReadCSV("..\\Input Files-plus\\Sales Order.csv")); SalesOrder.RemoveAt(0);
            Inventory = Reader.CreatString(Reader.ReadCSV("..\\Input Files\\Inventory.csv")); Inventory.RemoveAt(0);

            ProductionList.Add("91162940", new ProducationInformation("91162940", 510,410,410,5));
            ProductionList.Add("91162116", new ProducationInformation("91162116", 510, 410, 410, 5));
        }
    }

    class ProducationInformation
    {
        public string SKU { get; set; }
        public double Length { get; set; }
        public double Hight { get; set; }
        public double Width { get; set; }
        public double Weight { get; set; }

        public ProducationInformation(string sku, double length, double hight, double width, double weight) 
        {
            SKU = sku;
            Length = length; 
            Hight = hight; 
            Width = width; 
            Weight = weight;
        }
    }

    class Shipment 
    {
        #region Statics
        public string ShipmentNumber { get; set; }
        public List<Batch> ArrivalBatches { get; set; }
        public DateTime TimeIn { get; set; }
        #endregion
        public Shipment(string shipmentNumber, List<Batch> batches,DateTime timeIn)
        {
            ShipmentNumber = shipmentNumber;
            ArrivalBatches = batches;
            TimeIn= timeIn;
        }
    }
    class Batch
    {
        public string BatchNumber { get; set; }
        public List<Box> ArrivalBoxes= new List<Box>();
        public int Quantity { get { return ArrivalBoxes.Count;}}
        public string ProductID { get; set; }
        public DateTime TimeIn { get; set; }
        public double Weight { get { return ArrivalBoxes.Sum(b=>b.Weight); } }

        public Batch(string batchNumber, string productId, List<Box> arrivalBoxes, DateTime timeIn)
        {
            BatchNumber = batchNumber;
            ProductID= productId;
            ArrivalBoxes = arrivalBoxes;
            TimeIn = timeIn;
        }
    }

    class SalesOrder
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public int QuantitySold { get; set; }
        public string SKU { get; set; }

        public SalesOrder(string orderNumber, DateTime orderDate, int quantitySold, string sku) 
        {
            OrderNumber = orderNumber;
            OrderDate = orderDate;
            QuantitySold = quantitySold;
            SKU = sku;
        }
    }

    internal class SimulationGenerator:Sandbox
    {
        public int BoxNumber { get; set; }
        public int OrderNumber { get; set; }
        public List<Shipment> Shipments = new List<Shipment>();
        public event Action<Shipment> ShipmentOnStart = Shipment => { };
        /// <value>
        /// Generate shipments according to historical data.
        /// </value>
        public void GenerateShipmentsByHistroy()
        {
            int batchRank = 0;
            int shipmentRank = 0;
            string shipmentNumber = Input.InboundShipment[0][0];

            List<List<Batch>> batchesofShipments = new List<List<Batch>>();
            batchesofShipments.Add(new List<Batch>());

            DateTime timeIn = StringtoFormalDatetime.StringToDatetime(Input.InboundShipment[0][4]);

            for (batchRank = 0; batchRank < Input.InboundShipment.Count; batchRank++)
            {
                List<Box> boxes = new List<Box>();
                string boxName = Input.InboundShipment[batchRank][2];//Get box name;
                //List<string> boxProperties = Input.ProductionList.Where(b => b.SKU == boxName).ToList()[0];//Get box properties;

                for (int b = 0; b < int.Parse(Input.InboundShipment[batchRank][3]); b++)// generate boxes
                {
                    BoxNumber++;
                    ProducationInformation boxProperties = Input.ProductionList[boxName];
                    Box box = new Box(boxProperties.SKU, boxProperties.Length, boxProperties.Hight, boxProperties.Width, boxProperties.Weight, timeIn);

                    boxes.Add(box);
                }
                
                Batch batch = new Batch(Input.InboundShipment[batchRank][1], Input.InboundShipment[batchRank][2], boxes, timeIn);

                if (shipmentNumber == Input.InboundShipment[batchRank][0] && batchRank != Input.InboundShipment.Count - 1)// still in the same shipment
                {
                    batchesofShipments[shipmentRank].Add(batch);
                }
                else if (shipmentNumber == Input.InboundShipment[batchRank][0] && batchRank == Input.InboundShipment.Count - 1)//complete all batches and shipments
                {
                    batchesofShipments[shipmentRank].Add(batch);
                    Shipment shipment = new Shipment(shipmentNumber, batchesofShipments[shipmentRank], timeIn);
                    Schedule(() => ShipmentOnStart.Invoke(shipment), shipment.TimeIn);
                    timeIn = StringtoFormalDatetime.StringToDatetime(Input.InboundShipment[batchRank][4]);
                    //Shipments.Add(shipment);
                }
                else// another shipment
                {
                    batchesofShipments[shipmentRank].Add(batch);
                    Shipment shipment = new Shipment(shipmentNumber, batchesofShipments[shipmentRank], timeIn);
                    Schedule(() => ShipmentOnStart.Invoke(shipment), shipment.TimeIn); 
                    timeIn = StringtoFormalDatetime.StringToDatetime(Input.InboundShipment[batchRank][4]);
                    shipmentRank++;
                    //Shipments.Add(shipment);

                    //Generate new shipment;
                    batchesofShipments.Add(new List<Batch>());
                    shipmentNumber = Input.InboundShipment[batchRank][0];
                }
            }
        }

        public List<SalesOrder> Orders = new List<SalesOrder>();
        public event Action<SalesOrder> SalesOrderOnStart = salesOrder => { };
        public void GenerateOrdersByHistroy()
        {
            int numberofSalesOrder = Input.SalesOrder.Count;

            for (int orderRank = 0; orderRank < numberofSalesOrder; orderRank++)
            {
                OrderNumber++;
                string orderNumber = Input.SalesOrder[orderRank][0];
                DateTime orderDate = StringtoFormalDatetime.StringToDatetime(Input.SalesOrder[orderRank][1]);
                int quantitySold = int.Parse(Input.SalesOrder[orderRank][2]);
                string sku = Input.SalesOrder[orderRank][5];

                SalesOrder salesOrder = new SalesOrder(orderNumber, orderDate, quantitySold, sku);
                Schedule(()=> SalesOrderOnStart.Invoke(salesOrder), salesOrder.OrderDate);
            }
        }

        public void Test()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Generator test:");
            int Counter = 0;
            foreach (Shipment shipment in Shipments)
            {
                //Console.WriteLine("============================================");
                Console.WriteLine($"{shipment.ShipmentNumber},{shipment.ArrivalBatches.Count},{shipment.TimeIn}");

                foreach (Batch batch in shipment.ArrivalBatches)
                {
                    Counter++;
                    Console.WriteLine($"{batch.BatchNumber},{batch.ProductID},{batch.TimeIn}");
                    Console.WriteLine("********************************************");
                    foreach (Box box in batch.ArrivalBoxes)
                    {
                        Console.WriteLine($"{box.SKU},{box.Length}*{box.Hight}*{box.Width},{box.Weight}");
                    }
                }
            }

            Console.WriteLine($"TotalBatchNumber:{Input.InboundShipment.Count}");
            Console.WriteLine($"TotalBatchNumber:{Counter}");
            Console.WriteLine($"TotalBoxNumber:{BoxNumber}");
            Console.WriteLine($"TotalOrderNumber:{OrderNumber}");
        }
    }
}