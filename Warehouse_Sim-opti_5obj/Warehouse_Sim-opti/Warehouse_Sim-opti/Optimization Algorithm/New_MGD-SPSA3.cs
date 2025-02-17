using Warehouse_Sim_opti.Simulation_Model;
using Warehouse_Sim_opti.Simulation_Modules;
using MathNet.Numerics.LinearAlgebra;
using ILOG.Concert;
using ILOG.CPLEX;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Collections;

namespace Warehouse_Sim_opti.Optimization_Algorithm
{
    internal class New_MGD_SPSA3
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
        public static double TotalDelayTime;
        public static int TotalStaffofForklift, TotalStaffofUnpacking, TotalStaffofPacking, TotalForklift;
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
                );

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
                         $"Avg_StackingTime_At_Inbound, Avg_StackingTime_At_Outbound", "$Diff$");

                    IsFirst = true;
                }

                sw.WriteLine($"{CurrentIteration},{(Values[0] * TotalDelayTime)},{Values[1] * TotalStaffofForklift}," +
                $"{Values[2] * TotalStaffofUnpacking},{Values[3] * TotalStaffofPacking}," +
                $"{Values[4] * TotalForklift},{stackingTimes[0]},{stackingTimes[1]},{stackingTimes[2]},{diff}");

                sw.Close();
            }
            diff = 0;
            for (int p = 0; p < DimensionofVariables; p++)
            {
                double _diff = 0;
                //Console.WriteLine($"{p}||Before:{thet[p]}");
                for (int obj = 0; obj < DimensionofObjective; obj++)
                {
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
                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(Enumerable.Range(0, DimensionofObjective).Sum(i => LeftEnvaluations[i][obj]) * TotalDelayTime / DimensionofObjective).ToString("0000.00")} || " +
                        $"Right:{(Enumerable.Range(0, DimensionofObjective).Sum(i => RightEnvaluations[i][obj]) * TotalDelayTime / DimensionofObjective).ToString("0000.00")} || ");
                    Console.Write($"Value:{(Values[obj] * TotalDelayTime).ToString("0000.00")} || ");
                }
                else if (obj == 1)
                {
                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(Enumerable.Range(0, DimensionofObjective).Sum(i => LeftEnvaluations[i][obj]) * TotalStaffofForklift / DimensionofObjective).ToString("00")} || " +
                        $"Right:{(Enumerable.Range(0, DimensionofObjective).Sum(i => RightEnvaluations[i][obj]) * TotalStaffofForklift / DimensionofObjective).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * TotalStaffofForklift).ToString("00")} || ");
                }
                else if (obj == 2)
                {
                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(Enumerable.Range(0, DimensionofObjective).Sum(i => LeftEnvaluations[i][obj]) * TotalStaffofUnpacking / DimensionofObjective).ToString("00")} || " +
                        $"Right:{(Enumerable.Range(0, DimensionofObjective).Sum(i => RightEnvaluations[i][obj]) * TotalStaffofUnpacking / DimensionofObjective).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * TotalStaffofUnpacking).ToString("00")} || ");
                }
                else if (obj == 3)
                {
                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(Enumerable.Range(0, DimensionofObjective).Sum(i => LeftEnvaluations[i][obj]) * TotalStaffofPacking / DimensionofObjective).ToString("00")} || " +
                        $"Right:{(Enumerable.Range(0, DimensionofObjective).Sum(i => RightEnvaluations[i][obj]) * TotalStaffofPacking / DimensionofObjective).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * TotalStaffofPacking).ToString("00")} || ");
                }
                else if (obj == 4)
                {
                    //Console.Write($"Weight:{Weights[obj].ToString("0.00")} || ");
                    Console.Write($"Obj{obj}:Left:{(Enumerable.Range(0, DimensionofObjective).Sum(i => LeftEnvaluations[i][obj]) * TotalForklift / DimensionofObjective).ToString("00")} || " +
                        $"Right:{(Enumerable.Range(0, DimensionofObjective).Sum(i => RightEnvaluations[i][obj]) * TotalForklift / DimensionofObjective).ToString("00")} || ");
                    Console.Write($"Value:{(Values[obj] * TotalForklift).ToString("00")} || ");
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
            _values[0] = values[0] / TotalDelayTime;
            _values[1] = (values[1] / TotalStaffofForklift);
            _values[2] = (values[2] / TotalStaffofUnpacking);
            _values[3] = (values[3] / TotalStaffofPacking);
            _values[4] = (values[4] / TotalForklift);

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
            _values[0] = values[0] / TotalDelayTime;
            _values[1] = (values[1]/TotalStaffofForklift);
            _values[2] = (values[2]/TotalStaffofUnpacking);
            _values[3] = (values[3]/TotalStaffofPacking);
            _values[4] = (values[4]/TotalForklift);

            double[] stackingTimes = warehouse.StackingTimes;
            //Console.WriteLine(values[3]);
            return new Tuple<double[], double[]>(_values, stackingTimes);
        }

        public Tuple<int, int, int, int, int[][]> Compile(List<double> decisionvariables)
        {
            int days = (int)(Input.EndTime - Input.StartTime).TotalDays;
            int[][] manpowerShiftSchedule = new int[days][];
            int allocatedManpowerofForklifts = 0;
            int allocatedManpowerofUnpacking = 0;
            int allocatedManpowerofPacking = 0;
            int allocatedForklifts = 0;

            for (int day = 0; day < days; day++)
            {
                manpowerShiftSchedule[day] = new int[16];

                int today_manpower_forklift = 0;
                int today_manpower_unpacking = 0;
                int today_manpower_packing = 0;
                int today_forklift_am = 0;
                int today_forklift_pm = 0;

                manpowerShiftSchedule[day][0] = (int)Math.Round(decisionvariables[day * 10 + 0] * (StaffofForkliftsEnd - StaffofForkliftsStart), 0) + StaffofForkliftsStart;
                manpowerShiftSchedule[day][1] = (int)Math.Round(decisionvariables[day * 10 + 1] * (StaffofForkliftsEnd - StaffofForkliftsStart), 0) + StaffofForkliftsStart;
                manpowerShiftSchedule[day][2] = (int)Math.Round(decisionvariables[day * 10 + 2] * (StaffofUnpackingEnd - StaffofUnpackingStart), 0) + StaffofUnpackingStart;
                manpowerShiftSchedule[day][3] = (int)Math.Round(decisionvariables[day * 10 + 3] * (StaffofForkliftsEnd - StaffofForkliftsStart), 0) + StaffofForkliftsStart;
                manpowerShiftSchedule[day][4] = (int)Math.Round(decisionvariables[day * 10 + 4] * (StaffofPackingEnd - StaffofPackingStart), 0) + StaffofPackingStart;
                manpowerShiftSchedule[day][5] = manpowerShiftSchedule[day][0];
                manpowerShiftSchedule[day][6] = manpowerShiftSchedule[day][1];
                manpowerShiftSchedule[day][7] = manpowerShiftSchedule[day][3];
                today_manpower_forklift += manpowerShiftSchedule[day][0]+ manpowerShiftSchedule[day][1]+manpowerShiftSchedule[day][3];
                today_manpower_unpacking += manpowerShiftSchedule[day][2];
                today_manpower_packing += manpowerShiftSchedule[day][4];
                today_forklift_am += manpowerShiftSchedule[day][5] + manpowerShiftSchedule[day][6] + manpowerShiftSchedule[day][7];

                manpowerShiftSchedule[day][8] = (int)Math.Round(decisionvariables[day * 10 + 5] * (StaffofForkliftsEnd - StaffofForkliftsStart), 0) + StaffofForkliftsStart;
                manpowerShiftSchedule[day][9] = (int)Math.Round(decisionvariables[day * 10 + 6] * (StaffofForkliftsEnd - StaffofForkliftsStart), 0) + StaffofForkliftsStart;
                manpowerShiftSchedule[day][10] = (int)Math.Round(decisionvariables[day * 10 + 7] * (StaffofUnpackingEnd - StaffofUnpackingStart), 0) + StaffofUnpackingStart;
                manpowerShiftSchedule[day][11] = (int)Math.Round(decisionvariables[day * 10 + 8] * (StaffofForkliftsEnd - StaffofForkliftsStart), 0) + StaffofForkliftsStart;
                manpowerShiftSchedule[day][12] = (int)Math.Round(decisionvariables[day * 10 + 9] * (StaffofPackingEnd - StaffofPackingStart), 0) + StaffofPackingStart;
                manpowerShiftSchedule[day][13] = manpowerShiftSchedule[day][8];
                manpowerShiftSchedule[day][14] = manpowerShiftSchedule[day][9];
                manpowerShiftSchedule[day][15] = manpowerShiftSchedule[day][11];
                today_manpower_forklift += manpowerShiftSchedule[day][8] + manpowerShiftSchedule[day][9] + manpowerShiftSchedule[day][11];
                today_manpower_unpacking += manpowerShiftSchedule[day][10];
                today_manpower_packing += manpowerShiftSchedule[day][12];
                today_forklift_pm += manpowerShiftSchedule[day][13] + manpowerShiftSchedule[day][14] + manpowerShiftSchedule[day][15];

                allocatedManpowerofForklifts = Math.Max(allocatedManpowerofForklifts, today_manpower_forklift);
                allocatedManpowerofUnpacking = Math.Max(allocatedManpowerofUnpacking, today_manpower_unpacking);
                allocatedManpowerofPacking = Math.Max(allocatedManpowerofPacking, today_manpower_packing);
                allocatedForklifts = Math.Max(allocatedForklifts, Math.Max(today_forklift_am, today_forklift_pm));
            }

            return new Tuple<int, int, int, int, int[][]>(allocatedManpowerofForklifts, allocatedManpowerofUnpacking, allocatedManpowerofPacking,
                allocatedForklifts, manpowerShiftSchedule);
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
                $"({(Values[0] * TotalDelayTime).ToString("0.000")},{maxManpowerofForklifts.ToString("0.")}," +
                $"{maxManpowerofUnpacking.ToString("0.")},{maxManpowerofPacking.ToString("0.")}," +
                $"{maxForklift.ToString("0.")}).csv", false))
            {
                int days = (int)(Input.EndTime - Input.StartTime).TotalDays;

                sw.WriteLine($"{"Dock_staff"},{"In_Staff_Dri"},{"In_Staff_Unp"},{"Out_Staff_Dri"},{"Out_Staff_Pa"},{"Dock_Forklift"}," +
                    $"{"In_Forklift"},{"Out_Forklift"},{"Dock_staff"},{"In_Staff_Dri"},{"In_Staff_Unp"},{"Out_Staff_Dri"},{"Out_Staff_Pa"}," +
                    $"{"Dock_Forklift"},{"In_Forklift"},{"Out_Forklift"}");

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
                $"({(Values[0] * TotalDelayTime).ToString("0.000")},{maxManpowerofForklifts.ToString("0.")}," +
                $"{maxManpowerofUnpacking.ToString("0.")},{maxManpowerofPacking.ToString("0.")}," +
                $"{maxForklift.ToString("0.")}).csv", false))
            {
                sw.WriteLine($"{maxManpowerofForklifts},{maxManpowerofUnpacking},{maxManpowerofPacking},{maxForklift}");
                sw.Close();
            }
        }

        public New_MGD_SPSA3(int maxIterations, double[] _a, double[] _c, double[] _A, double[] _alpha, double[] _gamma,
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
            double[] currentPoint = new double[5] { 0.2, 0.2, 0.2, 0.2, 0.2 };
            double judgement = double.MaxValue;
            do
            {
                #region Calculate Direction
                Vector<double> combinatedVector = currentPoint[0] * gradients[0] +
                    currentPoint[1] * gradients[1] + currentPoint[2] * gradients[2] +
                    currentPoint[3] * gradients[3] + currentPoint[4] * gradients[4];

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
                    cplexModel.Prod(1.0, w[3]), cplexModel.Prod(1.0, w[4])), 1, "c1");

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

                    if (denominator == 0) break;
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
