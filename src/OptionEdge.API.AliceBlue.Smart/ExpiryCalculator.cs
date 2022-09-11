using System;
using System.Collections.Generic;
using System.Linq;

namespace OptionEdge.API.AliceBlue.Smart
{
    public class ExpiryCalculator 
    {
        List<DateTime> _holidays = new List<DateTime>();

        DateTime _today = DateTime.Now;

        public ExpiryCalculator(DateTime today, List<DateTime> holidays)
        {
            _today = today;
            _holidays =  holidays == null || holidays.Count == 0 ? GetHolidays() : holidays;
        }

        public IList<DateTime> Holidays
        {
            get
            {
                return _holidays;
            }
        }

        private List<DateTime> GetHolidays()
        {
            List<DateTime> holidays = new List<DateTime>();

            // 2021
            // Adding only holiays fallling on thursdays
            holidays.Add(DateTime.Parse("11-Mar-2021"));
            holidays.Add(DateTime.Parse("13-May-2021"));
            holidays.Add(DateTime.Parse("19-Aug-2021"));
            holidays.Add(DateTime.Parse("04-Nov-2021"));
            holidays.Add(DateTime.Parse("05-Nov-2021"));
            holidays.Add(DateTime.Parse("19-Nov-2021"));

            // 2022
            holidays.Add(DateTime.Parse("26-Jan-2022"));
            holidays.Add(DateTime.Parse("01-Mar-2022"));
            holidays.Add(DateTime.Parse("18-Mar-2022"));
            holidays.Add(DateTime.Parse("14-Apr-2022"));

            holidays.Add(DateTime.Parse("15-Apr-2022"));
            holidays.Add(DateTime.Parse("03-May-2022"));
            holidays.Add(DateTime.Parse("09-Aug-2022"));
            holidays.Add(DateTime.Parse("15-Aug-2022"));

            holidays.Add(DateTime.Parse("31-Aug-2022"));
            holidays.Add(DateTime.Parse("05-Oct-2022"));
            holidays.Add(DateTime.Parse("24-Oct-2022"));
            holidays.Add(DateTime.Parse("26-Oct-2022"));
            holidays.Add(DateTime.Parse("08-Nov-2022"));

            return holidays;
        }

        public bool IsHoliday(DateTime date)
        {
            return _holidays.Any(x => x.Date == date.Date);
        }

        private DateTime GetWeeklyExpiry()
        {
            DateTime nextThursday;
            DateTime current = _today;

            var now = DateTime.Now;

            if (current != default(DateTime))
                now = current;

            if (now.DayOfWeek == DayOfWeek.Thursday)
            {
                nextThursday = now;
            }
            else
            {
                nextThursday = now.GetNextWeekday(DayOfWeek.Thursday);
            }

            if (_holidays.Any(x => x.Date == nextThursday.Date))
                nextThursday = nextThursday.AddDays(-1);

            return nextThursday;

        }
        
        private DateTime GetNextWeeklyExpiry(DateTime from)
        {
            DateTime nextThursday;

            if (from.DayOfWeek == DayOfWeek.Wednesday)
            {
                from = from.AddDays(1);
            }

            nextThursday = from.ClosestWeekDay(DayOfWeek.Thursday, false);

            if (_holidays.Any(x => x.Date == nextThursday.Date))
                nextThursday = nextThursday.AddDays(-1);

            var daysToExpiry = DateTime.Now.GetWorkingDays(nextThursday);

            return nextThursday;
        }

        private DateTime GetMonthlyExpiry(DateTime baseDate)
        {           
            var lastThursdayOfThisMonth = baseDate.GetLastSpecificDayOfTheMonth(DayOfWeek.Thursday);

            if (baseDate > lastThursdayOfThisMonth)
            {
                lastThursdayOfThisMonth = baseDate.AddMonths(1).GetLastSpecificDayOfTheMonth(DayOfWeek.Thursday);
            }

            if (_holidays.Any(x => x.Date == lastThursdayOfThisMonth))
                lastThursdayOfThisMonth = lastThursdayOfThisMonth.AddDays(-1);

            var daysToExpiry = baseDate.GetWorkingDays(lastThursdayOfThisMonth);

            return lastThursdayOfThisMonth;
        }

        public DateTime[] GetExpiries()
        {
            List<DateTime> expiries = new List<DateTime>();

            var weeklyExpiryDetail = GetWeeklyExpiry().Date;
            var nextWeeklyExpiryDetail = GetNextWeeklyExpiry(weeklyExpiryDetail).Date;
            var nextToNextWeeklyExpiryDetail = GetNextWeeklyExpiry(nextWeeklyExpiryDetail).Date;
            var monthlyExpiryDetails = GetMonthlyExpiry(_today).Date;
            var nextMonthlyExpiryDetails = GetMonthlyExpiry(DateTime.Now.AddMonths(1).StartOfMonth()).Date;
            var nextToNextMonthlyExpiryDetails = GetMonthlyExpiry(DateTime.Now.AddMonths(2).StartOfMonth()).Date;

            expiries.Add(weeklyExpiryDetail);
            expiries.Add(nextWeeklyExpiryDetail);
            expiries.Add(nextToNextWeeklyExpiryDetail);
            expiries.Add(monthlyExpiryDetails);
            expiries.Add(nextMonthlyExpiryDetails);
            expiries.Add(nextToNextMonthlyExpiryDetails);
            
            return expiries.Distinct().ToArray();
        }

        public DateTime[] GetMonthlies()
        {
            var monthlyExpiryDetails = GetMonthlyExpiry(_today).Date;
            var nextMonthlyExpiryDetails = GetMonthlyExpiry(DateTime.Now.AddMonths(1)).Date;

            List<DateTime> expiries = new List<DateTime>();

            expiries.Add(monthlyExpiryDetails);
            expiries.Add(nextMonthlyExpiryDetails);

            return expiries.Distinct().ToArray();
        }
    }
}
