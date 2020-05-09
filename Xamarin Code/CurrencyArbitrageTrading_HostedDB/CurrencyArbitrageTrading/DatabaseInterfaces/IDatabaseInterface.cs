using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using CurrencyArbitrageTrading.Models;
using CurrencyArbitrageTrading.Models.DatabaseModels;

namespace CurrencyArbitrageTrading.DatabaseInterfaces
{
    public interface IDatabaseInterface
    {
        bool ResetSQLiteDatabase(bool paramDeleteExistingDatabase = false, bool paramUseEmbeddedSQLiteDatabase = false);
        List<CurrencyCodes> GetCurrencyCodesFromDatabase();
        List<CurrencyExchangeRates> GetHistoricCurrencyExchangeRatesFromDatabase();
        List<CurrencyExchangeRates> GetHistoricCurrencyExchangeRatesFromDatabase(string paramBaseCurrencyCode, string paramTargetCurrencyCode);
        List<CurrencyExchangeRatesArchive> GetCurrencyExchangeRatesArchiveFromDatabase();
        List<CurrencyArbitrageLog> GetCurrencyArbitrageLogFromDatabase();
        List<CurrencyExchangeRates> GetCurrencyExchangeRatesFromDatabase(string currentDate);
        List<CurrencyExchangeRates> GetLatestCurrencyExchangeRatesFromDatabase();
        bool SaveCurrencyExchangeRatesToDatabase(List<CurrencyExchangeRates> CurrencyExchangeRatesList);
        bool CheckIfTableRefreshLogExists(string paramTableName);
    }
}