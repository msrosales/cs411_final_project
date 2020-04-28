using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models
{
    public class CurrencyCodes
    {
        public string CurrencyCode { get; set; }
        public string CurrencyDescription { get; set; }

        public override string ToString()
        {
            return this.CurrencyDescription + " (" + this.CurrencyCode + ")";
        }
    }
}
