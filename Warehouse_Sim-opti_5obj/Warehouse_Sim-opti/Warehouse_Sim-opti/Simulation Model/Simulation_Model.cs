using O2DESNet;
using O2DESNet.Standard;
using Warehouse_Sim_opti.Simulation_Modules;

namespace Warehouse_Sim_opti.Simulation_Model
{
    class Simulation_System:Sandbox
    {
        static Random random = new Random((int)DateTime.Now.Ticks);

        SimulationGenerator generator = new SimulationGenerator(); 
        Gate gate = new Gate(3, 3, random.Next()); 
        InboundBufferArea inboundBufferArea = new InboundBufferArea(10, 3, 6, random.Next()); 
        ShelfArea shelfArea = new ShelfArea(14, 6); 
        OutboundBufferArea outboundBufferArea = new OutboundBufferArea(3, 1, 4, random.Next()); 
        Exit exit = new Exit();

        public void ManpowerShiftSchedule(DateTime startTime, DateTime endTime, int[][] schedule)
        {
            int days = (int)(endTime - startTime).TotalDays;

            for (int day = 0; day < days; day++)
            {
                DateTime restartTime = Input.StartTime + TimeSpan.FromDays(day) + TimeSpan.FromHours(6);

                int staffsAtGate = schedule[day][0]; 
                int staffsAtInboundofForklifts = schedule[day][1];
                int staffsAtInboundofUnpacking = schedule[day][2];
                int staffsAtOutboundofForklifts = schedule[day][3];
                int staffsAtOutboundofPacking = schedule[day][4];

                int fleetsAtGate = schedule[day][5];
                int fleetsAtInbound = schedule[day][6];
                int fleetsAtOutbound = schedule[day][7];

                Schedule(()=>ManpowerShift(staffsAtGate, staffsAtInboundofForklifts, staffsAtInboundofUnpacking, 
                    staffsAtOutboundofForklifts, staffsAtInboundofUnpacking, fleetsAtGate, fleetsAtInbound, fleetsAtOutbound), restartTime);

                Schedule(gate.TryStartLoadingBatchesorRestart, restartTime + TimeSpan.FromHours(6));
                Schedule(inboundBufferArea.GetNextTaskorRestart, restartTime);
                Schedule(outboundBufferArea.StartNextTaskorRestart, restartTime);

                restartTime = Input.StartTime + TimeSpan.FromDays(day) + TimeSpan.FromHours(14.5);

                staffsAtGate = schedule[day][8]; 
                staffsAtInboundofForklifts = schedule[day][9];
                staffsAtInboundofUnpacking = schedule[day][10];
                staffsAtOutboundofForklifts = schedule[day][11];
                staffsAtOutboundofPacking = schedule[day][12];

                fleetsAtGate = schedule[day][13];
                fleetsAtInbound = schedule[day][14];
                fleetsAtOutbound = schedule[day][15];

                Schedule(() => ManpowerShift(staffsAtGate, staffsAtInboundofForklifts, staffsAtInboundofUnpacking,
                    staffsAtOutboundofForklifts, staffsAtInboundofUnpacking, fleetsAtGate, fleetsAtInbound, fleetsAtOutbound), restartTime);

                Schedule(gate.TryStartLoadingBatchesorRestart, restartTime);
                Schedule(inboundBufferArea.GetNextTaskorRestart, restartTime);
                Schedule(outboundBufferArea.StartNextTaskorRestart, restartTime);

                restartTime = Input.StartTime + TimeSpan.FromDays(day) + TimeSpan.FromHours(19.0);
                Schedule(gate.TryStartLoadingBatchesorRestart, restartTime);
                Schedule(inboundBufferArea.GetNextTaskorRestart, restartTime);
                Schedule(outboundBufferArea.StartNextTaskorRestart, restartTime);
            }
        }

        public void ManpowerShift(int staffsAtGate, int staffsAtInboundofForklifts, int staffsAtInboundofUnpacking, int staffsAtOutboundofForklifts,
            int staffsAtOutboundofPacking, int fleetsAtGate, int fleetsAtInbound, int fleetsAtOutbound)
        {
            //Console.WriteLine($"{staffsAtGate},{staffsAtInbound},{staffsAtOutbound}");
            gate.InitializeandReallocate(staffsAtGate, 0,0,fleetsAtGate);
            inboundBufferArea.InitializeandReallocate(staffsAtInboundofForklifts, staffsAtInboundofUnpacking, 0, fleetsAtInbound);
            outboundBufferArea.InitializeandReallocate(staffsAtOutboundofForklifts, 0, staffsAtOutboundofPacking, fleetsAtOutbound);
        }

        public void Restart()
        {
            for (int days = 0; days <= (Input.EndTime - Input.StartTime).TotalDays; days++)
            {
                DateTime restartTime = Input.StartTime + TimeSpan.FromDays(days) + TimeSpan.FromHours(6);
                Schedule(ManpowerShift, restartTime);
                Schedule(gate.TryStartLoadingBatchesorRestart, restartTime+TimeSpan.FromHours(6));
                Schedule(inboundBufferArea.GetNextTaskorRestart, restartTime);
                Schedule(outboundBufferArea.StartNextTaskorRestart, restartTime);

                restartTime = Input.StartTime + TimeSpan.FromDays(days) + TimeSpan.FromHours(14.5);
                Schedule(ManpowerShift, restartTime);
                Schedule(gate.TryStartLoadingBatchesorRestart, restartTime);
                Schedule(inboundBufferArea.GetNextTaskorRestart, restartTime);
                Schedule(outboundBufferArea.StartNextTaskorRestart, restartTime);

                restartTime = Input.StartTime + TimeSpan.FromDays(days) + TimeSpan.FromHours(19.0);
                Schedule(gate.TryStartLoadingBatchesorRestart, restartTime);
                Schedule(inboundBufferArea.GetNextTaskorRestart, restartTime);
                Schedule(outboundBufferArea.StartNextTaskorRestart, restartTime);
            }
        }

        public void ManpowerShift()
        {
            //gate.InitializeandReallocate(3, 3);
            //inboundBufferArea.InitializeandReallocate(15, 8);
            //outboundBufferArea.InitializeandReallocate(2, 2);

            gate.InitializeandReallocate(3,0,0, 3);
            inboundBufferArea.InitializeandReallocate(7,6,0, 6);
            outboundBufferArea.InitializeandReallocate(3,0,1, 4);
        }

        public double[] StackingTimes = new double[3];

        public double GetKPI()
        {
            double totalAverageStackingTime = 0;
            double averageStackingTimeAtGate = gate.AverageStackingTime().TotalMinutes;
            double averageStackingTimeAtInboundBufferArea = inboundBufferArea.AverageStackingTime().TotalMinutes;
            double averageStackingTimeAtOutboundBufferArea = outboundBufferArea.AverageStackingTime().TotalMinutes;

            StackingTimes[0] = averageStackingTimeAtGate;
            StackingTimes[1] = averageStackingTimeAtInboundBufferArea;
            StackingTimes[2] = averageStackingTimeAtOutboundBufferArea;

            totalAverageStackingTime += averageStackingTimeAtGate + averageStackingTimeAtInboundBufferArea + averageStackingTimeAtOutboundBufferArea;

            //Console.WriteLine("Averagae Stacking Time:");
            //Console.WriteLine($"Gate: {averageStackingTimeAtGate.ToString("0.000")} min|| " +
            //    $"Inbound buffer area: {averageStackingTimeAtInboundBufferArea.ToString("0.000")} min||" +
            //    $"Outbound buffer area: {averageStackingTimeAtOutboundBufferArea.ToString("0.000")} min||" +
            //    $"Total Average Stacking Time:{totalAverageStackingTime.ToString("0.000")} min");
            //Console.WriteLine($"Simulation:{gate.ManpowerPool.Count + inboundBufferArea.ManpowerPool.Count + outboundBufferArea.ManpowerPool.Count}," +
            //    $"{gate.ForkliftFleet.Count + inboundBufferArea.ForkliftFleet.Count + outboundBufferArea.ForkliftFleet.Count}");
            return totalAverageStackingTime;
        }

        int MaxManpowerofForklifts = 0;
        int MaxManpowerofUnpacking = 0;
        int MaxManpowerofPacking = 0;

        int MaxForklifts = 0;

        public double[] GetKPIs()
        {
            double stackingTime = GetKPI();
            //Console.WriteLine($"Simulation:{MaxManpower},{MaxForklifts}");

            return new double[] { stackingTime, MaxManpowerofForklifts, MaxManpowerofUnpacking, MaxManpowerofPacking, MaxForklifts};
        }

        public Simulation_System()
        {
            AddChild(generator);
            AddChild(gate);
            AddChild(inboundBufferArea);
            AddChild(shelfArea);
            AddChild(outboundBufferArea);
            AddChild(exit);

            generator.ShipmentOnStart += gate.ShipmentArrival;
            generator.SalesOrderOnStart += outboundBufferArea.GetSalesOrder;
            gate.StaffandForkliftOnTraveling += inboundBufferArea.StaffandForkliftArrivalFromGate;
            gate.MessageInboundBufferArea += inboundBufferArea.GetMessageFromGate;
            inboundBufferArea.StaffandForkliftOnReturnToGate += gate.StaffandForkliftArrival;
            inboundBufferArea.OnTransferringToShelfArea += shelfArea.StaffandForkliftArrivalfromInboundbufferArea;
            inboundBufferArea.RequestIfShelfAreahasFreeCapacity += shelfArea.GetRequestFormInboundBufferArea;
            shelfArea.FeedbackIfShelfAreahasFreeCapacity += inboundBufferArea.GetMessageFromShelfArea;
            shelfArea.StaffandForkliftOnReturningToInboundBufferArea += inboundBufferArea.StaffandForkliftArrival;
            shelfArea.StaffandForkliftOnReturningToOutboundBufferArea += outboundBufferArea.StaffandForkliftReturnFromShelfArea;
            shelfArea.FeedbackIfShelfAreahasEnoughStorage += outboundBufferArea.ReceiveIfShelfAreahasEnoughStorage;

            outboundBufferArea.OnTraveltoShelfArea += shelfArea.StaffandForkliftArrivalfromOutboundbufferArea;
            outboundBufferArea.OnTraveltoExit += exit.ReceiveBathces;
            outboundBufferArea.RequestIfShelfAreahasEnoughStorage += shelfArea.ReceiveMessagefromOutboundbufferArea;

            exit.OnreturntoOutboundBufferArea += outboundBufferArea.StaffandForkliftReturnFromExit;
            //Running
            shelfArea.InitializeInventory();
            generator.GenerateShipmentsByHistroy(); //0.012s
            generator.GenerateOrdersByHistroy();
            Restart();

            WarmUp(Input.StartTime);
            Run(Input.EndTime); //0.007s

            //generator.Test();
            //gate.Test();
            //inboundBufferArea.Test();
            //shelfArea.Test();
            //outboundBufferArea.Test();
            //exit.Test();

            GetKPIs();
        }

        public Simulation_System(int[][] manpowerSchedule)
        {
            AddChild(generator);
            AddChild(gate);
            AddChild(inboundBufferArea);
            AddChild(shelfArea);
            AddChild(outboundBufferArea);
            AddChild(exit);

            generator.ShipmentOnStart += gate.ShipmentArrival;
            generator.SalesOrderOnStart += outboundBufferArea.GetSalesOrder;
            gate.StaffandForkliftOnTraveling += inboundBufferArea.StaffandForkliftArrivalFromGate;
            gate.MessageInboundBufferArea += inboundBufferArea.GetMessageFromGate;
            inboundBufferArea.StaffandForkliftOnReturnToGate += gate.StaffandForkliftArrival;
            inboundBufferArea.OnTransferringToShelfArea += shelfArea.StaffandForkliftArrivalfromInboundbufferArea;
            inboundBufferArea.RequestIfShelfAreahasFreeCapacity += shelfArea.GetRequestFormInboundBufferArea;
            shelfArea.FeedbackIfShelfAreahasFreeCapacity += inboundBufferArea.GetMessageFromShelfArea;
            shelfArea.StaffandForkliftOnReturningToInboundBufferArea += inboundBufferArea.StaffandForkliftArrival;
            shelfArea.StaffandForkliftOnReturningToOutboundBufferArea += outboundBufferArea.StaffandForkliftReturnFromShelfArea;
            shelfArea.FeedbackIfShelfAreahasEnoughStorage += outboundBufferArea.ReceiveIfShelfAreahasEnoughStorage;

            outboundBufferArea.OnTraveltoShelfArea += shelfArea.StaffandForkliftArrivalfromOutboundbufferArea;
            outboundBufferArea.OnTraveltoExit += exit.ReceiveBathces;
            outboundBufferArea.RequestIfShelfAreahasEnoughStorage += shelfArea.ReceiveMessagefromOutboundbufferArea;

            exit.OnreturntoOutboundBufferArea += outboundBufferArea.StaffandForkliftReturnFromExit;
            //Running
            shelfArea.InitializeInventory();
            generator.GenerateShipmentsByHistroy(); //0.012s
            generator.GenerateOrdersByHistroy();
            ManpowerShiftSchedule(Input.StartTime, Input.EndTime, manpowerSchedule);

            WarmUp(Input.StartTime);
            Run(Input.EndTime); //0.007s

            //generator.Test();
            //gate.Test();
            //inboundBufferArea.Test();
            //shelfArea.Test();
            //outboundBufferArea.Test();
            //exit.Test();
        }

        public Simulation_System(int maxManpowerofForklifts, int maxManpowerofUnpacking, int maxManpowerofPacking, 
            int maxForklifts, int[][] manpowerSchedule)
        {
            MaxManpowerofForklifts = maxManpowerofForklifts;
            MaxManpowerofUnpacking = maxManpowerofUnpacking;
            MaxManpowerofPacking = maxManpowerofPacking;

            MaxForklifts = maxForklifts;

            AddChild(generator);
            AddChild(gate);
            AddChild(inboundBufferArea);
            AddChild(shelfArea);
            AddChild(outboundBufferArea);
            AddChild(exit);

            generator.ShipmentOnStart += gate.ShipmentArrival;
            generator.SalesOrderOnStart += outboundBufferArea.GetSalesOrder;
            gate.StaffandForkliftOnTraveling += inboundBufferArea.StaffandForkliftArrivalFromGate;
            gate.MessageInboundBufferArea += inboundBufferArea.GetMessageFromGate;
            inboundBufferArea.StaffandForkliftOnReturnToGate += gate.StaffandForkliftArrival;
            inboundBufferArea.OnTransferringToShelfArea += shelfArea.StaffandForkliftArrivalfromInboundbufferArea;
            inboundBufferArea.RequestIfShelfAreahasFreeCapacity += shelfArea.GetRequestFormInboundBufferArea;
            shelfArea.FeedbackIfShelfAreahasFreeCapacity += inboundBufferArea.GetMessageFromShelfArea;
            shelfArea.StaffandForkliftOnReturningToInboundBufferArea += inboundBufferArea.StaffandForkliftArrival;
            shelfArea.StaffandForkliftOnReturningToOutboundBufferArea += outboundBufferArea.StaffandForkliftReturnFromShelfArea;
            shelfArea.FeedbackIfShelfAreahasEnoughStorage += outboundBufferArea.ReceiveIfShelfAreahasEnoughStorage;

            outboundBufferArea.OnTraveltoShelfArea += shelfArea.StaffandForkliftArrivalfromOutboundbufferArea;
            outboundBufferArea.OnTraveltoExit += exit.ReceiveBathces;
            outboundBufferArea.RequestIfShelfAreahasEnoughStorage += shelfArea.ReceiveMessagefromOutboundbufferArea;

            exit.OnreturntoOutboundBufferArea += outboundBufferArea.StaffandForkliftReturnFromExit;
            //Running
            shelfArea.InitializeInventory();
            generator.GenerateShipmentsByHistroy(); //0.012s
            generator.GenerateOrdersByHistroy();
            ManpowerShiftSchedule(Input.StartTime, Input.EndTime, manpowerSchedule);

            WarmUp(Input.StartTime);
            Run(Input.EndTime); //0.007s

            //generator.Test();
            //gate.Test();
            //inboundBufferArea.Test();
            //shelfArea.Test();
            //outboundBufferArea.Test();
            //exit.Test();
        }

        public static bool TimeCondition(double currentHour)
        {
            if ((6.0 <= currentHour && currentHour < 13) || (14.5 <= currentHour && currentHour < 18.5)
                || (19.0 <= currentHour && currentHour < 22.0))
                return true;
            else
                return false;
        }
    }
}