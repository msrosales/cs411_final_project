using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models.DatabaseModels
{
    public class CurrencyExchangeRatesArchive
    {
        public string Date { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string TargetCurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }

        public CurrencyExchangeRatesArchive()
        {

        }
    }
}
