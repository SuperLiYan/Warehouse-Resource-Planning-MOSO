//#define Algorithm_Test
//#define Single_objective
//#define ZDT1
//#define ZDT2
//#define ZDT3
//#define ZDT4
//#define Double_objective
//#define Tribe_objective
#define Four_objective

using Warehouse_Sim_opti.Simulation_Entities;
using Warehouse_Sim_opti.Simulation_Modules;
using Warehouse_Sim_opti.Simulation_Model;
using Warehouse_Sim_opti.Tool;
using Warehouse_Sim_opti.Optimization_Algorithm;

namespace Warehouse_Sim_opti
{
    public class Program
    {
        static void Main(string[] args)
        {
#if Algorithm_Test
            #region Algorithm Test
            int numberofvariables = 300;
            double[] uppers = new double[numberofvariables];
            double[] lowers = new double[numberofvariables];
            string Order = "Square_testing";//note that, the larger the decision space, the larger the value of a.
            if (Order == "Square_testing")
            {
                for (int p = 0; p < numberofvariables; p++)
                {
                    uppers[p] = 30;
                    lowers[p] = -30;
                }

                //Paramters of MGD-SPSA-single 
                MGD_SPSA mgd_spsa = new MGD_SPSA(10000, 0.16, 0.5, 100, 0.602, 0.101,
                    1, numberofvariables, uppers, lowers, "Square_testing");

                mgd_spsa.MainProgram();

                Console.WriteLine($"The actual optimal result is 0.00");
            }
            else if (Order == "ZDT1")
            {
                for (int p = 0; p < numberofvariables; p++)
                {
                    uppers[p] = 1;
                    lowers[p] = 0;
                }

                //Paramters of MGD-SPSA
                MGD_SPSA mgd_spsa = new MGD_SPSA(10000, 2000, 50, 100, 0.602, 0.101,
                    2, numberofvariables, uppers, lowers, "ZDT1");

                mgd_spsa.MainProgram();
            }
            else if (Order == "ZDT4")
            {
                lowers[0] = 0;  uppers[0] = 1;
                for (int p = 1; p < numberofvariables; p++)
                {
                    uppers[p] =  10;
                    lowers[p] = -10;
                }

                //Paramters of MGD-SPSA
                MGD_SPSA mgd_spsa = new MGD_SPSA(9000, 0.16, 0.5, 100, 0.602, 0.101,
                    2, numberofvariables, uppers, lowers, "ZDT4");

                mgd_spsa.MainProgram();
            }
            #endregion
#endif


#if ZDT1
        for (int p = 0; p < numberofvariables; p++)
        {
            uppers[p] = 1;
            lowers[p] = 0;
        }

        //Paramters of MGD-SPSA
        MGD_SPSA mgd_spsa = new MGD_SPSA(10000, new double[] { 50, 50 }, new double[] { 0.5, 0.5 },
            new double[] { 2000, 2000 }, new double[] { 0.602, 0.602 }, new double[] { 0.101, 0.101 },
            2, numberofvariables, uppers, lowers);

        mgd_spsa.MainProgram();
#endif

#if ZDT2
        for (int p = 0; p < numberofvariables; p++)
        {
            uppers[p] = 1;
            lowers[p] = 0;
        }

        //Paramters of MGD-SPSA
        MGD_SPSA mgd_spsa = new MGD_SPSA(10000, new double[] { 50, 50 }, new double[] { 0.5, 0.5 },
            new double[] { 2000, 2000 }, new double[] { 0.602, 0.602 }, new double[] { 0.101, 0.101 },
            2, numberofvariables, uppers, lowers);

        mgd_spsa.MainProgram();
#endif

#if ZDT3
        for (int p = 0; p < numberofvariables; p++)
        {
            uppers[p] = 1;
            lowers[p] = 0;
        }

        ////Paramters of MGD-SPSA
        MGD_SPSA mgd_spsa = new MGD_SPSA(3000, new double[] { 500, 500 }, new double[] { 0.5, 0.5 },
            new double[] { 5000, 5000 }, new double[] { 0.602, 0.602 }, new double[] { 0.101, 0.101 },
            2, numberofvariables, uppers, lowers);

        mgd_spsa.MainProgram();
#endif

#if ZDT4
        for (int p = 0; p < numberofvariables; p++)
        {
            uppers[p] = 1;
            lowers[p] = 0;
        }

        ////Paramters of MGD-SPSA
        MGD_SPSA mgd_spsa = new MGD_SPSA(5000, new double[] { 500, 500 }, new double[] { 0.5, 0.5 },
            new double[] { 5000, 5000 }, new double[] { 0.602, 0.602 }, new double[] { 0.101, 0.101 },
            2, numberofvariables, uppers, lowers);

        mgd_spsa.MainProgram();
#endif

#if Single_objective
            //Generate_Simulation_Data.GenerateOutboundSalesOrder(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0),
            //    "..\\Input Files-plus\\Sales Order.csv");

            //Generate_Simulation_Data.GenerateInboundShipment(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0),
            //        "..\\Input Files-plus\\Inbound Shipment.csv");

            Input.PropertiesSetting(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0));

            DateTime d1 = DateTime.Now;

            //Simulation_System warehouse = new Simulation_System();
            int numberofvariables = 372;
            double[] uppers = new double[numberofvariables];
            double[] lowers = new double[numberofvariables];
            string Order = "";//note that, the larger the decision space, the larger the value of a.

            for (int p = 0; p < numberofvariables; p++)
            {
                uppers[p] = 1;
                lowers[p] = 0.0001;
            }

    //        SPSA spsa = new SPSA(10000, 0.001, 0.15, 0.005, 0.602, 0.101,//5*E-6 = 205.04675;
    //1, numberofvariables, uppers, lowers, Order);

            SPSA spsa = new SPSA(10000, 0.001, 0.15, 0.1, 0.602, 0.101,//5*E-6 = 205.04675;
                1, numberofvariables, uppers, lowers, Order);

            spsa.MainProgram();

            DateTime d2 = DateTime.Now;
            Console.WriteLine((d2 - d1).TotalSeconds);
#endif

#if Double_objective
            //Generate_Simulation_Data.GenerateOutboundSalesOrder(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0),
            //    "..\\Input Files-plus\\Sales Order.csv");

            //Generate_Simulation_Data.GenerateInboundShipment(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0),
            //        "..\\Input Files-plus\\Inbound Shipment.csv");

            int numberofvariables = 374;//188
            double[] uppers = new double[numberofvariables];
            double[] lowers = new double[numberofvariables];

            Input.PropertiesSetting(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0));

            DateTime d1 = DateTime.Now;

            for (int p = 0; p < numberofvariables; p++)
            {
                uppers[p] = 1;
                lowers[p] = 0.01;
            }

            ////Paramters of MGD-SPSA
            ////MGD_SPSA(10000, new double[] { 50, 50 }, new double[] { 0.5, 0.5 },
            //new double[] { 5000, 5000 }, new double[] { 0.602, 0.602 }, new double[] { 0.101, 0.101 }, == 645.44115, 7100.00000

            MGD_SPSA.StaffStart = 20; MGD_SPSA.StaffEnd = MGD_SPSA.StaffStart + 60;
            MGD_SPSA.ForkliftStart = 10; MGD_SPSA.ForkliftEnd = MGD_SPSA.ForkliftStart + 30;

            MGD_SPSA mgd_spsa = new MGD_SPSA(20000, new double[] { 4.0 * Math.Pow(10, 2), 10.0 * Math.Pow(10, 0) }, new double[] { 0.30 * Math.Pow(10, 0), 0.30 * Math.Pow(10, 0) },

                new double[] { 0.5 * Math.Pow(10, 0), 3.0 * Math.Pow(10, 0) }, new double[] { 0.602, 0.602 }, new double[] { 0.101, 0.101 },

            2, numberofvariables, uppers, lowers);

            mgd_spsa.MainProgram();

            DateTime d2 = DateTime.Now;
            Console.WriteLine((d2 - d1).TotalSeconds);

            //for(MGD_SPSA.Start = 36; MGD_SPSA.Start >= 3; MGD_SPSA.Start-=3)
            //{
            //    MGD_SPSA.End = MGD_SPSA.Start + 6;

            //    MGD_SPSA mgd_spsa = new MGD_SPSA(10000, new double[] { 60, 60 }, new double[] { 0.6, 0.6 },
            //        new double[] { 30, 30 }, new double[] { 0.602, 0.602 }, new double[] { 0.101, 0.101 },
            //        2, numberofvariables, uppers, lowers);

            //    mgd_spsa.MainProgram();

            //    DateTime d2 = DateTime.Now;
            //    Console.WriteLine((d2 - d1).TotalSeconds);
            //}
#endif

#if Tribe_objective
            int numberofvariables = 374;//188
            double[] uppers = new double[numberofvariables];
            double[] lowers = new double[numberofvariables];

            Input.PropertiesSetting(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0));

            DateTime d1 = DateTime.Now;

            for (int p = 0; p < numberofvariables; p++)
            {
                uppers[p] = 1;
                lowers[p] = 0.01;
            }

            MGD_SPSA3.StaffofForkliftsStart = 10; MGD_SPSA3.StaffofForkliftsEnd = MGD_SPSA3.StaffofForkliftsStart + 20;
            MGD_SPSA3.StaffofUnpackingStart = 10; MGD_SPSA3.StaffofUnpackingEnd = MGD_SPSA3.StaffofUnpackingStart + 20;
            MGD_SPSA3.StaffofPackingStart = 10; MGD_SPSA3.StaffofPackingEnd = MGD_SPSA3.StaffofPackingStart + 20;

            MGD_SPSA3.ForkliftStart = 10; MGD_SPSA3.ForkliftEnd = MGD_SPSA3.ForkliftStart + 30;

            MGD_SPSA3 mgd_spsa3 = new MGD_SPSA3(20000, new double[] { 4.0 * Math.Pow(10, 2), 10.0 * Math.Pow(10, 0), 10.0 * Math.Pow(10, 0), 10.0 * Math.Pow(10, 0), 10.0 * Math.Pow(10, 0) }, 
                new double[] { 0.30 * Math.Pow(10, 0), 0.30 * Math.Pow(10, 0), 0.30 * Math.Pow(10, 0), 0.30 * Math.Pow(10, 0), 0.30 * Math.Pow(10, 0) },
                new double[] { 0.20 * Math.Pow(10, 0), 1.0 * Math.Pow(10, 0), 1.0 * Math.Pow(10, 0), 1.0 * Math.Pow(10, 0), 1.0 * Math.Pow(10, 0) }, 
                new double[] { 0.602, 0.602, 0.602, 0.602, 0.602 }, 
                new double[] { 0.101, 0.101, 0.101, 0.101, 0.101 },
                3, numberofvariables, uppers, lowers);

            mgd_spsa3.MainProgram();

            DateTime d2 = DateTime.Now;
            Console.WriteLine((d2 - d1).TotalSeconds);
#endif

#if Four_objective
            //int numberofvariables = 310;//188
            //double[] uppers = new double[numberofvariables];
            //double[] lowers = new double[numberofvariables];

            //Input.PropertiesSetting(new DateTime(2022, 8, 1, 0, 0, 0), new DateTime(2022, 9, 1, 0, 0, 0));

            //DateTime d1 = DateTime.Now;

            //for (int p = 0; p < numberofvariables; p++)
            //{
            //    uppers[p] = 1;
            //    lowers[p] = 0.01;
            //}

            //New_MGD_SPSA3.StaffofForkliftsStart = 1; New_MGD_SPSA3.StaffofForkliftsEnd = 100;
            //New_MGD_SPSA3.StaffofUnpackingStart = 1; New_MGD_SPSA3.StaffofUnpackingEnd = 100;
            //New_MGD_SPSA3.StaffofPackingStart = 1; New_MGD_SPSA3.StaffofPackingEnd = 100;
            //New_MGD_SPSA3.ForkliftStart = 1; New_MGD_SPSA3.ForkliftEnd = 100;

            //New_MGD_SPSA3.TotalDelayTime = 250; New_MGD_SPSA3.TotalStaffofForklift = 250;
            //New_MGD_SPSA3.TotalStaffofUnpacking = 250; New_MGD_SPSA3.TotalStaffofPacking = 250;
            //New_MGD_SPSA3.TotalForklift = 250;

            //New_MGD_SPSA3 mgd_spsa3 = new New_MGD_SPSA3(20000, new double[] { 0.15 * Math.Pow(10, 3), 0.15 * Math.Pow(10, 3), 0.15 * Math.Pow(10, 3), 0.15 * Math.Pow(10, 3), 0.15 * Math.Pow(10, 3) },
            //    new double[] { 0.10 * Math.Pow(10, 0), 0.10 * Math.Pow(10, 0), 0.10 * Math.Pow(10, 0), 0.10 * Math.Pow(10, 0), 0.10 * Math.Pow(10, 0) },
            //    new double[] { 1.0 * Math.Pow(10, 2), 1.0 * Math.Pow(10, 2), 1.0 * Math.Pow(10, 2), 1.0 * Math.Pow(10, 2), 1.0 * Math.Pow(10, 2) },
            //    new double[] { 0.602, 0.602, 0.602, 0.602, 0.602 },
            //    new double[] { 0.101, 0.101, 0.101, 0.101, 0.101 },
            //    5, numberofvariables, uppers, lowers);

            //mgd_spsa3.MainProgram();

            int populationSize = 20;
            int numberOfVariables = 310; // Example for DTLZ1 with m = 3, k should be 5, so n = m + k - 1 = 7
            int numberOfObjectives = 5;
            int generations = 20000;

            NSGAII nsgaII = new NSGAII(populationSize, numberOfVariables, numberOfObjectives);
            nsgaII.Run(generations);

            DateTime d2 = DateTime.Now;
            Console.WriteLine((d2 - d1).TotalSeconds);
#endif
        }
    }
}