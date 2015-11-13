using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetCME
{
    public static class ExtensionMethods
    {
        public static DateTime EndOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
        }

        public static DateTime GetTDate(this DateTime basedate, params object[] offset)
        {
            // initially thought of adding a set number of days to each T date to get T01 etc
            // But - days to add must guarantee going into the next intended month but not beyond
            // edge cases are (1) Jan 31st non-leap year, and (2) 1st of any month.
            // In fact, no fixed number of days to add will satisfy both of these edge cases.
            // Add 28 days to 1st of any month in a leap year and you will remain in the same month, 
            // so you need to add at least 31 to be sure of getting into the next month. But add 
            // 29/30/31 days to Jan 31st in a non leap year and you will go straight over Feb and into March

            // So, we have to factor the date into components in order to progress to end of incremented month

            int monthIncrement = 0;
            // check we have been passed an int
            try
            {
                if (offset.Length > 1)
                {
                    throw new ArgumentException("single integer value expected");
                }
                monthIncrement = Convert.ToInt32(offset[0]);
            }
            catch(Exception ex)
            {
                throw new Exception("Could not interpret argument as an integer offset", ex);
            }
            DateTime start = basedate;
            DateTime startIncremented = start.AddMonths(monthIncrement);
            if (monthIncrement == 0)
            {
                // basedate
                return startIncremented;
            }
            else
            {
                // adjust for end-of-month
                int day = DateTime.DaysInMonth(startIncremented.Year, startIncremented.Month);
                return new DateTime(startIncremented.Year, startIncremented.Month, day);
            }
        }

        public static DateTime Decrement(this DateTime value)
        {
            return value.AddDays(-1d);
        }
    }
}
