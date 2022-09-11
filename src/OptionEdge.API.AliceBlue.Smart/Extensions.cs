using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OptionEdge.API.AliceBlue.Smart
{
    internal static class ExtensionMethods
    {
        public static DateTime PreviousWorkDay(this DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            }
            while (IsWeekend(date));

            return date;
        }

        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).Date;
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).Date.AddMonths(1).AddMilliseconds(-1);
        }

        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static DateTime GetLastSpecificDayOfTheMonth(this DateTime date, DayOfWeek dayofweek)
        {
            var lastDayOfMonth = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

            while (lastDayOfMonth.DayOfWeek != dayofweek)
                lastDayOfMonth = lastDayOfMonth.AddDays(-1);

            return lastDayOfMonth;
        }

        public static int GetWorkingDays(this DateTime from, DateTime to)
        {
            var dayDifference = (int)to.Subtract(from).TotalDays;
            try
            {
                return Enumerable
                    .Range(1, dayDifference)
                    .Select(x => from.AddDays(x))
                    .Count(x => x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday);
            }
            catch
            {
                return 5;
            }
        }

        /// <summary>
        /// WARNING: NOT WORKING PROPERLY
        /// </summary>
        /// <param name="start"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        public static DateTime ClosestWeekDay(this DateTime date, DayOfWeek weekday, bool includeStartDate = true, bool? searchForward = true)
        {
            if (!searchForward.HasValue && !includeStartDate)
            {
                throw new ArgumentException("if searching in both directions, start date must be a valid result");
            }
            var day = date.DayOfWeek;
            int add = ((int)weekday - (int)day);
            if (searchForward.HasValue)
            {
                if (add < 0 && searchForward.Value)
                {
                    add += 7;
                }
                else if (add > 0 && !searchForward.Value)
                {
                    add -= 7;
                }
                else if (add == 0 && !includeStartDate)
                {
                    add = searchForward.Value ? 7 : -7;
                }
            }
            else if (add < -3)
            {
                add += 7;
            }
            else if (add > 3)
            {
                add -= 7;
            }
            return date.AddDays(add);
        }

        public static List<DateTime> GetLastWorkingDays(DateTime date)
        {
            List<DateTime> result = new List<DateTime>();
            date = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
            while (result.Count < 5)
            {
                // > 0 to exclude sunday, < 6 to exclude saturday
                if ((int)date.DayOfWeek > 0 && (int)date.DayOfWeek < 6)
                {
                    result.Add(date);
                }
                date = date.AddDays(-1);
            }
            return result;
        }

        /// <summary>
        ///  use following link to check, last thursday or specific day of the month
        ///  https://stackoverflow.com/questions/2711357/how-to-get-last-friday-of-months-using-net
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static bool IsDateInLastWeekOfTheMonth(this DateTime date)
        {
            return GetLastWorkingDays(date).Contains(date.Date);
        }

        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;

            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-diff).Date;
        }

        public static DateTime LastDayOfWeek(this DateTime dt) =>
        dt.FirstDayOfWeek().AddDays(6);    

        public static string GetNFOptionSymbol(this DateTime expiry, string optionType, int strike, bool forZerodha = false)
        {
            return GetExpirySymbolInternal("NIFTY", expiry, optionType, strike, forZerodha);
        }

        public static string GetBNFOptionSymbol(this DateTime expiry, string optionType, int strike, bool forZerodha = false)
        {
            return GetExpirySymbolInternal("BANKNIFTY", expiry, optionType, strike, forZerodha);
        }

        private const int OCTOBER = 10;
        private const int NOVEMBER = 11;
        private const int DECEMBER = 12;

        public static string GetExpirySymbol(this DateTime expiry, string baseSymbol, string optionType, int strike, bool forZerodha = false)
        {
            baseSymbol = baseSymbol.ToUpper();
            if (baseSymbol == "NIFTY" || baseSymbol == "NIFTY 50")
                return GetNFOptionSymbol(expiry, optionType, strike, forZerodha);
            if (baseSymbol == "BANKNIFTY" || baseSymbol == "NIFTY BANK")
                return GetBNFOptionSymbol(expiry, optionType, strike, forZerodha);
            else
                return "";
        }

        private static string GetExpirySymbolInternal (string baseSymbol, DateTime expiry, string optionType, int strike, bool forZerodha = false)
        {
            var symbol = $"{baseSymbol}{expiry.ToString("yy")}";

            // if expiry days fall within the last week of the months
            if (expiry.Date.IsDateInLastWeekOfTheMonth())
                symbol += expiry.ToString("MMM");
            else
            {
                if (expiry.Date.Month == OCTOBER ||
                expiry.Date.Month == NOVEMBER ||
                expiry.Date.Month == DECEMBER)
                {
                    //symbol += expiry.ToString("MMM").First();
                    if (forZerodha == true)
                        symbol += expiry.ToString("MMM").Substring(0, 1); 
                    else
                        symbol += expiry.ToString("MM");
                }
                else
                    symbol += expiry.ToString("MM").Replace("0", "");

                var dd = expiry.ToString("dd");
                if (dd.StartsWith("0"))
                {
                    if (forZerodha == false)
                        symbol += dd.Replace("0", "");
                    else
                        symbol += dd;
                }
                else
                    symbol += dd;
            }

            symbol += strike + optionType;

            return symbol.ToUpper();
        }
    }
}
