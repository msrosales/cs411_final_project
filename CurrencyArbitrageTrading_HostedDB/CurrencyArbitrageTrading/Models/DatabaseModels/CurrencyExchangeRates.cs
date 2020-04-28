using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models.DatabaseModels
{
    public class CurrencyExchangeRates
    {
        public string Date { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string TargetCurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }

        public CurrencyExchangeRates()
        {

        }

        public override string ToString()
        {
            return "1 " + this.BaseCurrencyCode + " = " + this.ExchangeRate.ToString("N10") + " " + this.TargetCurrencyCode;
        }

        public string CurrencyExchangeRateDetails()
        {
            return "Date: " + this.Date
                + "; BaseCurrencyCode: " + this.BaseCurrencyCode
                + "; TargetCurrencyCode: " + this.TargetCurrencyCode
                + "; ExchangeRate: " + this.ExchangeRate.ToString("N10");
        }
    }
}
