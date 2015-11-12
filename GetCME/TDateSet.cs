using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCME
{
    public class TDateSet
    {
        public TDateSet(DateTime startdate, int[] tdates)
        {
            Dates = new List<DateTime>();

            foreach (int i in tdates)
            {
                Dates.Add(startdate.GetTDate(i));
            }
        }
        public TDateSet(string startdate, int[] tdates) :
            this(
                new DateTime(
                    Convert.ToInt32(startdate.Substring(0, 4)),
                    Convert.ToInt32(startdate.Substring(4, 2)),
                    Convert.ToInt32(startdate.Substring(6, 2))
                ),
                tdates
           )
        { }
        public List<DateTime> Dates { get; set; }
        public int Length
        {
            get { return Dates.Count; }
        }     
    }
}
