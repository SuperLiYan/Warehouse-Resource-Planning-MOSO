using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Sim_opti.Tool
{
    internal class Generate_Simulation_Data
    {
        static Random random = new Random((int)DateTime.Now.Ticks);

        public static void GenerateInboundShipment(DateTime startTime, DateTime endTime, string path)
        {
            Dictionary<string, int> totalQuantitiesbySKU = new Dictionary<string, int>();
            totalQuantitiesbySKU.Add("91162116", 441);
            totalQuantitiesbySKU.Add("91162940", 293);

            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.WriteLine($"Shipment Number,Batch Number,Product ID/ SKU,Quantity(box),Time In");

                for (TimeSpan days = TimeSpan.FromDays(0); days < (endTime - startTime); days += TimeSpan.FromDays(1))
                {
                    DateTime today = startTime + days;

                    int rankofShipment = 0;
                    int numberofShipment = random.Next(1, 3);

                    rankofShipment++;

                    string chooseSKU = "";
                    for (int ship = 0; ship < numberofShipment; ship++)
                    {
                        int hh = random.Next(6, 14);
                        int mm = random.Next(0, 60);
                        int ss = random.Next(0, 60);

                        if (random.NextDouble() < 441.0 / (441.0 + 293.0))
                            chooseSKU = "91162116";
                        else
                            chooseSKU = "91162940";
                        
                        int rankofBatch = 0;
                        int numberofBatch = random.Next(15,40+1);
                        for (int batch = 0; batch < numberofBatch; batch++)
                        {
                            rankofBatch++;
                            if (chooseSKU == "91162116")
                                sw.WriteLine($"SN{today.Day.ToString("00")}{today.Month.ToString("00")}22-{rankofShipment}," +
                                    $"BN1/{today.Day.ToString("00")}{today.Month.ToString("00")}/{rankofBatch.ToString("000")}," +
                                    $"91162116,12,{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}");
                            else if(chooseSKU == "91162940")
                                sw.WriteLine($"SN{today.Day.ToString("00")}{today.Month.ToString("00")}22-{rankofShipment}," +
                                    $"BN2/{today.Day.ToString("00")}{today.Month.ToString("00")}/{rankofBatch.ToString("000")}," +
                                    $"91162940,6,{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}");
                        }
                    }

                    numberofShipment = random.Next(1, 3);

                    rankofShipment++;

                    chooseSKU = "";
                    for (int ship = 0; ship < numberofShipment; ship++)
                    {
                        int hh = random.Next(14, 19 + 1);
                        int mm = random.Next(0, 60);
                        int ss = random.Next(0, 60);

                        if (random.NextDouble() < 441.0 / (441.0 + 293.0))
                            chooseSKU = "91162116";
                        else
                            chooseSKU = "91162940";

                        int rankofBatch = 0;
                        int numberofBatch = random.Next(15, 40 + 1);
                        for (int batch = 0; batch < numberofBatch; batch++)
                        {
                            rankofBatch++;
                            if (chooseSKU == "91162116")
                                sw.WriteLine($"SN{today.Day.ToString("00")}{today.Month.ToString("00")}22-{rankofShipment}," +
                                    $"BN1/{today.Day.ToString("00")}{today.Month.ToString("00")}/{rankofBatch.ToString("000")}," +
                                    $"91162116,12,{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}");
                            else if (chooseSKU == "91162940")
                                sw.WriteLine($"SN{today.Day.ToString("00")}{today.Month.ToString("00")}22-{rankofShipment}," +
                                    $"BN2/{today.Day.ToString("00")}{today.Month.ToString("00")}/{rankofBatch.ToString("000")}," +
                                    $"91162940,6,{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}");
                        }
                    }

                    numberofShipment = random.Next(0, 2);

                    rankofShipment++;

                    chooseSKU = "";
                    for (int ship = 0; ship < numberofShipment; ship++)
                    {
                        int hh = random.Next(19, 21 + 1);
                        int mm = random.Next(0, 60);
                        int ss = random.Next(0, 60);

                        if (random.NextDouble() < 441.0 / (441.0 + 293.0))
                            chooseSKU = "91162116";
                        else
                            chooseSKU = "91162940";

                        int rankofBatch = 0;
                        int numberofBatch = random.Next(15, 40 + 1);
                        for (int batch = 0; batch < numberofBatch; batch++)
                        {
                            rankofBatch++;
                            if (chooseSKU == "91162116")
                                sw.WriteLine($"SN{today.Day.ToString("00")}{today.Month.ToString("00")}22-{rankofShipment}," +
                                    $"BN1/{today.Day.ToString("00")}{today.Month.ToString("00")}/{rankofBatch.ToString("000")}," +
                                    $"91162116,12,{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}");
                            else if (chooseSKU == "91162940")
                                sw.WriteLine($"SN{today.Day.ToString("00")}{today.Month.ToString("00")}22-{rankofShipment}," +
                                    $"BN2/{today.Day.ToString("00")}{today.Month.ToString("00")}/{rankofBatch.ToString("000")}," +
                                    $"91162940,6,{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}");
                        }
                    }
                }

                sw.Close();
            }
        }

        public static void GenerateOutboundSalesOrder(DateTime startTime, DateTime endTime, string path)
        {
            Dictionary<string, int> totalQuantitiesbySKU = new Dictionary<string, int>();
            totalQuantitiesbySKU.Add("91162116", 77);
            totalQuantitiesbySKU.Add("91162940", 113);

            Dictionary<string, List<Tuple<int[], int, double>>> quantitiesbySKU = new Dictionary<string, List<Tuple<int[], int, double>>>();
            quantitiesbySKU.Add("91162116", new List<Tuple<int[], int, double>>() { 
                new Tuple<int[], int, double>(new int[2] {4,10}, 26, 26.0/77.0),
                new Tuple<int[], int, double>(new int[2] {11,18}, 33, 59.0/77.0),
                new Tuple<int[], int, double>(new int[2] {19,24}, 7, 66.0/77.0),
                new Tuple<int[], int, double>(new int[2] {25,31}, 6, 72.0/77.0),
                new Tuple<int[], int, double>(new int[2] {32,38}, 5, 1)
            });

            quantitiesbySKU.Add("91162940", new List<Tuple<int[], int, double>>() {
                new Tuple<int[], int, double>(new int[2] {1,6}, 23, 23.0/113.0),
                new Tuple<int[], int, double>(new int[2] {7,10}, 36, 59.0/113.0),
                new Tuple<int[], int, double>(new int[2] {11,15}, 20, 79.0/113.0),
                new Tuple<int[], int, double>(new int[2] {16,19}, 20, 99.0/113.0),
                new Tuple<int[], int, double>(new int[2] {20,24}, 8, 107.0/113.0),
                new Tuple<int[], int, double>(new int[2] {25,28}, 6, 1)
            });

            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.WriteLine($"Order No.,Order Date,Quantity Sold (SU),Transaction revenue,Location,Item code");

                for (TimeSpan days = TimeSpan.FromDays(0); days < (endTime - startTime); days += TimeSpan.FromDays(1))
                {
                    DateTime today = startTime + days;

                    int rankofSalesOrder = 0;
                    int numberofSalesOrder = random.Next(10,18);

                    for (int orders = 0; orders < numberofSalesOrder; orders++)
                    {
                        rankofSalesOrder++;
                        string chooseSKU = "";
                        if (random.NextDouble() < 77.0 / (77.0 + 113.0))
                            chooseSKU = "91162116";
                        else
                            chooseSKU = "91162940";

                        double chooseBoxesQuantity = random.NextDouble();
                        int boxesquantity = 0;
                        double startPoint = 0;

                        for (int i = 0; i < quantitiesbySKU[chooseSKU].Count; i++)
                        {
                            int hh = random.Next(6, 14+1);
                            int mm = random.Next(0, 60);
                            int ss = random.Next(0, 60);
                            if (startPoint <= chooseBoxesQuantity && chooseBoxesQuantity < quantitiesbySKU[chooseSKU][i].Item3)
                            {
                                boxesquantity = random.Next(quantitiesbySKU[chooseSKU][i].Item1[0], quantitiesbySKU[chooseSKU][i].Item1[1]+1);

                                sw.WriteLine($"SO2208{today.Day.ToString("00")}-{rankofSalesOrder.ToString("000")}," +
                                    $"{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}," +
                                    $"{boxesquantity},{10},{"5002933413"},{chooseSKU}");
                                break;
                            }
                            else
                            {
                                startPoint = quantitiesbySKU[chooseSKU][i].Item3;
                            }
                        }
                    }

                    numberofSalesOrder = random.Next(10, 18);
                    for (int orders = 0; orders < numberofSalesOrder; orders++)
                    {
                        rankofSalesOrder++;
                        string chooseSKU = "";
                        if (random.NextDouble() < 77.0 / (77.0 + 113.0))
                            chooseSKU = "91162116";
                        else
                            chooseSKU = "91162940";

                        double chooseBoxesQuantity = random.NextDouble();
                        int boxesquantity = 0;
                        double startPoint = 0;

                        for (int i = 0; i < quantitiesbySKU[chooseSKU].Count; i++)
                        {
                            int hh = random.Next(14, 19+1);
                            int mm = random.Next(0, 60);
                            int ss = random.Next(0, 60);
                            if (startPoint <= chooseBoxesQuantity && chooseBoxesQuantity < quantitiesbySKU[chooseSKU][i].Item3)
                            {
                                boxesquantity = random.Next(quantitiesbySKU[chooseSKU][i].Item1[0], quantitiesbySKU[chooseSKU][i].Item1[1] + 1);

                                sw.WriteLine($"SO2208{today.Day.ToString("00")}-{rankofSalesOrder.ToString("000")}," +
                                    $"{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}," +
                                    $"{boxesquantity},{10},{"5002933413"},{chooseSKU}");
                                break;
                            }
                            else
                            {
                                startPoint = quantitiesbySKU[chooseSKU][i].Item3;
                            }
                        }
                    }

                    numberofSalesOrder = random.Next(6, 10);
                    for (int orders = 0; orders < numberofSalesOrder; orders++)
                    {
                        rankofSalesOrder++;
                        string chooseSKU = "";
                        if (random.NextDouble() < 77.0 / (77.0 + 113.0))
                            chooseSKU = "91162116";
                        else
                            chooseSKU = "91162940";

                        double chooseBoxesQuantity = random.NextDouble();
                        int boxesquantity = 0;
                        double startPoint = 0;

                        for (int i = 0; i < quantitiesbySKU[chooseSKU].Count; i++)
                        {
                            int hh = random.Next(20, 21 + 1);
                            int mm = random.Next(0, 60);
                            int ss = random.Next(0, 60);
                            if (startPoint <= chooseBoxesQuantity && chooseBoxesQuantity < quantitiesbySKU[chooseSKU][i].Item3)
                            {
                                boxesquantity = random.Next(quantitiesbySKU[chooseSKU][i].Item1[0], quantitiesbySKU[chooseSKU][i].Item1[1] + 1);

                                sw.WriteLine($"SO2208{today.Day.ToString("00")}-{rankofSalesOrder.ToString("000")}," +
                                    $"{today.Year}/{today.Month}/{today.Day} {hh}:{mm.ToString("00")}:{ss.ToString("00")}," +
                                    $"{boxesquantity},{10},{"5002933413"},{chooseSKU}");
                                break;
                            }
                            else
                            {
                                startPoint = quantitiesbySKU[chooseSKU][i].Item3;
                            }
                        }
                    }
                }

                sw.Close();
            }
        }
    }
}
