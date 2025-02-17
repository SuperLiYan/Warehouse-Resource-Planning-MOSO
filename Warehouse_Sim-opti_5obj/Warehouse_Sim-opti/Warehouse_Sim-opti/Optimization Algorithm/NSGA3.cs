using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse_Sim_opti.Simulation_Model;
using Warehouse_Sim_opti.Simulation_Modules;

namespace Warehouse_Sim_opti.Optimization_Algorithm
{
    public class Solution
    {
        public double[] Variables;
        public double[] Objectives;
        public List<Solution> DominatedSolutions = new List<Solution>();
        public int DominationCount;
        public int Rank;
        public double CrowdingDistance;

        private static Random rand = new Random();

        public Solution(int numberOfVariables, int numberOfObjectives)
        {
            Variables = new double[numberOfVariables];
            Objectives = new double[numberOfObjectives];
        }

        public void Initialize()
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                Variables[i] = rand.NextDouble();
            }
        }

        public void Mutate(double mutationRate)
        {
            for (int i = 0; i < Variables.Length; i++)
            {
                if (rand.NextDouble() < mutationRate)
                {
                    Variables[i] += (rand.NextDouble() - 0.5) * 0.1;
                    Variables[i] = Math.Max(0, Math.Min(Variables[i], 1)); // Keep within [0,1]
                }
            }
        }

        public bool Dominates(Solution other)
        {
            bool betterInOneObjective = false;
            for (int i = 0; i < Objectives.Length; i++)
            {
                if (Objectives[i] < other.Objectives[i])
                {
                    betterInOneObjective = true;
                }
                else if (Objectives[i] > other.Objectives[i])
                {
                    return false;
                }
            }
            return betterInOneObjective;
        }


        public void EvaluateSimulation(int m, int k)
        {
            int n = Variables.Length; // The total number of decision variables

            Tuple<int, int, int, int, int[][]> inputs = Compile(Variables.ToList<double>());
            int maxManpowerofForklifts = inputs.Item1;
            int maxManpowerofUnpacking = inputs.Item2;
            int maxManpowerofPacking = inputs.Item3;
            int maxForklift = inputs.Item4;
            int[][] manpowerSchedule = inputs.Item5;

            Simulation_System warehouse = new Simulation_System(maxManpowerofForklifts, maxManpowerofUnpacking,
                maxManpowerofPacking, maxForklift, manpowerSchedule);

            double[] values = warehouse.GetKPIs();
            Objectives[0] = values[0];
            Objectives[1] = values[1];
            Objectives[2] = values[2];
            Objectives[3] = values[3];
            Objectives[4] = values[4];
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

                manpowerShiftSchedule[day][0] = (int)Math.Round(decisionvariables[day * 10 + 0] * (New_MGD_SPSA3.StaffofForkliftsEnd - New_MGD_SPSA3.StaffofForkliftsStart), 0) + New_MGD_SPSA3.StaffofForkliftsStart;
                manpowerShiftSchedule[day][1] = (int)Math.Round(decisionvariables[day * 10 + 1] * (New_MGD_SPSA3.StaffofForkliftsEnd - New_MGD_SPSA3.StaffofForkliftsStart), 0) + New_MGD_SPSA3.StaffofForkliftsStart;
                manpowerShiftSchedule[day][2] = (int)Math.Round(decisionvariables[day * 10 + 2] * (New_MGD_SPSA3.StaffofUnpackingEnd - New_MGD_SPSA3.StaffofUnpackingStart), 0) + New_MGD_SPSA3.StaffofUnpackingStart;
                manpowerShiftSchedule[day][3] = (int)Math.Round(decisionvariables[day * 10 + 3] * (New_MGD_SPSA3.StaffofForkliftsEnd - New_MGD_SPSA3.StaffofForkliftsStart), 0) + New_MGD_SPSA3.StaffofForkliftsStart;
                manpowerShiftSchedule[day][4] = (int)Math.Round(decisionvariables[day * 10 + 4] * (New_MGD_SPSA3.StaffofPackingEnd - New_MGD_SPSA3.StaffofPackingStart), 0) + New_MGD_SPSA3.StaffofPackingStart;
                manpowerShiftSchedule[day][5] = manpowerShiftSchedule[day][0];
                manpowerShiftSchedule[day][6] = manpowerShiftSchedule[day][1];
                manpowerShiftSchedule[day][7] = manpowerShiftSchedule[day][3];
                today_manpower_forklift += manpowerShiftSchedule[day][0] + manpowerShiftSchedule[day][1] + manpowerShiftSchedule[day][3];
                today_manpower_unpacking += manpowerShiftSchedule[day][2];
                today_manpower_packing += manpowerShiftSchedule[day][4];
                today_forklift_am += manpowerShiftSchedule[day][5] + manpowerShiftSchedule[day][6] + manpowerShiftSchedule[day][7];

                manpowerShiftSchedule[day][8] = (int)Math.Round(decisionvariables[day * 10 + 5] * (New_MGD_SPSA3.StaffofForkliftsEnd - New_MGD_SPSA3.StaffofForkliftsStart), 0) + New_MGD_SPSA3.StaffofForkliftsStart;
                manpowerShiftSchedule[day][9] = (int)Math.Round(decisionvariables[day * 10 + 6] * (New_MGD_SPSA3.StaffofForkliftsEnd - New_MGD_SPSA3.StaffofForkliftsStart), 0) + New_MGD_SPSA3.StaffofForkliftsStart;
                manpowerShiftSchedule[day][10] = (int)Math.Round(decisionvariables[day * 10 + 7] * (New_MGD_SPSA3.StaffofUnpackingEnd - New_MGD_SPSA3.StaffofUnpackingStart), 0) + New_MGD_SPSA3.StaffofUnpackingStart;
                manpowerShiftSchedule[day][11] = (int)Math.Round(decisionvariables[day * 10 + 8] * (New_MGD_SPSA3.StaffofForkliftsEnd - New_MGD_SPSA3.StaffofForkliftsStart), 0) + New_MGD_SPSA3.StaffofForkliftsStart;
                manpowerShiftSchedule[day][12] = (int)Math.Round(decisionvariables[day * 10 + 9] * (New_MGD_SPSA3.StaffofPackingEnd - New_MGD_SPSA3.StaffofPackingStart), 0) + New_MGD_SPSA3.StaffofPackingStart;
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

        public void Mutate()
        {
            int idx = rand.Next(Variables.Length);
            Variables[idx] += (rand.NextDouble() - 0.5) * 2 * 0.01; // 5% mutation
            Variables[idx] = Math.Max(0, Math.Min(Variables[idx], 1)); // Keep within [0,1]
        }
    }

    public class NSGAII
    {
        private List<Solution> population = new List<Solution>();
        private List<Solution> eliteSolutions = new List<Solution>();
        private int populationSize;
        private int numberOfVariables;
        private int numberOfObjectives;
        private double mutationRate = 0.9;
        private double crossoverRate = 0.02;
        private static Random rand = new Random();

        public NSGAII(int populationSize, int numberOfVariables, int numberOfObjectives)
        {
            this.populationSize = populationSize;
            this.numberOfVariables = numberOfVariables;
            this.numberOfObjectives = numberOfObjectives;
        }

        public void InitializePopulation()
        {
            population = Enumerable.Range(0, populationSize).AsParallel().Select(i =>
            {
                var solution = new Solution(numberOfVariables, numberOfObjectives);
                solution.Initialize();
                solution.EvaluateSimulation(numberOfObjectives, numberOfVariables - numberOfObjectives + 1); // Assume this method is implemented
                return solution;
            }).ToList();
        }

        public void Run(int generations)
        {
            InitializePopulation();

            for (int gen = 0; gen < generations; gen++)
            {
                var offspringPopulation = GenerateOffspring();
                var combinedPopulation = population.Concat(offspringPopulation).ToList();

                var fronts = FastNonDominatedSort(combinedPopulation);
                fronts.ForEach(CalculateCrowdingDistance);

                UpdateEliteSolutions(combinedPopulation);
                population = SelectNextGeneration(fronts);

                Console.WriteLine($"Generation {gen + 1}: Population Size: {population.Count}");
            }

            ExportToCSV("Approximated Pareto Frontier.csv", eliteSolutions);
        }

        private List<Solution> GenerateOffspring()
        {
            return Enumerable.Range(0, populationSize).AsParallel().Select(i =>
            {
                int index1 = rand.Next(populationSize);
                int index2 = rand.Next(populationSize);
                var parent1 = population[index1];
                var parent2 = population[index2];

                if (rand.NextDouble() < crossoverRate)
                {
                    var offspring = Crossover(parent1, parent2);
                    offspring.Mutate(mutationRate);
                    offspring.EvaluateSimulation(numberOfObjectives, numberOfVariables - numberOfObjectives + 1);
                    return offspring;
                }

                return null;
            }).Where(offspring => offspring != null).ToList();
        }

        private Solution Crossover(Solution parent1, Solution parent2)
        {
            Solution offspring = new Solution(numberOfVariables, numberOfObjectives);
            for (int i = 0; i < numberOfVariables; i++)
            {
                offspring.Variables[i] = rand.NextDouble() < 0.5 ? parent1.Variables[i] : parent2.Variables[i];
            }
            return offspring;
        }

        private List<List<Solution>> FastNonDominatedSort(List<Solution> combinedPopulation)
        {
            List<List<Solution>> fronts = new List<List<Solution>>();
            foreach (var p in combinedPopulation)
            {
                p.DominatedSolutions.Clear();
                p.DominationCount = 0;

                foreach (var q in combinedPopulation)
                {
                    if (p != q)
                    {
                        if (p.Dominates(q))
                        {
                            p.DominatedSolutions.Add(q);
                        }
                        else if (q.Dominates(p))
                        {
                            p.DominationCount++;
                        }
                    }
                }

                if (p.DominationCount == 0)
                {
                    p.Rank = 0;
                    if (fronts.Count == 0)
                    {
                        fronts.Add(new List<Solution>());
                    }
                    fronts[0].Add(p);
                }
            }

            int i = 0;
            while (i < fronts.Count)
            {
                List<Solution> nextFront = new List<Solution>();
                foreach (var p in fronts[i])
                {
                    foreach (var q in p.DominatedSolutions)
                    {
                        q.DominationCount--;
                        if (q.DominationCount == 0)
                        {
                            q.Rank = i + 1;
                            nextFront.Add(q);
                        }
                    }
                }
                if (nextFront.Count > 0)
                {
                    fronts.Add(nextFront);
                }
                i++;
            }

            return fronts;
        }

        private void CalculateCrowdingDistance(List<Solution> front)
        {
            int objectivesCount = front[0].Objectives.Length;
            int frontSize = front.Count;

            foreach (var p in front)
            {
                p.CrowdingDistance = 0;
            }

            for (int i = 0; i < objectivesCount; i++)
            {
                front.Sort((p, q) => p.Objectives[i].CompareTo(q.Objectives[i]));
                front.First().CrowdingDistance = front.Last().CrowdingDistance = double.MaxValue;

                if (frontSize > 2 && (front.Last().Objectives[i] - front.First().Objectives[i]) > 0)
                {
                    double norm = front.Last().Objectives[i] - front.First().Objectives[i];
                    for (int j = 1; j < frontSize - 1; j++)
                    {
                        front[j].CrowdingDistance += (front[j + 1].Objectives[i] - front[j - 1].Objectives[i]) / norm;
                    }
                }
            }
        }

        private void UpdateEliteSolutions(List<Solution> currentPopulation)
        {
            var newElites = new List<Solution>();
            foreach (var current in currentPopulation)
            {
                bool isDominated = false;
                var dominatedSolutions = new List<Solution>();

                foreach (var elite in eliteSolutions)
                {
                    if (elite.Dominates(current))
                    {
                        isDominated = true;
                        break;
                    }
                    else if (current.Dominates(elite))
                    {
                        dominatedSolutions.Add(elite);
                    }
                }

                if (!isDominated && !EliteListContains(current))
                {
                    newElites.Add(current);
                }

                dominatedSolutions.ForEach(d => eliteSolutions.Remove(d));
            }

            eliteSolutions.AddRange(newElites);
        }

        private bool EliteListContains(Solution solution)
        {
            return eliteSolutions.Any(e => AreEquivalent(e, solution));
        }

        private bool AreEquivalent(Solution a, Solution b)
        {
            // You may want to consider a tolerance for floating-point comparisons if applicable
            return a.Objectives.SequenceEqual(b.Objectives);
        }


        private List<Solution> SelectNextGeneration(List<List<Solution>> fronts)
        {
            var nextGeneration = new List<Solution>();
            int count = 0;
            foreach (var front in fronts)
            {
                if (count + front.Count <= populationSize)
                {
                    nextGeneration.AddRange(front);
                    count += front.Count;
                }
                else
                {
                    front.Sort((p, q) => q.CrowdingDistance.CompareTo(p.CrowdingDistance));
                    nextGeneration.AddRange(front.Take(populationSize - count));
                    break;
                }
            }

            return nextGeneration;
        }

        private void ExportToCSV(string filename, List<Solution> finalPopulation)
        {
            using (var writer = new StreamWriter(filename))
            {
                // Write the header
                writer.WriteLine("Objective 1,Objective 2,Objective 3");

                // Write the data
                foreach (var solution in finalPopulation)
                {
                    writer.WriteLine($"{solution.Objectives[0]},{solution.Objectives[1]+ solution.Objectives[2]+ solution.Objectives[3]},{solution.Objectives[4]}");
                }
            }
        }
    }
}