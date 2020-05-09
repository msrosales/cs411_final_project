using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Constants
{
    public class Constants
    {
        public const string SQLiteFileName = "CurrencyArbitrageTrading.db3";
        public const bool UseEmbeddedSQLiteDatabase = false;

        public const string Begin_Date = "BEGIN_DATE";
        public const string End_Date = "END_DATE";
        public const string DateFormat = "yyyy-MM-dd";
        public const int HistoryDays = 30;
        public const string BaseCurrency = "ABC";
        public const string TargetCurrency = "XYZ";
        public const string URL_LatestExchangeRatesAPI_Base_To_Target = "https://api.exchangeratesapi.io/latest?base=ABC&symbols=XYZ";
        public const string URL_LatestExchangeRatesAPI_Base = "https://api.exchangeratesapi.io/latest?base=ABC";
        public const string URL_HistoricExchangeRatesAPI_Base = "https://api.exchangeratesapi.io/history?start_at=BEGIN_DATE&end_at=END_DATE&base=ABC&symbols=XYZ";

        public const string CurrencyCodesTable = "CurrencyCodes";
        public const string CurrencyExchangeRatesTable = "CurrencyExchangeRates";
        public const string CurrencyExchangeRatesArchiveTable = "CurrencyExchangeRatesArchive";
        public const string CurrencyArbitrageLogTable = "CurrencyArbitrageLog";
        public const string TableRefreshLogTable = "TableRefreshLog";
        public const int HighlightProfitableExchanges = 3;

        public static readonly Dictionary<string, string> CurrencyKeyValuePairs = new Dictionary<string, string>
        {
            { "AUD", "Australian Dollar" },
            { "CAD", "Canadian Dollar" },
            { "CHF", "Switzerland Franc"},
            { "CNY", "Chinese Yuan Renminbi" },
            { "EUR", "Euro" },
            { "GBP", "British Pound" },
            { "HKD", "Hong Kong Dollar" },
            { "INR", "Indian Rupee" },
            { "JPY", "Japanese Yen" },
            { "NZD",  "New Zealand Dollar" },
            { "RUB", "Russia Ruble" },
            { "SGD", "Singapore Dollar" },
            { "USD", "United States Dollar" }
        };
    }
}