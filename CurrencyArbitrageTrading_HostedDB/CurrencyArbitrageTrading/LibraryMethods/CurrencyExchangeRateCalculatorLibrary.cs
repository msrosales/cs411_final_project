using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;

using Newtonsoft.Json;

using CurrencyArbitrageTrading.Constants;
using CurrencyArbitrageTrading.DatabaseInterfaces;
using CurrencyArbitrageTrading.Models;
using CurrencyArbitrageTrading.Models.DatabaseModels;

namespace CurrencyArbitrageTrading.LibraryMethods
{
    public static class CurrencyExchangeRateCalculatorLibrary
    {
        public static DatabaseServices DatabaseServices { get; set; }
        public static List<CurrencyExchangeRates> LatestCurrencyExchangeRatesList { get; set; }
        public static List<CurrencyExchangeRates> HistoricCurrencyExchangeRatesList { get; set; }

        public static bool LoadFromWebAPI()
        {
            bool returnValue = false;
            try
            {
                LatestCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();
                LatestCurrencyExchangeRatesList = GetLatestCurrencyExchangeRatesFromWebAPI();

                HistoricCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();
                HistoricCurrencyExchangeRatesList = GetHistoricCurrencyExchangeRatesFromWebAPI();

                //Success
                returnValue = true;
            }
            catch (Exception ex)
            {
                //Error
                returnValue = false;
                throw ex;
            }

            return returnValue;
        }

        public static bool LoadFromDatabase()
        {
            bool returnValue = false;
            try
            {
                //Initialize DatabaseServices
                DatabaseServices = new DatabaseServices();

                LatestCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();
                LatestCurrencyExchangeRatesList = DatabaseServices.GetLatestCurrencyExchangeRatesFromDatabase();

                HistoricCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();
                HistoricCurrencyExchangeRatesList = DatabaseServices.GetHistoricCurrencyExchangeRatesFromDatabase();

                //Success
                returnValue = true;
            }
            catch (Exception ex)
            {
                //Error
                returnValue = false;
                throw ex;
            }

            return returnValue;
        }

        public static List<CurrencyExchangeRates> GetLatestCurrencyExchangeRatesFromWebAPI()
        {
            List<string> CurrencyCodesStringList = Constants.Constants.CurrencyKeyValuePairs.Select(type => type.Key).ToList();
            List<CurrencyExchangeRates> CurrencyExchangeRatesList = new List<CurrencyExchangeRates>();
            List<CurrencyExchangeRates> CurrencyExchangeRatesList_Distinct = new List<CurrencyExchangeRates>();

            try
            {
                #region Get Latest Exchange Rates

                foreach (var eachBaseCurrencyCode in CurrencyCodesStringList) //Loop Through Every Base Currency That We Are Interested In
                {
                    string URL_LatestExchangeRatesAPI_Base = Constants.Constants.URL_LatestExchangeRatesAPI_Base
                                                                                .Replace(Constants.Constants.BaseCurrency, eachBaseCurrencyCode);

                    using (WebClient webClient = new WebClient())
                    {
                        string jsonResponse = webClient.DownloadString(URL_LatestExchangeRatesAPI_Base); //Get JSON Response String

                        if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                        {
                            //Convert JSON Response String To Object
                            CurrencyExchangeRatesJSON jsonResponseObject = JsonConvert.DeserializeObject<CurrencyExchangeRatesJSON>(jsonResponse);

                            if (jsonResponseObject != null && jsonResponseObject.Date.HasValue && jsonResponseObject.Rates.Count() > 0)
                            {
                                foreach (var eachRate in jsonResponseObject.Rates) //For Every Target Currency Exchange - Create And Store CurrencyExchangeRates Object
                                {
                                    if (CurrencyCodesStringList.Contains(eachRate.Key)) //Store Only The Exchange Rates Of Target Currencies That We Are Interested In
                                    {
                                        CurrencyExchangeRatesList.Add(new CurrencyExchangeRates()
                                        {
                                            Date = jsonResponseObject.Date.Value.Date.ToString(Constants.Constants.DateFormat),
                                            BaseCurrencyCode = jsonResponseObject.Base,
                                            TargetCurrencyCode = eachRate.Key,
                                            ExchangeRate = eachRate.Value
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion Get Latest Exchange Rates

                #region Remove Duplicates

                foreach (var eachCurrencyExchangeRates in CurrencyExchangeRatesList)
                {
                    if (CurrencyExchangeRatesList_Distinct
                            .Where(type => type.Date == eachCurrencyExchangeRates.Date
                                        && type.BaseCurrencyCode == eachCurrencyExchangeRates.BaseCurrencyCode
                                        && type.TargetCurrencyCode == eachCurrencyExchangeRates.TargetCurrencyCode)
                            .Any() == false)
                    {
                        CurrencyExchangeRatesList_Distinct.Add(eachCurrencyExchangeRates);
                    }
                }

                #endregion Remove Duplicates
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrencyExchangeRatesList_Distinct;
        }

        public static CurrencyExchangeRates GetLatestCurrencyExchangeRatesFromWebAPI(string BaseCurrencyCode, string TargetCurrencyCode)
        {

            CurrencyExchangeRates CurrencyExchangeRates = new CurrencyExchangeRates();

            try
            {
                #region Get Latest Exchange Rates

                string URL_LatestExchangeRatesAPI_Base_To_Target = Constants.Constants.URL_LatestExchangeRatesAPI_Base_To_Target
                                                                            .Replace(Constants.Constants.BaseCurrency, BaseCurrencyCode)
                                                                            .Replace(Constants.Constants.TargetCurrency, TargetCurrencyCode);

                using (WebClient webClient = new WebClient())
                {
                    string jsonResponse = webClient.DownloadString(URL_LatestExchangeRatesAPI_Base_To_Target); //Get JSON Response String

                    if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                    {
                        //Convert JSON Response String To Object
                        CurrencyExchangeRatesJSON jsonResponseObject = JsonConvert.DeserializeObject<CurrencyExchangeRatesJSON>(jsonResponse);

                        if (jsonResponseObject != null && jsonResponseObject.Date.HasValue && jsonResponseObject.Rates.Count() > 0)
                        {
                            foreach (var eachRate in jsonResponseObject.Rates) //For Every Target Currency Exchange - Create And Store CurrencyExchangeRates Object
                            {
                                CurrencyExchangeRates = new CurrencyExchangeRates()
                                {
                                    Date = jsonResponseObject.Date.Value.Date.ToString(Constants.Constants.DateFormat),
                                    BaseCurrencyCode = jsonResponseObject.Base,
                                    TargetCurrencyCode = eachRate.Key,
                                    ExchangeRate = eachRate.Value
                                };
                            }
                        }
                    }
                }

                #endregion Get Latest Exchange Rates
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrencyExchangeRates;
        }

        public static List<CurrencyExchangeRates> GetHistoricCurrencyExchangeRatesFromWebAPI()
        {
            List<string> CurrencyCodesStringList = Constants.Constants.CurrencyKeyValuePairs.Select(type => type.Key).ToList();
            List<CurrencyExchangeRates> CurrencyExchangeRatesList = new List<CurrencyExchangeRates>();
            List<CurrencyExchangeRates> CurrencyExchangeRatesList_Distinct = new List<CurrencyExchangeRates>();

            try
            {
                #region Get Historic Exchange Rates

                foreach (var eachBaseCurrencyCode in CurrencyCodesStringList) //Loop Through Every Base Currency That We Are Interested In
                {
                    string Begin_Date = DateTime.Now.AddDays(-1 * Constants.Constants.HistoryDays).ToString(Constants.Constants.DateFormat);
                    string End_Date = DateTime.Now.ToString(Constants.Constants.DateFormat);

                    string commaSeparatedCurrencyCodesString = String.Join(",", CurrencyCodesStringList.Where(currencyCode => currencyCode != eachBaseCurrencyCode).ToList());

                    string URL_HistoricExchangeRatesAPI_Base = Constants.Constants.URL_HistoricExchangeRatesAPI_Base
                                                                                .Replace(Constants.Constants.BaseCurrency, eachBaseCurrencyCode)
                                                                                .Replace(Constants.Constants.Begin_Date, Begin_Date)
                                                                                .Replace(Constants.Constants.End_Date, End_Date)
                                                                                .Replace(Constants.Constants.TargetCurrency, commaSeparatedCurrencyCodesString);

                    using (WebClient webClient = new WebClient())
                    {
                        string jsonResponse = webClient.DownloadString(URL_HistoricExchangeRatesAPI_Base); //Get JSON Response String

                        if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                        {
                            //Convert JSON Response String To Object
                            HistoricCurrencyExchangeRatesJSON jsonResponseObject = JsonConvert.DeserializeObject<HistoricCurrencyExchangeRatesJSON>(jsonResponse);

                            if (jsonResponseObject != null && jsonResponseObject.Rates.Count() > 0)
                            {
                                foreach (var eachRate in jsonResponseObject.Rates) //For Every Target Currency Exchange - Create And Store CurrencyExchangeRates Object
                                {
                                    foreach (var eachRateValue in eachRate.Value) //For Every Currency Exchange Value
                                    {
                                        CurrencyExchangeRates CurrencyExchangeRates = new CurrencyExchangeRates()
                                        {
                                            Date = eachRate.Key.ToString(Constants.Constants.DateFormat),
                                            BaseCurrencyCode = jsonResponseObject.Base,
                                            TargetCurrencyCode = eachRateValue.Key,
                                            ExchangeRate = eachRateValue.Value
                                        };

                                        CurrencyExchangeRatesList.Add(CurrencyExchangeRates);
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion Get Historic Exchange Rates

                #region Remove Duplicates

                foreach (var eachCurrencyExchangeRates in CurrencyExchangeRatesList)
                {
                    if (CurrencyExchangeRatesList_Distinct
                            .Where(type => type.Date == eachCurrencyExchangeRates.Date
                                        && type.BaseCurrencyCode == eachCurrencyExchangeRates.BaseCurrencyCode
                                        && type.TargetCurrencyCode == eachCurrencyExchangeRates.TargetCurrencyCode)
                            .Any() == false)
                    {
                        CurrencyExchangeRatesList_Distinct.Add(eachCurrencyExchangeRates);
                    }
                }

                #endregion Remove Duplicates
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrencyExchangeRatesList_Distinct;
        }

        public static List<T> DataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        public static List<CurrencyArbitrageLog> CalculateCurrencyArbitrage(List<CurrencyExchangeRates> paramCurrencyExchangeRates, string paramDate, string paramBaseCurrencyCode, string paramTargetCurrencyCode, int paramDegree = 1)
        {
            List<CurrencyArbitrageLog> CurrencyArbitrageLogList = new List<CurrencyArbitrageLog>();
            List<CurrencyArbitrageLog> CurrencyArbitrageLogList_Distinct = new List<CurrencyArbitrageLog>();

            try
            {
                #region Calculate Currency Arbitrage Values

                if (paramCurrencyExchangeRates != null && paramCurrencyExchangeRates.Count() > 0)
                {
                    if (paramDegree > 0)
                    {
                        decimal actualValue = paramCurrencyExchangeRates
                                                    .Where(type => type.Date == paramDate
                                                                && type.BaseCurrencyCode == paramBaseCurrencyCode
                                                                && type.TargetCurrencyCode == paramTargetCurrencyCode)
                                                    .Select(type => type.ExchangeRate)
                                                    .FirstOrDefault();

                        if (paramBaseCurrencyCode.Equals(paramTargetCurrencyCode))
                        {
                            actualValue = 1;
                        }

                        paramCurrencyExchangeRates = paramCurrencyExchangeRates.Where(type => type.Date == paramDate).ToList();
                        List<string> distinctIntermediateCurrencyCodes = paramCurrencyExchangeRates
                                                                                .Where(type => type.BaseCurrencyCode.Equals(paramBaseCurrencyCode) == false
                                                                                            && type.BaseCurrencyCode.Equals(paramTargetCurrencyCode) == false)
                                                                                .Select(type => type.BaseCurrencyCode)
                                                                                .Distinct()
                                                                                .OrderBy(type => type)
                                                                                .ToList();

                        List<List<string>> listIntermediateCurrencyCodes = new List<List<string>>();
                        for(int i = 1; i <= paramDegree; i++) //Create The Dynamic List/Array Of CurrencyCodes Based On The Degree Value
                        {
                            listIntermediateCurrencyCodes.Add(distinctIntermediateCurrencyCodes);
                        }

                        //Create Cartesian Product Of CurrencyCodes Present In The Dynamic List/Array Of CurrencyCodes
                        List<string> listIntermediateCurrencyCodesCartesianList = listIntermediateCurrencyCodes
                                                                                    .CartesianProduct()
                                                                                    .Select(tuple => $"{string.Join(" ", tuple)}")
                                                                                    .ToList();

                        foreach(string eachCartesianList in listIntermediateCurrencyCodesCartesianList)
                        {
                            if (eachCartesianList.Split(' ').Count() == eachCartesianList.Split(' ').Distinct().Count()) //Perform Currency Arbitrage Calculation ONLY For Distinct CurrencyCodes In IntermediateCurrencyCodesCartesianList
                            {
                                CurrencyArbitrageLogList.Add(new CurrencyArbitrageLog()
                                {
                                    Date = paramDate,
                                    BaseCurrencyCode = paramBaseCurrencyCode,
                                    TargetCurrencyCode = paramTargetCurrencyCode,
                                    IntermediateCurrencyCodes = eachCartesianList,
                                    Degree = paramDegree,
                                    ImpliedValue = 0,
                                    ActualValue = actualValue
                                });
                            }
                        }

                        //Calculate Implied Value For Each Combination Of IntermediateCurrencyCodes
                        foreach(CurrencyArbitrageLog eachCurrencyArbitrage in CurrencyArbitrageLogList)
                        {
                            string sourceCurrency = eachCurrencyArbitrage.BaseCurrencyCode;
                            decimal impliedValue = 1;

                            foreach(string eachIntermediateCurrencyCode in eachCurrencyArbitrage.IntermediateCurrencyCodes.Split(' ').ToList())
                            {
                                string destinationCurrency = eachIntermediateCurrencyCode;

                                impliedValue = paramCurrencyExchangeRates
                                                    .Where(type => type.Date == paramDate
                                                                && type.BaseCurrencyCode == sourceCurrency
                                                                && type.TargetCurrencyCode == destinationCurrency)
                                                    .Select(type => type.ExchangeRate * impliedValue)
                                                    .FirstOrDefault();

                                sourceCurrency = eachIntermediateCurrencyCode;
                            }

                            impliedValue = paramCurrencyExchangeRates
                                                .Where(type => type.Date == paramDate
                                                            && type.BaseCurrencyCode == sourceCurrency
                                                            && type.TargetCurrencyCode == eachCurrencyArbitrage.TargetCurrencyCode)
                                                .Select(type => type.ExchangeRate * impliedValue)
                                                .FirstOrDefault();

                            eachCurrencyArbitrage.ImpliedValue = impliedValue;
                        }
                    }
                    else //Degree Value Set To Zero (0)
                    {
                        CurrencyArbitrageLog currencyArbitrageLog = paramCurrencyExchangeRates
                                                                        .Where(type => type.Date == paramDate
                                                                                    && type.BaseCurrencyCode == paramBaseCurrencyCode
                                                                                    && type.TargetCurrencyCode == paramTargetCurrencyCode)
                                                                        .Select(type => new CurrencyArbitrageLog()
                                                                        {
                                                                            Date = type.Date,
                                                                            BaseCurrencyCode = type.BaseCurrencyCode,
                                                                            TargetCurrencyCode = type.TargetCurrencyCode,
                                                                            IntermediateCurrencyCodes = null,
                                                                            Degree = 0,
                                                                            ImpliedValue = type.ExchangeRate,
                                                                            ActualValue = type.ExchangeRate
                                                                        })
                                                                        .FirstOrDefault();
                        if (currencyArbitrageLog != null)
                        {
                            CurrencyArbitrageLogList.Add(currencyArbitrageLog);
                        }
                    }
                }

                #endregion Calculate Currency Arbitrage Values

                #region Remove Duplicates & Return ONLY Profitable Currency Arbitrage Values

                foreach (var eachCurrencyArbitrageLog in CurrencyArbitrageLogList.Where(type => type.ImpliedValue > type.ActualValue).ToList())
                {
                    if (CurrencyArbitrageLogList_Distinct
                            .Where(type => type.Date == eachCurrencyArbitrageLog.Date
                                        && type.BaseCurrencyCode == eachCurrencyArbitrageLog.BaseCurrencyCode
                                        && type.IntermediateCurrencyCodes == eachCurrencyArbitrageLog.IntermediateCurrencyCodes
                                        && type.TargetCurrencyCode == eachCurrencyArbitrageLog.TargetCurrencyCode
                                        && type.Degree == eachCurrencyArbitrageLog.Degree
                                        && type.ImpliedValue == eachCurrencyArbitrageLog.ImpliedValue
                                        && type.ActualValue == eachCurrencyArbitrageLog.ActualValue)
                            .Any() == false)
                    {
                        CurrencyArbitrageLogList_Distinct.Add(eachCurrencyArbitrageLog);
                    }
                }

                //Sort / OrderBy The Results Based On The Most Profitable Arbitrage Transactions On The Top
                CurrencyArbitrageLogList_Distinct = CurrencyArbitrageLogList_Distinct.OrderByDescending(type => type.ImpliedValue).ToList();

                #endregion Remove Duplicates & Return ONLY Profitable Currency Arbitrage Values
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrencyArbitrageLogList_Distinct;
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                from acc in accumulator
                from item in sequence
                select acc.Concat(new[] { item }));
        }

        #region Hosted Database Methods

        public static List<CurrencyCodes> GetCurrencyCodesFromDatabase_HostedDB()
        {
            //Declare Return Variable
            List<CurrencyCodes> currencyCodes = new List<CurrencyCodes>();

            //Hosted Database Endpoint
            string endpointURL = "https://cs411arbitrage.web.illinois.edu/test/GetCurrencyCodes";

            using (WebClient webClient = new WebClient())
            {
                string jsonResponse = webClient.DownloadString(endpointURL); //Get JSON Response String

                if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                {
                    //Convert JSON Response String To Object
                    currencyCodes = JsonConvert.DeserializeObject<List<CurrencyCodes>>(jsonResponse);
                }
            }

            return currencyCodes;
        }


        //public static List<CurrencyExchangeRates> GetHistoricCurrencyExchangeRatesFromDatabase_HostedDB()
        //{
        //    //Declare Return Variable
        //    List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

        //    try
        //    {
        //        List<string> CurrencyCodesStringList = Constants.Constants.CurrencyKeyValuePairs.Select(type => type.Key).ToList();

        //        //Hosted Database Endpoint
        //        string endpointURL = "https://cs411arbitrage.web.illinois.edu/test/GetCurrencyExchangeRates";

        //        using (WebClient webClient = new WebClient())
        //        {
        //            string jsonResponse = webClient.DownloadString(endpointURL); //Get JSON Response String

        //            if (string.IsNullOrWhiteSpace(jsonResponse) == false)
        //            {
        //                //Convert JSON Response String To Object
        //                CurrencyExchangeRatesDBList = JsonConvert.DeserializeObject<List<CurrencyExchangeRates>>(jsonResponse);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return CurrencyExchangeRatesDBList;
        //}


        //public static async Task<List<CurrencyExchangeRates>> GetHistoricCurrencyExchangeRatesFromDatabase_HostedDB()
        //{
        //    //Declare Return Variable
        //    List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

        //    try
        //    {
        //        HttpClient _client;
        //        var uri = new Uri(string.Format("https://cs411arbitrage.web.illinois.edu/test/GetCurrencyExchangeRates", string.Empty));
        //        _client = new HttpClient();
        //        var response = await _client.GetStringAsync(uri);
        //        CurrencyExchangeRatesDBList = JsonConvert.DeserializeObject<List<CurrencyExchangeRates>>(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return CurrencyExchangeRatesDBList;
        //}

        public static List<CurrencyExchangeRates> GetCurrencyExchangeRatesFromDatabase_HostedDB()
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    string jsonResponse = webClient.DownloadString("https://cs411arbitrage.web.illinois.edu/test/GetCurrencyExchangeRates"); //Get JSON Response String

                    if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                    {
                        //Convert JSON Response String To Object
                        CurrencyExchangeRatesDBList = JsonConvert.DeserializeObject<List<CurrencyExchangeRates>>(jsonResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return CurrencyExchangeRatesDBList;
        }


        public static List<CurrencyExchangeRates> GetHistoricCurrencyExchangeRatesFromDatabase_HostedDB(string paramBaseCurrencyCode, string paramTargetCurrencyCode)
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            //Hosted Database Endpoint
            string endpointURL = "https://cs411arbitrage.web.illinois.edu/test/SelectHistoricCurrencyExchangeRates?baseCode=" + paramBaseCurrencyCode + "&targetCode=" + paramTargetCurrencyCode;

            using (WebClient webClient = new WebClient())
            {
                string jsonResponse = webClient.DownloadString(endpointURL); //Get JSON Response String

                if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                {
                    //Convert JSON Response String To Object
                    CurrencyExchangeRatesDBList = JsonConvert.DeserializeObject<List<CurrencyExchangeRates>>(jsonResponse);
                }
            }

            return CurrencyExchangeRatesDBList;
        }


        public static List<CurrencyExchangeRatesArchive> GetCurrencyExchangeRatesArchiveFromDatabase_HostedDB()
        {
            //Declare Return Variable
            List<CurrencyExchangeRatesArchive> currencyExchangeRatesArchive = new List<CurrencyExchangeRatesArchive>();

            //Hosted Database Endpoint
            string endpointURL = "https://cs411arbitrage.web.illinois.edu/test/GetHistoricCurrencyExchangeRates";

            using (WebClient webClient = new WebClient())
            {
                string jsonResponse = webClient.DownloadString(endpointURL); //Get JSON Response String

                if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                {
                    //Convert JSON Response String To Object
                    currencyExchangeRatesArchive = JsonConvert.DeserializeObject<List<CurrencyExchangeRatesArchive>>(jsonResponse);
                }
            }

            return currencyExchangeRatesArchive;
        }


        public static List<CurrencyArbitrageLog> GetCurrencyArbitrageLogFromDatabase_HostedDB(string paramBaseCurrencyCode, string paramTargetCurrencyCode, int paramDegree = 1, string paramSortBy = "value")
        {
            //Declare Return Variable
            List<CurrencyArbitrageLog> currencyArbitrageLogs = new List<CurrencyArbitrageLog>();

            //Hosted Database Endpoint
            string endpointURL = "https://cs411arbitrage.web.illinois.edu/test/SelectCurrencyArbitrageLog?baseCode=" + paramBaseCurrencyCode + "&targetCode=" + paramTargetCurrencyCode + "&degree=" + paramDegree.ToString() + "&sortBy=" + paramSortBy;

            using (WebClient webClient = new WebClient())
            {
                string jsonResponse = webClient.DownloadString(endpointURL); //Get JSON Response String

                if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                {
                    //Convert JSON Response String To Object
                    currencyArbitrageLogs = JsonConvert.DeserializeObject<List<CurrencyArbitrageLog>>(jsonResponse);
                }
            }

            return currencyArbitrageLogs;
        }


        public static List<CurrencyExchangeRates> GetCurrencyExchangeRatesFromDatabase_HostedDB(string currentDate)
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            //Hosted Database Endpoint
            string endpointURL = "https://cs411arbitrage.web.illinois.edu/test/SelectCurrencyExchangeRatesByDate?date=" + currentDate;

            using (WebClient webClient = new WebClient())
            {
                string jsonResponse = webClient.DownloadString(endpointURL); //Get JSON Response String

                if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                {
                    //Convert JSON Response String To Object
                    CurrencyExchangeRatesDBList = JsonConvert.DeserializeObject<List<CurrencyExchangeRates>>(jsonResponse);
                }
            }

            return CurrencyExchangeRatesDBList;
        }


        public static List<CurrencyExchangeRates> GetLatestCurrencyExchangeRatesFromDatabase_HostedDB()
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            //Hosted Database Endpoint
            string endpointURL = "https://cs411arbitrage.web.illinois.edu/test/GetLatestCurrencyExchangeRates";

            using (WebClient webClient = new WebClient())
            {
                string jsonResponse = webClient.DownloadString(endpointURL); //Get JSON Response String

                if (string.IsNullOrWhiteSpace(jsonResponse) == false)
                {
                    //Convert JSON Response String To Object
                    CurrencyExchangeRatesDBList = JsonConvert.DeserializeObject<List<CurrencyExchangeRates>>(jsonResponse);
                }
            }

            return CurrencyExchangeRatesDBList;
        }

        #endregion Hosted Database Methods
    }
}