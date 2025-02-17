using MathNet.Numerics.Providers.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse_Sim_opti.Simulation_Model;
using Warehouse_Sim_opti.Simulation_Modules;

namespace Warehouse_Sim_opti
{
    public class SPSA
    {
        string Order { get; set; }
        int MaxIterations { get; set; }
        int CurrentIteration { get; set; }

        //Paramters for SPSA
        private double a, c, A, alpha, gamma;
        private double a_k => a * Math.Pow(A + CurrentIteration + 1, -alpha);
        private double c_k => c * Math.Pow(CurrentIteration + 1, -gamma);
        //Paramters for SPSA

        private int NumberofObjectives { get; set; }
        private int NumberofVariables { get; set; }
        private double[] Weights { get; set; }
        private double[] Improvement_ratio  { get; set; }

        private double[] Uppers { get; set; }
        private double[] Lowers { get; set; }
        private double[][] LeftEnvaluations { get; set; }
        private double[][] RightEnvaluations { get; set; }
        private double[] Values { get; set; }
        private double[] LastValues { get; set; }

        private double[] thet { get; set; }
        private double[][] thetplus { get; set; }
        private double[][] thetminus { get; set; }

        private int[,] delta { get; set; }
        private double[,] Grad { get; set; }

        Random random = new Random((int)DateTime.Now.Ticks);

        public void Initthet()
        {
            //double[] initializedDVs = new double[12] { 0.118, 0.706, 0.176, 0.2, 0.5, 0.3, 0.118, 0.706, 0.176, 0.2, 0.5, 0.3 };

            //for (int day = 0; day < 31; day++)
            //{
            //    thet = initializedDVs.Concat(thet).ToArray();
            //}

            for (int p = 0; p < NumberofVariables; p++)
                thet[p] = random.NextDouble() * (Uppers[p] - Lowers[p]) + Lowers[p];

            //foreach (double v in initializedDVs)
            //    Console.WriteLine(v);

            Values = GetValues(thet, false);
        }

        public void Evaluate()
        {
            for (int obj = 0; obj < NumberofObjectives; obj++)
            {                
                for (int p = 0; p < NumberofVariables; p++)
                {
                    delta[obj, p] = 2 * new Random(unchecked((int)DateTime.Now.Ticks)).Next(2) - 1;

                    thetminus[obj][p] = Math.Min(thet[p] - c_k * delta[obj, p], Uppers[p]);
                    thetminus[obj][p] = Math.Max(thetminus[obj][p], Lowers[p]);

                    thetplus[obj][p] = Math.Min(thet[p] + c_k * delta[obj, p], Uppers[p]);
                    thetplus[obj][p] = Math.Max(thetplus[obj][p], Lowers[p]);

                    //Console.WriteLine(c_k * delta[obj, p]);
                }
            }
            DateTime d1 = DateTime.Now;
            for (int obj = 0; obj < NumberofObjectives; obj++)
            {
                Parallel.Invoke(() =>{LeftEnvaluations[obj] = GetValues(thetminus[obj], false);},
                () =>{ RightEnvaluations[obj] = GetValues(thetplus[obj], false); },
                () => { Values = GetValues(thet, true); });

                DateTime d2 = DateTime.Now;

                //Console.WriteLine((d2-d1).TotalSeconds);
            }

            for (int p = 0; p < NumberofVariables; p++)
            {
                for (int obj = 0; obj < NumberofObjectives; obj++)
                {                   
                    Grad[obj, p] = (RightEnvaluations[obj][obj] - LeftEnvaluations[obj][obj]) * Math.Pow(2 * c_k * delta[obj, p], -1);
                    thet[p] = thet[p] - a_k * Grad[obj, p] * Weights[obj];

                    //Console.WriteLine(a_k * Grad[obj, p]);
                }

                thet[p] = Math.Min(thet[p], Uppers[p]);
                thet[p] = Math.Max(thet[p], Lowers[p]);
            }

            LastValues = Values;

            for (int obj = 0; obj < NumberofObjectives; obj++)
            {
                Improvement_ratio[obj] = (Values[obj] - LastValues[obj]+0.02)/ (LastValues[obj]+0.01);
                Weights[obj] = Weights[obj] + a_k * Improvement_ratio[obj];
                Console.Write($"Itera:{CurrentIteration} || ");
                Console.Write($"Obj{obj}:Left:{LeftEnvaluations[obj][obj].ToString("0.00000")} || Right:{RightEnvaluations[obj][obj].ToString("0.00000")} || ");
                Console.Write($"Value:{Values[obj].ToString("0.00000")} || ");

                //SW.WriteLine($"{CurrentIteration},{Values[obj]}");
            }
            Console.WriteLine();
        }

        public int MaxManpower = 38;
        public int MaxForklifts = 29;
        //StreamWriter SW = new StreamWriter("..\\optimization_process.csv");               
        bool IsFirst = false;
        public double[] GetValues(double[] decisionvariables, bool currentValue)
        {
            int[][] manpowerShiftSchedule = Compile(decisionvariables);

            Simulation_System warehouse = new Simulation_System(manpowerShiftSchedule);

            double[] kpi = new double[1] { warehouse.GetKPI() };

            if (currentValue)
            {
                using (StreamWriter sw = new StreamWriter($"..\\Output Files\\Optimization_Process.csv", IsFirst))
                {
                    if (!IsFirst)
                    {
                        sw.WriteLine($"Avg_StackingTime,Employee Costs, Avg_StackingTime_At_Gate," +
                             $"Avg_StackingTime_At_Inbound, Avg_StackingTime_At_Outbound");

                        IsFirst = true;
                    }

                    sw.WriteLine($"{CurrentIteration},{kpi[0]},{warehouse.StackingTimes[0]},{warehouse.StackingTimes[1]},{warehouse.StackingTimes[2]}");

                    sw.Close();
                }
            }

            return kpi;

            //if (Order == "Square_testing")
            //{
            //    Square_testing square_testing = new Square_testing(decisionvariables);

            //    square_testing.CalculateValues();

            //    return square_testing.Values;
            //}
            //else if (Order == "ZDT1")
            //{
            //    ZDT1 zdt1 = new ZDT1(decisionvariables);

            //    zdt1.CalculateValues();

            //    return zdt1.Values;
            //}
            //else if (Order == "ZDT4")
            //{
            //    ZDT4 zdt4 = new ZDT4(decisionvariables);

            //    zdt4.CalculateValues();

            //    return zdt4.Values;
            //}
            //else
            //{
            //    return new double[0];
            //}
        }

        public int[][] Compile(double[] decisionvariables)
        {
            int days = (int)(Input.EndTime - Input.StartTime).TotalDays;
            int[][] manpowerShiftSchedule = new int[days][];
            int allocatedManpower = MaxManpower - 6;
            int allocatedForklifts = MaxForklifts - 3;

            for (int day = 0; day < days; day++)
            {
                manpowerShiftSchedule[day] = new int[12];

                double totalStaffsRates = decisionvariables[day * 12 + 0] + decisionvariables[day * 12 + 1] + decisionvariables[day * 12 + 2]
                    + decisionvariables[day * 12 + 6] + decisionvariables[day * 12 + 7] + decisionvariables[day * 12 + 8];
                List<double> staffsAllocatedRates = new List<double>()
                {
                    decisionvariables[day * 12 + 0]/totalStaffsRates,
                    decisionvariables[day * 12 + 1]/totalStaffsRates,
                    decisionvariables[day * 12 + 2]/totalStaffsRates,
                    decisionvariables[day * 12 + 6]/totalStaffsRates,
                    decisionvariables[day * 12 + 7]/totalStaffsRates,
                    decisionvariables[day * 12 + 8]/totalStaffsRates
                };

                List<int> allocatedStaffs = new List<int>();
                foreach (double allocatedrate in staffsAllocatedRates)
                    allocatedStaffs.Add((int)Math.Round(allocatedrate * allocatedManpower, 0));
                int currentAllocatedStaffs = allocatedStaffs.Sum();

                double totalForkliftsRateAM = decisionvariables[day * 12 + 3] + decisionvariables[day * 12 + 4] + decisionvariables[day * 12 + 5];
                List<double> forkliftsAllocatedRatesAM = new List<double>()
                {
                    decisionvariables[day * 12 + 3]/totalForkliftsRateAM,
                    decisionvariables[day * 12 + 4]/totalForkliftsRateAM,
                    decisionvariables[day * 12 + 5]/totalForkliftsRateAM
                };

                List<int> allocatedForkliftsAM = new List<int>();
                foreach (double allocatedrate in forkliftsAllocatedRatesAM)
                    allocatedForkliftsAM.Add((int)Math.Round(allocatedrate * allocatedForklifts, 0));
                int currentAllocatedForkliftsAM = allocatedForkliftsAM.Sum();

                double totalForkliftsRatePM = decisionvariables[day * 12 + 9] + decisionvariables[day * 12 + 10] + decisionvariables[day * 12 + 11];
                List<double> forkliftsAllocatedRatesPM = new List<double>()
                {
                    decisionvariables[day * 12 + 9]/totalForkliftsRatePM,
                    decisionvariables[day * 12 + 10]/totalForkliftsRatePM,
                    decisionvariables[day * 12 + 11]/totalForkliftsRatePM
                };

                List<int> allocatedForkliftsPM = new List<int>();
                foreach (double allocatedrate in forkliftsAllocatedRatesPM)
                    allocatedForkliftsPM.Add((int)Math.Round(allocatedrate * allocatedForklifts, 0));
                int currentAllocatedForkliftsPM = allocatedForkliftsPM.Sum();

                if (currentAllocatedStaffs < allocatedManpower)
                {
                    int gap = allocatedManpower - currentAllocatedStaffs;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMin = allocatedStaffs.IndexOf(allocatedStaffs.Min());
                        allocatedStaffs[indexofMin]++;
                    }
                }
                else if (currentAllocatedStaffs > allocatedManpower)
                {
                    int gap = currentAllocatedStaffs - allocatedManpower;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMax = allocatedStaffs.IndexOf(allocatedStaffs.Max());
                        allocatedStaffs[indexofMax]--;
                    }
                }

                if (currentAllocatedForkliftsAM < allocatedForklifts)
                {
                    int gap = allocatedForklifts - currentAllocatedForkliftsAM;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMin = allocatedForkliftsAM.IndexOf(allocatedForkliftsAM.Min());
                        allocatedForkliftsAM[indexofMin]++;
                    }
                }
                else if (currentAllocatedForkliftsAM > allocatedForklifts)
                {
                    int gap = currentAllocatedForkliftsAM - allocatedForklifts;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMax = allocatedForkliftsAM.IndexOf(allocatedForkliftsAM.Max());
                        allocatedForkliftsAM[indexofMax]--;
                    }
                }

                if (currentAllocatedForkliftsPM < allocatedForklifts)
                {
                    int gap = allocatedForklifts - currentAllocatedForkliftsPM;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMin = allocatedForkliftsPM.IndexOf(allocatedForkliftsPM.Min());
                        allocatedForkliftsPM[indexofMin]++;
                    }
                }
                else if (currentAllocatedForkliftsPM > allocatedForklifts)
                {
                    int gap = currentAllocatedForkliftsPM - allocatedForklifts;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMax = allocatedForkliftsPM.IndexOf(allocatedForkliftsPM.Max());
                        allocatedForkliftsPM[indexofMax]--;
                    }
                }

                //if (allocatedStaffs[0] > allocatedForkliftsAM[0])
                //{
                //    int gap = allocatedStaffs[0] -allocatedForkliftsAM[0];
                //    allocatedStaffs[0] -= gap;
                //    for (int i = 0; i < gap; i++)
                //    {
                //        if (allocatedStaffs[1] > allocatedStaffs[2])
                //            allocatedStaffs[2]++;
                //        else allocatedStaffs[1]++;

                //    }
                //}
                //else if (allocatedStaffs[0] < allocatedForkliftsAM[0])
                //{
                //    int gap = allocatedForkliftsAM[0] - allocatedStaffs[0];
                //    allocatedForkliftsAM[0] -= gap;
                //    for (int i = 0; i < gap; i++)
                //    {
                //        if(allocatedForkliftsAM[1] > allocatedForkliftsAM[2])
                //            allocatedForkliftsAM[2]++;
                //        else allocatedForkliftsAM[1]++;
                //    }
                //}

                //if (allocatedStaffs[3] > allocatedForkliftsPM[0])
                //{
                //    int gap = allocatedStaffs[3] - allocatedForkliftsPM[0];
                //    allocatedStaffs[3] -= gap;
                //    for (int i = 0; i < gap; i++)
                //    {
                //        if (allocatedStaffs[4] > allocatedStaffs[5])
                //            allocatedStaffs[4]++;
                //        else allocatedStaffs[5]++;

                //    }
                //}
                //else if (allocatedStaffs[3] < allocatedForkliftsPM[0])
                //{
                //    int gap = allocatedForkliftsPM[0] - allocatedStaffs[3];
                //    allocatedForkliftsPM[0] -= gap;
                //    for (int i = 0; i < gap; i++)
                //    {
                //        if (allocatedForkliftsPM[1] > allocatedForkliftsPM[2])
                //            allocatedForkliftsPM[2]++;
                //        else allocatedForkliftsPM[1]++;
                //    }
                //}

                //if (allocatedStaffs[1] < allocatedForkliftsAM[1])
                //{
                //    int gap = allocatedForkliftsAM[1] - allocatedStaffs[1];
                //    allocatedForkliftsAM[1] -= gap;
                //    allocatedForkliftsAM[2] += gap;
                //}

                //if (allocatedStaffs[2] < allocatedForkliftsAM[2])
                //{
                //    int gap = allocatedForkliftsAM[2] - allocatedStaffs[2];
                //    allocatedForkliftsAM[2] -= gap;
                //    allocatedForkliftsAM[1] += gap;
                //}

                //if (allocatedStaffs[4] < allocatedForkliftsPM[1])
                //{
                //    int gap = allocatedForkliftsPM[1] - allocatedStaffs[4];
                //    allocatedForkliftsPM[1] -= gap;
                //    allocatedForkliftsPM[2] += gap;
                //}

                //if (allocatedStaffs[5] < allocatedForkliftsPM[2])
                //{
                //    int gap = allocatedForkliftsPM[2] - allocatedStaffs[5];
                //    allocatedForkliftsPM[2] -= gap;
                //    allocatedForkliftsPM[1] += gap;
                //}

                manpowerShiftSchedule[day][0] = allocatedStaffs[0] + 1;
                manpowerShiftSchedule[day][1] = allocatedStaffs[1] + 1;
                manpowerShiftSchedule[day][2] = allocatedStaffs[2] + 1;

                manpowerShiftSchedule[day][3] = allocatedForkliftsAM[0] + 1;
                manpowerShiftSchedule[day][4] = allocatedForkliftsAM[1] + 1;
                manpowerShiftSchedule[day][5] = allocatedForkliftsAM[2] + 1;

                manpowerShiftSchedule[day][6] = allocatedStaffs[3] + 1;
                manpowerShiftSchedule[day][7] = allocatedStaffs[4] + 1;
                manpowerShiftSchedule[day][8] = allocatedStaffs[5] + 1;

                manpowerShiftSchedule[day][9] = allocatedForkliftsPM[0] + 1;
                manpowerShiftSchedule[day][10] = allocatedForkliftsPM[1] + 1;
                manpowerShiftSchedule[day][11] = allocatedForkliftsPM[2] + 1;
            }

            return(manpowerShiftSchedule);
        }

        public void MainProgram()
        {            
            Initthet();

            while (CurrentIteration < MaxIterations)
            {
                Evaluate();
                CurrentIteration++;

                if (CurrentIteration == MaxIterations / 2)
                    alpha = 1.0; gamma = 1.0 / 6.0;
            }

            using (StreamWriter sw = new StreamWriter("..\\Output Files\\ManpowerShiftSchedule.csv", false))
            {
                List<int> dvs = new List<int>();

                int days = (int)(Input.EndTime - Input.StartTime).TotalDays;
                int[][] manpowerShiftSchedule = Compile(thet);
                         
                for (int day = 0; day < days; day++)
                {
                    for (int it = 0; it < 12; it++)
                    {
                        sw.Write($"{manpowerShiftSchedule[day][it]},");
                    }
                    sw.WriteLine();
                }

                sw.Close();
            }
        }

        public SPSA(int maxIterations, double _a, double _c, double _A, double _alpha, double _gamma, 
            int numberofobjectives, int numberofvariables, double[] uppers, double[] lowers, string order)
        {
            MaxIterations = maxIterations;

            a = _a; c = _c; A = c; A = _A; alpha = _alpha; gamma = _gamma;
          
            NumberofObjectives = numberofobjectives;
            NumberofVariables = numberofvariables;

            Weights = new double[numberofobjectives];
            Improvement_ratio = new double[numberofobjectives];
            for (int obj= 0; obj < numberofobjectives; obj++)
                Weights[obj] = 1.0 / numberofobjectives;

            Uppers = uppers; Lowers = lowers;

            LeftEnvaluations = new double[NumberofObjectives][];
            for (int obj = 0; obj < NumberofObjectives; obj++) { LeftEnvaluations[obj] = new double[NumberofVariables]; }
            
            RightEnvaluations = new double[NumberofObjectives][];
            for (int obj = 0; obj < NumberofObjectives; obj++) { RightEnvaluations[obj] = new double[NumberofVariables]; }
            
            Values = new double[NumberofObjectives];

            thet = new double[NumberofVariables];

            thetplus = new double[NumberofObjectives][];
            for (int obj = 0; obj < NumberofObjectives; obj++) { thetplus[obj] = new double[NumberofVariables]; }

            thetminus = new double[NumberofObjectives][];
            for(int obj = 0; obj < NumberofObjectives; obj++) { thetminus[obj] = new double[NumberofVariables]; }

            delta = new int[NumberofObjectives, NumberofVariables];
            Grad = new double[NumberofObjectives, NumberofVariables];

            CurrentIteration = 0;

            Order= order;
        }
    }
}
