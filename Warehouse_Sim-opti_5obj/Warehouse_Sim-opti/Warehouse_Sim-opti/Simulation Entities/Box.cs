using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse_Sim_opti.Simulation_Entities
{
    internal class Box
    {
        #region Statics
        /// <value>
        /// Product ID.
        /// </value>
        public string SKU { get; set; }
        public double Length { get; set; }
        public double Hight { get; set; }
        public double Width { get; set; }
        public double Weight { get; set; }
        /// <value>
        /// Arrival Time
        /// </value>
        public DateTime TimeIn = new DateTime();
        #endregion

        public Box(string sku, double length, double hight, double width, double weight, DateTime timeIn)
        {
            SKU = sku;
            Length = length;
            Hight = hight;
            Width = width;
            Weight = weight;
            TimeIn = timeIn;
        }
    }
}
