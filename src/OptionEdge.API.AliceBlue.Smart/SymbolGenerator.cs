using System;

namespace OptionEdge.API.AliceBlue.Smart
{
    public class SymbolGenerator 
    {
        private const int OCTOBER = 10;
        private const int NOVEMBER = 11;
        private const int DECEMBER = 12;

        public string GetSymbol(string instrumentSymbol, DateTime expiry, ALICE_BLUE_API_OPTION_TYPE optionType, decimal strike)
        {
            var optionTypeValue = optionType == ALICE_BLUE_API_OPTION_TYPE.PE ? "PE" : "CE";

            var symbol = $"{instrumentSymbol}{expiry.ToString("yy")}";

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
                    symbol += expiry.ToString("MM");
                }
                else
                    symbol += expiry.ToString("MM").Replace("0", "");

                var dd = expiry.ToString("dd");
                if (dd.StartsWith("0"))
                    symbol += dd.Replace("0", "");
                else
                    symbol += dd;
            }

            symbol += strike + optionTypeValue;

            return symbol.ToUpper();
        }

        public decimal GetATMStrike(decimal ltp, int strikeDiff)
        {
            return (int)(ltp % strikeDiff >= (strikeDiff/2) ? (Math.Ceiling(ltp / strikeDiff) * strikeDiff) : (Math.Floor(ltp / strikeDiff) * strikeDiff));
        }
    }
}
