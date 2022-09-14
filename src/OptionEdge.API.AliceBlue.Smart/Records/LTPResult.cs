using System;
using System.Collections.Generic;
using System.Text;

namespace OptionEdge.API.AliceBlue.Smart.Records
{
    public class LTPResult
    {
        public int InstrumentToken { get; set; }
        public decimal LastTradedPrice { get; set; }
        public decimal BuyPrice1 { get; set; }
        public decimal SellPrice1 { get; set; }
        public decimal BuyQty1 { get; set; }
        public decimal SellQty1 { get; set; }
    }
}
