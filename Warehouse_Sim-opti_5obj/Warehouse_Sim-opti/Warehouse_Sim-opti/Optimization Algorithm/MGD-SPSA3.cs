using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse_Sim_opti.Simulation_Model;
using Warehouse_Sim_opti.Simulation_Modules;
using MathNet.Numerics.LinearAlgebra;
using ILOG.Concert;
using ILOG.CPLEX;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Collections;

namespace Warehouse_Sim_opti.Optimization_Algorithm
{
    internal class MGD_SPSA3
    {
        int MaxIterations { get; set; }
        int CurrentIteration { get; set; }
        private static int DimensionofObjective { get; set; }
        private int DimensionofVariables { get; set; }
        private double[] a, c, A, alpha, gamma;
        private double[] a_k
        {
            get
            {
                double[] _a_k = new double[DimensionofObjective];
                for (int obj = 0; obj < DimensionofObjective; obj++)
                    _a_k[obj] = a[obj] * Math.Pow(A[obj] + CurrentIteration + 1, -alpha[obj]);

                return _a_k;
            }
        }

        private double[] c_k
        {
            get
            {
                double[] _c_k = new double[DimensionofObjective];
                for (int obj = 0; obj < DimensionofObjective; obj++)
                    _c_k[obj] = c[obj] * Math.Pow(CurrentIteration + 1, -gamma[obj]);

                return _c_k;
            }
        }

        private double[] Weights { get; set; }
        private double[] Uppers { get; set; }
        private double[] Lowers { get; set; }
        private List<double[]> LeftEnvaluations = new List<double[]>();
        private List<double[]> RightEnvaluations = new List<double[]>();
        public static int StaffofForkliftsStart, StaffofForkliftsEnd, 
            StaffofUnpackingStart, StaffofUnpackingEnd,
            StaffofPackingStart, StaffofPackingEnd;

        public static int ForkliftStart, ForkliftEnd;
        int[] RangeofForklifts = new int[2] { ForkliftStart, ForkliftEnd };
        private double[] Values { get; set; }
        private List<double> thet = new List<double>();
        private List<List<double>> thetplus = new List<List<double>>();
        private List<List<double>> thetminus = new List<List<double>>();
        private int[,] delta { get; set; }
        private List<List<double>> Grad = new List<List<double>>();
        static Random random = new Random((int)DateTime.Now.Ticks);

        public void Initthet() 
        {
            delta = new int[DimensionofObjective, DimensionofVariables];

            for (int obj = 0; obj < DimensionofObjective; obj++)
            {
                Grad.Add(new List<double>());
                LeftEnvaluations.Add(new double[DimensionofObjective]);
                RightEnvaluations.Add(new double[DimensionofObjective]);

                thetminus.Add(new List<double>());
                thetplus.Add(new List<double>());
            }

            for (int p = 0; p < DimensionofVariables; p++)
            {
                thet.Add(random.NextDouble() * (Uppers[p] - Lowers[p]) + Lowers[p]);
                //thet.Add(0.5);

                for (int obj = 0; obj < DimensionofObjective; obj++)
                {
                    Grad[obj].Add(0);
                    thetminus[obj].Add(0);
                    thetplus[obj].Add(0);
                }
            }
            thet[496] = 1.0;
            thet[497] = 1.0;
            thet[498] = 1.0;
            thet[499] = 1.0;
        }

        double diff = 1;
        bool IsFirst = false;
        public void Evaluate()
        {
            for (int obj = 0; obj < DimensionofObjective; obj++)
            {
                for (int p = 0; p < DimensionofVariables; p++)
                {
                    delta[obj, p] = 2 * new Random(unchecked((int)DateTime.Now.Ticks)).Next(2) - 1;

                    thetminus[obj][p] = Math.Min(thet[p] - c_k[obj] * delta[obj, p], Uppers[p]);
                    thetminus[obj][p] = Math.Max(thetminus[obj][p], Lowers[p]);

                    thetplus[obj][p] = Math.Min(thet[p] + c_k[obj] * delta[obj, p], Uppers[p]);
                    thetplus[obj][p] = Math.Max(thetplus[obj][p], Lowers[p]);

                    //Console.WriteLine($"{p}:{(thetplus[obj][p]).ToString("0.000")},{(thetminus[obj][p]).ToString("0.000")}");
                }
            }

            Tuple<double[], double[]> currentValues;
            double[] stackingTimes = new double[3];

            Parallel.Invoke(
                () => { LeftEnvaluations[0] = GetValues(thetminus[0]).Item1; },
                () => { LeftEnvaluations[1] = GetValues(thetminus[1]).Item1; },
                () => { LeftEnvaluations[2] = GetValues(thetminus[2]).Item1; },
                () => { LeftEnvaluations[3] = GetValues(thetminus[3]).Item1; },
                () => { LeftEnvaluations[4] = GetValues(thetminus[4]).Item1; },

                () => { RightEnvaluations[0] = GetValues(thetplus[0]).Item1; },
                () => { RightEnvaluations[1] = GetValues(thetplus[1]).Item1; },
                () => { RightEnvaluations[2] = GetValues(thetplus[2]).Item1; },
                () => { RightEnvaluations[3] = GetValues(thetplus[3]).Item1; },
                () => { RightEnvaluations[4] = GetValues(thetplus[4]).Item1; }
                ) ;      

            for (int p = 0; p < DimensionofVariables; p++)
            {
                for (int obj = 0; obj < DimensionofObjective; obj++)
                {
                    Grad[obj][p] = 0;
                    for (int _obj = 0; _obj < DimensionofObjective; _obj++)
                    {
                        double difference = RightEnvaluations[_obj][obj] - LeftEnvaluations[_obj][obj] == 0 ? 0.0001 : RightEnvaluations[_obj][obj] - LeftEnvaluations[_obj][obj];
                        Grad[obj][p] += (difference) * Math.Pow(2 * c_k[_obj] * delta[_obj, p], -1);
                    }

                    Grad[obj][p] = Grad[obj][p] / DimensionofObjective;
                }
            }

            double[] moldLength = new double[DimensionofObjective];
            for (int obj = 0; obj < DimensionofObjective; obj++)
                moldLength[obj] = Grad[obj].Sum(x => Math.Abs(x));

            for (int p = 0; p < DimensionofVariables; p++)
                for (int obj = 0; obj < DimensionofObjective; obj++)
                    Grad[obj][p] = Grad[obj][p] / moldLength[obj];

            

            Parallel.Invoke(
                () => {
                    List<Vector<double>> vectors = new List<Vector<double>>();
                    foreach (List<double> gradient in Grad)
                        vectors.Add(Vector<double>.Build.DenseOfArray(gradient.ToArray()));

                    Weights = Frank_Wolfe(vectors);
                },
                () =>
                {
                    currentValues = GetValues(thet);
                    Values = currentValues.Item1;
                    stackingTimes = currentValues.Item2;
                }
                );

            using (StreamWriter sw = new StreamWriter($"..\\Output Files\\Optimization_Process.csv", IsFirst))
            {
                if (!IsFirst)
                {
                    sw.WriteLine($"Iterations,Avg_StackingTime, ManpowerofForklifts, ManpowerofUnpacking, ManpowerofPacking, Forklifts, Avg_StackingTime_At_Gate," +
                         $"Avg_StackingTime_At_Inbound, Avg_StackingTime_At_Outbound","$Diff$");

                    IsFirst = true;
                }

                sw.WriteLine($"{CurrentIteration},{(Values[0] * 250.0)},{(Values[1] * (StaffofForkliftsEnd - StaffofForkliftsStart) + StaffofForkliftsStart)}," +
                $"{(Values[2] * (StaffofUnpackingEnd - StaffofUnpackingStart) + StaffofUnpackingStart)},{(Values[3] * (StaffofPackingEnd - StaffofPackingStart) + StaffofPackingStart)}," +
                $"{(Values[4] * (ForkliftEnd - ForkliftStart) + ForkliftStart)},{stackingTimes[0]},{stackingTimes[1]},{stackingTimes[2]},{diff}");

                sw.Close();
            }
            diff = 0;
            for (int p = 0; p < DimensionofVariables; p++)
            {
                double _diff = 0;
                //Console.WriteLine($"{p}||Before:{thet[p]}");
                for (int obj = 0; obj < DimensionofObjective; obj++)
                {
                    if (p < 496)
                        thet[p] = thet[p] - a_k[obj] * Weights[obj] * Grad[obj][p];
                    else if (p == 496)
                        thet[p] = thet[p] - a_k[obj] * Weights[obj] * Grad[obj][p];
                    else if (p == 497)
                        thet[p] = thet[p] - a_k[obj] * Weights[obj] * Grad[obj][p];
                    else if (p == 498)
                        thet[p] = thet[p] - a_k[obj] * Weights[obj] * Grad[obj][p];
                    else if (p == 499)
                        thet[p] = thet[p] - a_k[obj] * Weights[obj] * Grad[obj][p];
                    _diff += Weights[obj] * Grad[obj][p];
                    //Console.WriteLine(a_k[obj] * Weights[obj] * Grad[obj][p]);
                }
                //Console.WriteLine($"{p}:{_diff}");
                diff += Math.Abs(_diff);
                //Console.WriteLine($"{p}||Delta:{a_k[0] * Weights[0] * Grad[0][p] + a_k[1] * Weights[1] * Grad[1][p]+ a_k[1] * Weights[2] * Grad[2][p] + a_k[1] * Weights[3] * Grad[3][p] + a_k[1] * Weights[4] * Grad[4][p]}");
                thet[p] = Math.Min(thet[p], Uppers[p]);
                thet[p] = Math.Max(thet[p], Lowers[p]);
                //Console.WriteLine($"a_k:{a_k[0]},{a_k[1]}");
                //Console.WriteLine($"Gradient:{Weights[0] * Grad[0][p]}, {Weights[1] * Grad[1][p]}");
                //Console.WriteLine($"{p}||After:{thet[p]}");
            }

            Console.Write($"Itera:{CurrentIteration.ToString("00000")} || ");
            for (int obj = 0; obj < DimensionofObjective; obj++)
            {
                if (obj == 0)
                {
                    double times = 250.0;

                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{((LeftEnvaluations[0][obj] + LeftEnvaluations[1][obj] + LeftEnvaluations[2][obj]) * times / 3.0).ToString("0000.00")} || " +
                        $"Right:{((RightEnvaluations[0][obj] + RightEnvaluations[1][obj] + RightEnvaluations[2][obj]) * times / 3.0).ToString("0000.00")} || ");
                    Console.Write($"Value:{(Values[obj] * times).ToString("0000.00")} || ");
                }
                else if (obj == 1)
                {
                    double times = (StaffofForkliftsEnd - StaffofForkliftsStart);

                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(((LeftEnvaluations[0][obj] + LeftEnvaluations[1][obj] + LeftEnvaluations[2][obj]) * times + 3 * StaffofForkliftsStart) / 3.0).ToString("00")} || " +
                        $"Right:{(((RightEnvaluations[0][obj] + RightEnvaluations[1][obj] + RightEnvaluations[2][obj]) * times + 3 * StaffofForkliftsStart) / 3.0).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * times + StaffofForkliftsStart).ToString("00")} || ");
                }
                else if (obj == 2)
                {
                    double times = (StaffofUnpackingEnd - StaffofUnpackingStart);

                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(((LeftEnvaluations[0][obj] + LeftEnvaluations[1][obj] + LeftEnvaluations[2][obj]) * times + 3 * StaffofUnpackingStart) / 3.0).ToString("00")} || " +
                        $"Right:{(((RightEnvaluations[0][obj] + RightEnvaluations[1][obj] + RightEnvaluations[2][obj]) * times + 3 * StaffofUnpackingStart) / 3.0).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * times + StaffofUnpackingStart).ToString("00")} || ");
                }
                else if (obj == 3)
                {
                    double times = (StaffofPackingEnd - StaffofPackingStart);

                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(((LeftEnvaluations[0][obj] + LeftEnvaluations[1][obj] + LeftEnvaluations[2][obj]) * times + 3 * StaffofPackingStart) / 3.0).ToString("00")} || " +
                        $"Right:{(((RightEnvaluations[0][obj] + RightEnvaluations[1][obj] + RightEnvaluations[2][obj]) * times + 3 * StaffofPackingStart) / 3.0).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * times + StaffofPackingStart).ToString("00")} || ");
                }
                else if (obj == 4)
                {

                    double times = (ForkliftEnd - ForkliftStart);

                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(((LeftEnvaluations[0][obj] + LeftEnvaluations[1][obj] + LeftEnvaluations[2][obj]) * times + 3 * ForkliftStart) / 3.0).ToString("00")} || " +
                        $"Right:{(((RightEnvaluations[0][obj] + RightEnvaluations[1][obj] + RightEnvaluations[2][obj]) * times + 3 * ForkliftStart) / 3.0).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * times + ForkliftStart).ToString("00")} || ");
                }
            }
            Console.Write($"{diff}");
            Console.WriteLine();
        }

        public Tuple<double[], double[]> GetValues(List<double> decisionVariables)
        {
            Tuple<int, int, int, int, int[][]> inputs = Compile(decisionVariables);
            int maxManpowerofForklifts = inputs.Item1;
            int maxManpowerofUnpacking = inputs.Item2;
            int maxManpowerofPacking = inputs.Item3;
            int maxForklift = inputs.Item4;
            int[][] manpowerSchedule = inputs.Item5;

            Simulation_System warehouse = new Simulation_System(maxManpowerofForklifts, maxManpowerofUnpacking, 
                maxManpowerofPacking, maxForklift, manpowerSchedule);

            double[] values = warehouse.GetKPIs();
            double[] _values = new double[DimensionofObjective];
            _values[0] = values[0] / 250.0;
            _values[1] = (values[1]- StaffofForkliftsStart) / (StaffofForkliftsEnd - StaffofForkliftsStart);
            _values[2] = (values[2] - StaffofUnpackingStart) / (StaffofUnpackingEnd - StaffofUnpackingStart);
            _values[3] = (values[3] - StaffofPackingStart) / (StaffofPackingEnd - StaffofPackingStart);
            _values[4] = (values[4] - ForkliftStart) / (ForkliftEnd - ForkliftStart);

            double[] stackingTimes = warehouse.StackingTimes;
            //Console.WriteLine(values[3]);
            return new Tuple<double[], double[]>(_values, stackingTimes);
        }

        public Tuple<double[], double[]> GetValues2(List<double> decisionVariables)
        {
            Tuple<int, int, int, int, int[][]> inputs = Compile(decisionVariables);
            int maxManpowerofForklifts = inputs.Item1;
            int maxManpowerofUnpacking = inputs.Item2;
            int maxManpowerofPacking = inputs.Item3;
            int maxForklift = inputs.Item4;
            int[][] manpowerSchedule = inputs.Item5;

            Simulation_System warehouse = new Simulation_System(maxManpowerofForklifts, maxManpowerofUnpacking,
                maxManpowerofPacking, maxForklift, manpowerSchedule);

            double[] values = warehouse.GetKPIs();
            double[] _values = new double[DimensionofObjective];
            _values[0] = values[0] / 250.0;
            _values[1] = (values[1] - StaffofForkliftsStart) / (StaffofForkliftsEnd - StaffofForkliftsStart);
            _values[2] = (values[2] - StaffofUnpackingStart) / (StaffofUnpackingEnd - StaffofUnpackingStart);
            _values[3] = (values[3] - StaffofPackingStart) / (StaffofPackingEnd - StaffofPackingStart);
            _values[4] = (values[4] - ForkliftStart) / (ForkliftEnd - ForkliftStart);

            double[] stackingTimes = warehouse.StackingTimes;
            //Console.WriteLine(values[3]);
            return new Tuple<double[], double[]>(_values, stackingTimes);
        }

        public Tuple<int, int, int, int, int[][]> Compile(List<double> decisionvariables)
        {
            int days = (int)(Input.EndTime - Input.StartTime).TotalDays;
            int[][] manpowerShiftSchedule = new int[days][];
            int allocatedManpowerofForklifts = (int)Math.Round(decisionvariables[496] * (StaffofForkliftsEnd - StaffofForkliftsStart), 0) + StaffofForkliftsStart - 6;
            int allocatedManpowerofUnpacking = (int)Math.Round(decisionvariables[497] * (StaffofUnpackingEnd - StaffofUnpackingStart), 0) + StaffofUnpackingStart - 2;
            int allocatedManpowerofPacking = (int)Math.Round(decisionvariables[498] * (StaffofPackingEnd - StaffofPackingStart), 0) + StaffofPackingStart - 2;

            int allocatedForklifts = (int)Math.Round(decisionvariables[499] * (ForkliftEnd - ForkliftStart), 0)+ ForkliftStart - 3;
            for (int day = 0; day < days; day++)
            {
                manpowerShiftSchedule[day] = new int[16];

                double totalStaffsofForkliftsRates = decisionvariables[day * 16 + 0] + decisionvariables[day * 16 + 1] + decisionvariables[day * 16 + 3]
                    + decisionvariables[day * 16 + 8] + decisionvariables[day * 16 + 9] + decisionvariables[day * 16 + 11];

                List<double> staffsofForkliftsAllocatedRates = new List<double>()
                {
                    decisionvariables[day * 16 + 0]/totalStaffsofForkliftsRates,
                    decisionvariables[day * 16 + 1]/totalStaffsofForkliftsRates,
                    decisionvariables[day * 16 + 3]/totalStaffsofForkliftsRates,
                    decisionvariables[day * 16 + 8]/totalStaffsofForkliftsRates,
                    decisionvariables[day * 16 + 9]/totalStaffsofForkliftsRates,
                    decisionvariables[day * 16 + 11]/totalStaffsofForkliftsRates
                };

                List<int> allocatedStaffsofForklifts = new List<int>();
                foreach (double allocatedrate in staffsofForkliftsAllocatedRates)
                    allocatedStaffsofForklifts.Add((int)Math.Round(allocatedrate * allocatedManpowerofForklifts, 0));
                int currentAllocatedStaffsofForklifts = allocatedStaffsofForklifts.Sum();

                double totalStaffsofUnpackingRates = decisionvariables[day * 16 + 2] + decisionvariables[day * 16 + 10];

                List<double> staffsofUnpackingAllocatedRates = new List<double>()
                {
                    decisionvariables[day * 16 + 2]/totalStaffsofUnpackingRates,
                    decisionvariables[day * 16 + 10]/totalStaffsofUnpackingRates
                };

                List<int> allocatedStaffsofUnpacking = new List<int>();
                foreach (double allocatedrate in staffsofUnpackingAllocatedRates)
                    allocatedStaffsofUnpacking.Add((int)Math.Round(allocatedrate * allocatedManpowerofUnpacking, 0));
                int currentAllocatedStaffsofUnpacking = allocatedStaffsofUnpacking.Sum();

                double totalStaffsofPackingRates = decisionvariables[day * 16 + 4] + decisionvariables[day * 16 + 12];

                List<double> staffsofPackingAllocatedRates = new List<double>()
                {
                    decisionvariables[day * 16 + 4]/totalStaffsofPackingRates,
                    decisionvariables[day * 16 + 12]/totalStaffsofPackingRates
                };

                List<int> allocatedStaffsofPacking = new List<int>();
                foreach (double allocatedrate in staffsofPackingAllocatedRates)
                    allocatedStaffsofPacking.Add((int)Math.Round(allocatedrate * allocatedManpowerofPacking, 0));
                int currentAllocatedStaffsofPacking = allocatedStaffsofPacking.Sum();

                double totalForkliftsRateAM = decisionvariables[day * 16 + 5] + decisionvariables[day * 16 + 6] + decisionvariables[day * 16 + 7];
                List<double> forkliftsAllocatedRatesAM = new List<double>()
                {
                    decisionvariables[day * 16 + 5]/totalForkliftsRateAM,
                    decisionvariables[day * 16 + 6]/totalForkliftsRateAM,
                    decisionvariables[day * 16 + 7]/totalForkliftsRateAM
                };

                List<int> allocatedForkliftsAM = new List<int>();
                foreach (double allocatedrate in forkliftsAllocatedRatesAM)
                    allocatedForkliftsAM.Add((int)Math.Round(allocatedrate * allocatedForklifts, 0));
                int currentAllocatedForkliftsAM = allocatedForkliftsAM.Sum();

                double totalForkliftsRatePM = decisionvariables[day * 16 + 13] + decisionvariables[day * 16 + 14] + decisionvariables[day * 16 + 15];
                List<double> forkliftsAllocatedRatesPM = new List<double>()
                {
                    decisionvariables[day * 16 + 13]/totalForkliftsRatePM,
                    decisionvariables[day * 16 + 14]/totalForkliftsRatePM,
                    decisionvariables[day * 16 + 15]/totalForkliftsRatePM
                };

                List<int> allocatedForkliftsPM = new List<int>();
                foreach (double allocatedrate in forkliftsAllocatedRatesPM)
                    allocatedForkliftsPM.Add((int)Math.Round(allocatedrate * allocatedForklifts, 0));
                int currentAllocatedForkliftsPM = allocatedForkliftsPM.Sum();

                if (currentAllocatedStaffsofForklifts < allocatedManpowerofForklifts)
                {
                    int gap = allocatedManpowerofForklifts - currentAllocatedStaffsofForklifts;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMin = allocatedStaffsofForklifts.IndexOf(allocatedStaffsofForklifts.Min());
                        allocatedStaffsofForklifts[indexofMin]++;
                    }
                }
                else if (currentAllocatedStaffsofForklifts > allocatedManpowerofForklifts)
                {
                    int gap = currentAllocatedStaffsofForklifts - allocatedManpowerofForklifts;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMax = allocatedStaffsofForklifts.IndexOf(allocatedStaffsofForklifts.Max());
                        allocatedStaffsofForklifts[indexofMax]--;
                    }
                }

                if (currentAllocatedStaffsofUnpacking < allocatedManpowerofUnpacking)
                {
                    int gap = allocatedManpowerofUnpacking - currentAllocatedStaffsofUnpacking;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMin = allocatedStaffsofUnpacking.IndexOf(allocatedStaffsofUnpacking.Min());
                        allocatedStaffsofUnpacking[indexofMin]++;
                    }
                }
                else if (currentAllocatedStaffsofUnpacking > allocatedManpowerofUnpacking)
                {
                    int gap = currentAllocatedStaffsofUnpacking - allocatedManpowerofUnpacking;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMax = allocatedStaffsofUnpacking.IndexOf(allocatedStaffsofUnpacking.Max());
                        allocatedStaffsofUnpacking[indexofMax]--;
                    }
                }

                if (currentAllocatedStaffsofPacking < allocatedManpowerofPacking)
                {
                    int gap = allocatedManpowerofPacking - currentAllocatedStaffsofPacking;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMin = allocatedStaffsofPacking.IndexOf(allocatedStaffsofPacking.Min());
                        allocatedStaffsofPacking[indexofMin]++;
                    }
                }
                else if (currentAllocatedStaffsofPacking > allocatedManpowerofPacking)
                {
                    int gap = currentAllocatedStaffsofPacking - allocatedManpowerofPacking;
                    for (int i = 0; i < gap; i++)
                    {
                        int indexofMax = allocatedStaffsofPacking.IndexOf(allocatedStaffsofPacking.Max());
                        allocatedStaffsofPacking[indexofMax]--;
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

                manpowerShiftSchedule[day][0] = allocatedStaffsofForklifts[0] + 1;
                manpowerShiftSchedule[day][1] = allocatedStaffsofForklifts[1] + 1;
                manpowerShiftSchedule[day][2] = allocatedStaffsofUnpacking[0] + 1;

                manpowerShiftSchedule[day][3] = allocatedStaffsofForklifts[2] + 1;
                manpowerShiftSchedule[day][4] = allocatedStaffsofPacking[0] + 1;
                manpowerShiftSchedule[day][5] = allocatedForkliftsAM[0] + 1;
                manpowerShiftSchedule[day][6] = allocatedForkliftsAM[1] + 1;
                manpowerShiftSchedule[day][7] = allocatedForkliftsAM[2] + 1;

                manpowerShiftSchedule[day][8] = allocatedStaffsofForklifts[3] + 1;
                manpowerShiftSchedule[day][9] = allocatedStaffsofForklifts[4] + 1;
                manpowerShiftSchedule[day][10] = allocatedStaffsofUnpacking[1] + 1;

                manpowerShiftSchedule[day][11] = allocatedStaffsofForklifts[5] + 1;
                manpowerShiftSchedule[day][12] = allocatedStaffsofPacking[1] + 1;
                manpowerShiftSchedule[day][13] = allocatedForkliftsPM[0] + 1;
                manpowerShiftSchedule[day][14] = allocatedForkliftsPM[1] + 1;
                manpowerShiftSchedule[day][15] = allocatedForkliftsPM[2] + 1;
            }

            return new Tuple<int, int, int, int, int[][]>(allocatedManpowerofForklifts + 6, allocatedManpowerofUnpacking + 2, allocatedManpowerofPacking + 2, 
                allocatedForklifts + 3, manpowerShiftSchedule);
        }

        public void MainProgram()
        {
            Initthet();

            while (CurrentIteration < MaxIterations)
            {
                Evaluate();
                CurrentIteration++;

                if (CurrentIteration == MaxIterations / 2)
                    alpha = new double[5] { 1.0, 1.0, 1.0, 1.0, 1.0 }; gamma = new double[5] { 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0 };
            }

            Tuple<int, int, int, int, int[][]> finalDecisionVariables = Compile(thet);
            int maxManpowerofForklifts = finalDecisionVariables.Item1;
            int maxManpowerofUnpacking = finalDecisionVariables.Item2;
            int maxManpowerofPacking = finalDecisionVariables.Item3;
            int maxForklift = finalDecisionVariables.Item4;
            int[][] manpowerSchedule = finalDecisionVariables.Item5;

            using (StreamWriter sw = new StreamWriter($"..\\Output Files\\ManpowerShiftSchedule" +
                $"({(Values[0] * 250.0).ToString("0.000")},{(Values[1]*(StaffofForkliftsEnd - StaffofForkliftsStart) + StaffofForkliftsStart).ToString("0.")}," +
                $"{(Values[2] * (StaffofUnpackingEnd - StaffofUnpackingStart) + StaffofUnpackingStart).ToString("0.")},{(Values[3] * (StaffofPackingEnd - StaffofPackingStart) + StaffofPackingStart).ToString("0.")}," +
                $"{(Values[4] * (ForkliftEnd - ForkliftStart)+ ForkliftStart).ToString("0.")}).csv", false))
            {
                int days = (int)(Input.EndTime - Input.StartTime).TotalDays;

                for (int day = 0; day < days; day++)
                {
                    for (int p = 0; p < 16; p++)
                    {
                        sw.Write($"{manpowerSchedule[day][p]},");
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }

            using (StreamWriter sw = new StreamWriter($"..\\Output Files\\StaffsandForklifts" +
                $"({(Values[0] * 250.0).ToString("0.000")},{(Values[1] * (StaffofForkliftsEnd - StaffofForkliftsStart) + StaffofForkliftsStart).ToString("0.")}," +
                $"{(Values[2] * (StaffofUnpackingEnd - StaffofUnpackingStart) + StaffofUnpackingStart).ToString("0.")},{(Values[3] * (StaffofPackingEnd - StaffofPackingStart) + StaffofPackingStart).ToString("0.")}," +
                $"{(Values[4] * (ForkliftEnd - ForkliftStart) + ForkliftStart).ToString("0.")}).csv", false))
            {
                sw.WriteLine($"{maxManpowerofForklifts},{maxManpowerofUnpacking},{maxManpowerofPacking},{maxForklift}");
                sw.Close();
            }
        }

        public MGD_SPSA3(int maxIterations, double[] _a, double[] _c, double[] _A, double[] _alpha, double[] _gamma,
    int numberofobjectives, int numberofvariables, double[] uppers, double[] lowers)
        {
            MaxIterations = maxIterations;

            a = _a; c = _c; A = c; A = _A; alpha = _alpha; gamma = _gamma;

            DimensionofObjective = numberofobjectives;
            DimensionofVariables = numberofvariables;

            Weights = new double[numberofobjectives];
            for (int obj = 0; obj < numberofobjectives; obj++)
                Weights[obj] = 1.0 / numberofobjectives;

            Uppers = uppers; Lowers = lowers;

            Values = new double[DimensionofObjective];

            CurrentIteration = 0;
        }

        public double[] Frank_Wolfe(List<Vector<double>> gradients)
        {
            double[] currentPoint = new double[5] { 0.2, 0.2, 0.2, 0.2, 0.2};
            double judgement = double.MaxValue;
            do
            {
                #region Calculate Direction
                Vector<double> combinatedVector = currentPoint[0]* gradients[0]+
                    currentPoint[1] * gradients[1]+ currentPoint[2] * gradients[2]+
                    currentPoint[3] * gradients[3]+ currentPoint[4] * gradients[4];

                //实例化一个空模型
                Cplex cplexModel = new Cplex();

                //生成决策变量并赋值
                INumVar[][] deVar = new INumVar[1][];
                double[] lb = { 0.0, 0.0, 0.0, 0.0, 0.0 };
                double[] ub = { 1.0, 1.0, 1.0, 1.0, 1.0 };
                string[] deVarName = { "w1", "w2", "w3", "w4", "w5" };
                INumVar[] w = cplexModel.NumVarArray(5, lb, ub, deVarName);
                deVar[0] = w;
                //目标函数
                double[] objCoef = new double[5] { 2 * combinatedVector * gradients[0], 2 * combinatedVector * gradients[1], 2 * combinatedVector * gradients[2],
                 2 * combinatedVector * gradients[3], 2 * combinatedVector * gradients[4]};//目标函数系数(object coefficient)
                                                                                           //
                cplexModel.AddMinimize(cplexModel.ScalProd(w, objCoef));
                //约束条件
                IRange[][] rng = new IRange[1][];
                rng[0] = new IRange[1];

                rng[0][0] = cplexModel.AddEq(cplexModel.Sum(cplexModel.Prod(1.0, w[0]), cplexModel.Prod(1.0, w[1]), cplexModel.Prod(1.0, w[2]),
                    cplexModel.Prod(1.0, w[3]),cplexModel.Prod(1.0, w[4])), 1, "c1");

                //rng[0][1] = cplexModel.AddLe(cplexModel.Prod(1.0, w[0]), 1, "c2");
                //rng[0][2] = cplexModel.AddLe(cplexModel.Prod(-1.0, w[0]), 0, "c3");

                //rng[0][3] = cplexModel.AddLe(cplexModel.Prod(1.0, w[1]), 1, "c4");
                //rng[0][4] = cplexModel.AddLe(cplexModel.Prod(-1.0, w[1]), 0, "c5");

                //rng[0][5] = cplexModel.AddLe(cplexModel.Prod(1.0, w[2]), 1, "c6");
                //rng[0][6] = cplexModel.AddLe(cplexModel.Prod(-1.0, w[2]), 0, "c7");

                cplexModel.ExportModel("lpex1.lp");

                //cplexModel.SetParam(Cplex.Param.Preprocessing.Presolve, false);
                cplexModel.SetOut(null);
                if (cplexModel.Solve())
                {
                    //int nvars = cplexModel.GetValues(deVar[0]).Length;
                    //for (int j = 0; j < nvars; ++j)
                    //{
                    //    Console.WriteLine("Variable   " + j + ": Value = " + cplexModel.GetValues(deVar[0])[j]);
                    //    //cplexModel.Output().WriteLine("Variable   " + j + ": Value = " + cplexModel.GetValues(deVar[0])[j]);
                    //}

                    Vector<double> bestPoint = Vector<double>.Build.DenseOfArray(cplexModel.GetValues(deVar[0]));
                    Vector<double> direction = (bestPoint - Vector<double>.Build.DenseOfArray(currentPoint));
                    cplexModel.End();

                    #region Calculate step size

                    double numerator, denominator = 0;

                    numerator = (currentPoint[0] * gradients[0] + currentPoint[1] * gradients[1] + currentPoint[2] * gradients[2] + currentPoint[3] * gradients[3] + currentPoint[4] * gradients[4]) * 
                        (direction[0] * gradients[0] + direction[1] * gradients[1] + direction[2] * gradients[2] + direction[3] * gradients[3] + direction[4] * gradients[4]);

                    denominator = (direction[0] * gradients[0] + direction[1] * gradients[1] + direction[2] * gradients[2] + direction[3] * gradients[3] + direction[4] * gradients[4]) *
                        (direction[0] * gradients[0] + direction[1] * gradients[1] + direction[2] * gradients[2] + direction[3] * gradients[3] + direction[4] * gradients[4]);

                    double stepSize = Math.Min(1, -numerator / denominator);

                    //Console.WriteLine(stepSize);
                    #endregion

                    currentPoint = (Vector<double>.Build.DenseOfArray(currentPoint) + stepSize * direction).ToArray<double>();

                    combinatedVector = Vector<double>.Build.DenseOfArray(new[] { 2 * combinatedVector * gradients[0], 2 * combinatedVector * gradients[1], 2 * combinatedVector * gradients[2],
                    2 * combinatedVector * gradients[3], 2 * combinatedVector * gradients[4]});

                    judgement = Math.Abs(combinatedVector * direction);
                    combinatedVector = gradients[0] * currentPoint[0] + gradients[1] * currentPoint[1] + gradients[2] * currentPoint[2] + gradients[3] * currentPoint[3] + gradients[4] * currentPoint[4];
                    //Console.WriteLine($"Current Point w1:{currentPoint[0].ToString("0.0000000")}, w2:{currentPoint[1].ToString("0.0000000")}, w3:{currentPoint[2].ToString("0.0000000")}" +
                    //    $",judgement:{judgement.ToString("0.0000")},CurrentValue:{combinatedVector * combinatedVector}");
                }

                #endregion


            } while (judgement >= 0.001);

            return currentPoint;
        }
    }
}
