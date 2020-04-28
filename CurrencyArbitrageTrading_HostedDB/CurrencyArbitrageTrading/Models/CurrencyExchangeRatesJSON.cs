using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models
{
    public class CurrencyExchangeRatesCustomJSON
    {
        public DateTime Date { get; set; }
        public string BaseCurrencyCode { get; set; }
        public string TargetCurrencyCode { get; set; }
        public Decimal ExchangeRate { get; set; }
    }

    public class CurrencyExchangeRatesJSON
    {
        public Dictionary<string, decimal> Rates { get; set; }
        public string Base { get; set; }
        public DateTime? Date { get; set; }
    }

    public class HistoricCurrencyExchangeRatesJSON
    {
        public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; set; }
        public string Start_At { get; set; }
        public string Base { get; set; }
        public string End_At { get; set; }
    }
}