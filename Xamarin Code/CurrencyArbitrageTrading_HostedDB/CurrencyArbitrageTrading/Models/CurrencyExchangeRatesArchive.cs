using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models
{
    public class CurrencyExchangeRatesArchive
    {
        public DateTime Date { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string TargetCurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }

        public override string ToString()
        {
            return this.Date.ToString("yyyy/MM/dd")
                + "; BaseCurrencyCode: " + this.BaseCurrencyCode
                + "; TargetCurrencyCode: " + this.TargetCurrencyCode
                + "; ExchangeRate: " + this.ExchangeRate.ToString("N6");
        }
    }
}
