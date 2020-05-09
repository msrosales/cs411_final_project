using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models.DatabaseModels
{
    public class CurrencyCodes
    {
        public string CurrencyCode { get; set; }
        public string CurrencyDescription { get; set; }

        public CurrencyCodes()
        {

        }

        public override string ToString()
        {
            return this.CurrencyDescription + " (" + this.CurrencyCode + ")";
        }
    }
}
