using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Sim_opti
{
    class ZDT1
    {
        public double[] Values { get; set; }
        List<double> DecisionVariables { get; set; }

        public void CalculateValues()
        {
            double value0 = 0;
            double value1 = 0;
            Values = new double[2];

            value0 = DecisionVariables[0];

            double g = 1.0 + (9.0 * (DecisionVariables.Sum() - DecisionVariables[0])) / (DecisionVariables.Count - 1);

            double h = 1 - Math.Sqrt(value0 / g);

            value1 = g * h;

            Values[0] = value0; Values[1] = value1;
        }

        public ZDT1(List<double> decisionvariables)
        {
            DecisionVariables = decisionvariables;

            CalculateValues();
        }
    }

    class ZDT2
    {
        public double[] Values { get; set; }
        List<double> DecisionVariables { get; set; }

        public void CalculateValues()
        {
            double value0 = 0;
            double value1 = 0;
            Values = new double[2];

            value0 = DecisionVariables[0];

            double g = 1.0 + (9.0 * (DecisionVariables.Sum() - DecisionVariables[0])) / (DecisionVariables.Count - 1);

            double h = 1 - Math.Pow(value0 / g, 2);

            value1 = g * h;

            Values[0] = value0; Values[1] = value1;
        }

        public ZDT2(List<double> decisionvariables)
        {
            DecisionVariables = decisionvariables;

            CalculateValues();
        }
    }

    class ZDT3
    {
        public double[] Values { get; set; }
        List<double> DecisionVariables { get; set; }

        public void CalculateValues()
        {
            double value0 = 0;
            double value1 = 0;
            Values = new double[2];

            value0 = DecisionVariables[0];

            double g = 1.0 + (9.0 * (DecisionVariables.Sum() - DecisionVariables[0])) / (DecisionVariables.Count - 1);

            double h = 1 - Math.Sqrt(value0 / g) - (value0 / g)*Math.Sin(10*Math.PI* value0);
            value1 = h;

            Values[0] = value0; Values[1] = value1;
        }

        public ZDT3(List<double> decisionvariables)
        {
            DecisionVariables = decisionvariables;

            CalculateValues();
        }
    }

    class ZDT4
    {
        public double[] Values { get; set; }
        List<double> DecisionVariables { get; set; }

        public void CalculateValues()
        {
            double value0 = 0;
            double value1 = 0;
            Values = new double[2];

            value0 = DecisionVariables[0];

            double g = 1 + 10 * (DecisionVariables.Count - 1);

            for (int p = 1; p < DecisionVariables.Count; p++)
            {
                g += Math.Pow(DecisionVariables[p], 2) - 10 * Math.Cos(4 * Math.PI * (DecisionVariables[p] * 20 -10));
            }

            double h = 1 - Math.Sqrt(value0 / g);
            value1 = h;

            Values[0] = value0; Values[1] = value1;
        }

        public ZDT4(List<double> decisionvariables)
        {
            DecisionVariables = decisionvariables;

            CalculateValues();
        }
    }
}
