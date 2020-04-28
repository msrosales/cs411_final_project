using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models
{
    public class CurrencyArbitrageLog
    {
        public DateTime Date { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string IntermediateCurrencyCodes { get; set; }
        public string TargetCurrencyCode { get; set; }
        public int Degree { get; set; }
        public decimal ImpliedValue { get; set; }
        public decimal ActualValue { get; set; }
    }
}
