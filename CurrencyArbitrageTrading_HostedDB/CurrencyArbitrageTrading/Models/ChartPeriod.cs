using System;

namespace CurrencyArbitrageTrading.Models
{
    public class ChartPeriod
    {
        public int Key { get; set; }
        public string Value { get; set; }

        public ChartPeriod()
        {

        }

        public override string ToString()
        {
            return "Exchange Rate Trend - " + this.Value;
        }
    }
}