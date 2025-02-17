using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse_Sim_opti.Simulation_Modules;

namespace Warehouse_Sim_opti.Tool
{
    internal class StringtoFormalDatetime
    {
        public static DateTime StringToDatetime(string DatetimeInString)
        {
            string[] _DatetimeInString = DatetimeInString.Split(" ");
            string[] YYMMDD = _DatetimeInString[0].Split("/");
            string year = YYMMDD[0]; 
            string month = int.Parse(YYMMDD[1]).ToString("00");
            string day = int.Parse(YYMMDD[2]).ToString("00");

            string[] HHMMSS = YYMMDD = _DatetimeInString[1].Split(":");
            string hh = int.Parse(HHMMSS[0]).ToString("00");
            string mm = int.Parse(HHMMSS[1]).ToString("00");

            DateTime DT = DateTime.ParseExact($"{year}/{month}/{day} {hh}:{mm}", "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);

            return DT;
        }
    }
}
