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

using SQLite;

using CurrencyArbitrageTrading.LibraryMethods;
using CurrencyArbitrageTrading.Models;
using CurrencyArbitrageTrading.Models.DatabaseModels;
using CurrencyArbitrageTrading.Constants;

namespace CurrencyArbitrageTrading.DatabaseInterfaces
{
    public class DatabaseServices : IDatabaseInterface
    {
        public string SQLiteDatabaseConnectionString { get; set; }
        public SQLiteConnection SQLiteDatabaseConnection { get; set; }

        public DatabaseServices()
        {
            this.SQLiteDatabaseConnectionString = GetSQLiteDatabaseConnectionString();
            this.SQLiteDatabaseConnection = GetSQLiteDatabaseConnection();
        }

        public bool ResetSQLiteDatabase(bool paramDeleteExistingDatabase = false, bool paramUseEmbeddedSQLiteDatabase = false)
        {
            //Declare Return Variable
            bool returnValue = false;

            try
            {
                string sqliteFileName = Constants.Constants.SQLiteFileName;
                string documentsDirectoryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string destinationSQLiteDatabasePath = Path.Combine(documentsDirectoryPath, sqliteFileName);

                if (File.Exists(destinationSQLiteDatabasePath) == true && paramDeleteExistingDatabase == true)
                {
                    File.Delete(destinationSQLiteDatabasePath);
                }

                if (paramUseEmbeddedSQLiteDatabase == true)
                {
                    using (BinaryReader binaryReader = new BinaryReader(Android.App.Application.Context.Assets.Open(sqliteFileName)))
                    {
                        using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(destinationSQLiteDatabasePath, FileMode.Create)))
                        {
                            byte[] buffer = new byte[2048];
                            int length = 0;
                            while ((length = binaryReader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                binaryWriter.Write(buffer, 0, length);
                            }
                        }
                    }
                }
                else if (File.Exists(destinationSQLiteDatabasePath) == false)
                {
                    //Create Database & Tables & Populate Initial Dataset & Metadata
                    CreateSQLiteDatabaseTables(destinationSQLiteDatabasePath);
                }

                //Success
                returnValue = true;
            }
            catch (Exception ex)
            {
                //Error
                returnValue = false;

                throw ex;
            }

            //Return
            return returnValue;
        }

        private string GetSQLiteDatabaseConnectionString(bool paramUseEmbeddedSQLiteDatabase = false)
        {
            string sqliteFileName = Constants.Constants.SQLiteFileName;
            string documentsDirectoryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string destinationSQLiteDatabasePath = Path.Combine(documentsDirectoryPath, sqliteFileName);

            if (File.Exists(destinationSQLiteDatabasePath) == false)
            {
                if (paramUseEmbeddedSQLiteDatabase == true)
                {
                    using (BinaryReader binaryReader = new BinaryReader(Android.App.Application.Context.Assets.Open(sqliteFileName)))
                    {
                        using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(destinationSQLiteDatabasePath, FileMode.Create)))
                        {
                            byte[] buffer = new byte[2048];
                            int length = 0;
                            while ((length = binaryReader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                binaryWriter.Write(buffer, 0, length);
                            }
                        }
                    }
                }
                else
                {
                    //Create Database & Tables & Populate Initial Dataset & Metadata
                    CreateSQLiteDatabaseTables(destinationSQLiteDatabasePath);
                }
            }

            return (destinationSQLiteDatabasePath);
        }

        private SQLiteConnection GetSQLiteDatabaseConnection(bool paramUseEmbeddedSQLiteDatabase = false)
        {
            SQLiteConnection sqliteConnection = null;

            try
            {
                string sqliteFileName = Constants.Constants.SQLiteFileName;
                string documentsDirectoryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string destinationSQLiteDatabasePath = Path.Combine(documentsDirectoryPath, sqliteFileName);

                if (File.Exists(destinationSQLiteDatabasePath) == false)
                {
                    if (paramUseEmbeddedSQLiteDatabase == true)
                    {
                        using (BinaryReader binaryReader = new BinaryReader(Android.App.Application.Context.Assets.Open(sqliteFileName)))
                        {
                            using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(destinationSQLiteDatabasePath, FileMode.Create)))
                            {
                                byte[] buffer = new byte[2048];
                                int length = 0;
                                while ((length = binaryReader.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    binaryWriter.Write(buffer, 0, length);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Create Database & Tables & Populate Initial Dataset & Metadata
                        CreateSQLiteDatabaseTables(destinationSQLiteDatabasePath);

                    }
                }

                sqliteConnection = new SQLiteConnection(destinationSQLiteDatabasePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return (sqliteConnection);
        }

        private bool CreateSQLiteDatabaseTables(string destinationSQLiteDatabasePath)
        {
            //Declare Return Variable
            bool returnValue = false;
            try
            {
                //Create SQLite Database Connection
                using (SQLiteConnection sqliteConnection = new SQLiteConnection(destinationSQLiteDatabasePath))
                {
                    //Create Table CurrencyCodes
                    sqliteConnection.Execute("CREATE TABLE IF NOT EXISTS CurrencyCodes (CurrencyCode TEXT NOT NULL, CurrencyDescription TEXT NOT NULL, PRIMARY KEY(CurrencyCode));");

                    //Create Table CurrencyExchangeRates
                    sqliteConnection.Execute("CREATE TABLE IF NOT EXISTS CurrencyExchangeRates (Date TEXT NOT NULL, BaseCurrencyCode TEXT NOT NULL, TargetCurrencyCode TEXT NOT NULL, ExchangeRate NUMERIC NOT NULL, FOREIGN KEY(TargetCurrencyCode) REFERENCES CurrencyCodes(CurrencyCode), FOREIGN KEY(BaseCurrencyCode) REFERENCES CurrencyCodes(CurrencyCode), PRIMARY KEY (Date,BaseCurrencyCode,TargetCurrencyCode));");

                    //Create Table CurrencyExchangeRatesArchive
                    sqliteConnection.Execute("CREATE TABLE IF NOT EXISTS CurrencyExchangeRatesArchive (Date TEXT NOT NULL, BaseCurrencyCode TEXT NOT NULL, TargetCurrencyCode TEXT NOT NULL, ExchangeRate NUMERIC NOT NULL, FOREIGN KEY(TargetCurrencyCode) REFERENCES CurrencyCodes(CurrencyCode), FOREIGN KEY(BaseCurrencyCode) REFERENCES CurrencyCodes(CurrencyCode), PRIMARY KEY (Date,BaseCurrencyCode,TargetCurrencyCode));");

                    //Create Table CurrencyArbitrageLog
                    sqliteConnection.Execute("CREATE TABLE IF NOT EXISTS CurrencyArbitrageLog (Date TEXT NOT NULL, BaseCurrencyCode TEXT NOT NULL, IntermediateCurrencyCodes TEXT NOT NULL, TargetCurrencyCode TEXT NOT NULL, Degree INTEGER NOT NULL, ImpliedValue NUMERIC NOT NULL, ActualValue NUMERIC NOT NULL, PRIMARY KEY (Date,BaseCurrencyCode,IntermediateCurrencyCodes,TargetCurrencyCode,Degree), FOREIGN KEY(TargetCurrencyCode) REFERENCES CurrencyCodes(CurrencyCode), FOREIGN KEY (BaseCurrencyCode) REFERENCES CurrencyCodes(CurrencyCode));");

                    //Create Table DataRefreshLog
                    sqliteConnection.Execute("CREATE TABLE IF NOT EXISTS TableRefreshLog (TableName TEXT NOT NULL, RefreshDate TEXT NOT NULL, PRIMARY KEY(TableName, RefreshDate));");

                    //Populate CurrencyCodes Metadata
                    List<CurrencyCodes> PreferredCurrencyCodesList = new List<CurrencyCodes>();
                    PreferredCurrencyCodesList.AddRange(Constants.Constants.CurrencyKeyValuePairs
                                                                            .Select(type => new CurrencyCodes()
                                                                            {
                                                                                CurrencyCode = type.Key,
                                                                                CurrencyDescription = type.Value
                                                                            })
                                                                            .ToList());

                    sqliteConnection.Execute("DELETE FROM CurrencyCodes");

                    sqliteConnection.InsertAll(objects: PreferredCurrencyCodesList, runInTransaction: false);

                    //Populate TableRefreshLog Table - For CurrencyCodes Table
                    TableRefreshLog tableRefreshLog = new TableRefreshLog()
                    {
                        TableName = Constants.Constants.CurrencyCodesTable,
                        RefreshDate = DateTime.Now.ToString(Constants.Constants.DateFormat)
                    };

                    sqliteConnection.Execute("DELETE FROM TableRefreshLog WHERE TableName = \"" + Constants.Constants.CurrencyCodesTable  + "\" AND RefreshDate = \"" + DateTime.Now.ToString(Constants.Constants.DateFormat)  + "\"");

                    sqliteConnection.Insert(tableRefreshLog);
                }
            }
            catch (Exception ex)
            {
                //Error
                returnValue = false;

                throw ex;
            }

            //Return
            return returnValue;
        }

        #region Select Records From Tables

        public List<CurrencyCodes> GetCurrencyCodesFromDatabase()
        {
            //Declare Return Variable
            List<CurrencyCodes> currencyCodes = new List<CurrencyCodes>();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
            {
                currencyCodes = sqliteConnection.Query<CurrencyCodes>("SELECT CurrencyCode, CurrencyDescription FROM CurrencyCodes");
            }

            return currencyCodes;
        }

        public List<CurrencyExchangeRates> GetHistoricCurrencyExchangeRatesFromDatabase()
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
            {
                CurrencyExchangeRatesDBList = sqliteConnection.Query<CurrencyExchangeRates>("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates");
            }

            return CurrencyExchangeRatesDBList;
        }

        public List<CurrencyExchangeRates> GetHistoricCurrencyExchangeRatesFromDatabase(string paramBaseCurrencyCode, string paramTargetCurrencyCode)
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
            {
                CurrencyExchangeRatesDBList = sqliteConnection.Query<CurrencyExchangeRates>("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates WHERE BaseCurrencyCode = \"" + paramBaseCurrencyCode + "\" AND TargetCurrencyCode = \"" + paramTargetCurrencyCode + "\" ORDER BY DATE(Date) LIMIT " + Constants.Constants.HistoryDays.ToString());
            }

            return CurrencyExchangeRatesDBList;
        }

        public List<CurrencyExchangeRatesArchive> GetCurrencyExchangeRatesArchiveFromDatabase()
        {
            //Declare Return Variable
            List<CurrencyExchangeRatesArchive> currencyExchangeRatesArchive = new List<CurrencyExchangeRatesArchive>();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
            {
                currencyExchangeRatesArchive = sqliteConnection.Query<CurrencyExchangeRatesArchive>("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRatesArchive");
            }

            return currencyExchangeRatesArchive;
        }

        public List<CurrencyArbitrageLog> GetCurrencyArbitrageLogFromDatabase()
        {
            //Declare Return Variable
            List<CurrencyArbitrageLog> currencyArbitrageLogs = new List<CurrencyArbitrageLog>();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
            {
                currencyArbitrageLogs = sqliteConnection.Query<CurrencyArbitrageLog>("SELECT Date, BaseCurrencyCode, IntermediateCurrencyCodes, TargetCurrencyCode, Degree, ImpliedValue, ActualValue FROM CurrencyArbitrageLog");
            }

            return currencyArbitrageLogs;
        }

        public List<CurrencyExchangeRates> GetCurrencyExchangeRatesFromDatabase(string currentDate)
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
            {
                CurrencyExchangeRatesDBList = sqliteConnection.Query<CurrencyExchangeRates>("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates WHERE Date = \"" + currentDate + "\"");
            }

            return CurrencyExchangeRatesDBList;
        }

        public List<CurrencyExchangeRates> GetLatestCurrencyExchangeRatesFromDatabase()
        {
            //Declare Return Variable
            List<CurrencyExchangeRates> CurrencyExchangeRatesDBList = new List<CurrencyExchangeRates>();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
            {
                var currentDate = sqliteConnection.Query<DateValueString>("SELECT Date FROM CurrencyExchangeRates ORDER BY DATE(Date) DESC LIMIT 1");
                var currentDateString = currentDate.FirstOrDefault().Date;

                CurrencyExchangeRatesDBList = sqliteConnection.Query<CurrencyExchangeRates>("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates WHERE Date = \"" + currentDateString + "\"");
            }

            return CurrencyExchangeRatesDBList;
        }

        #endregion Select Records From Tables

        #region Insert Records InTo Tables

        public bool SaveCurrencyExchangeRatesToDatabase(List<CurrencyExchangeRates> CurrencyExchangeRatesList)
        {
            //Declare Return Variable
            bool returnValue = false;

            try
            {
                if (CurrencyExchangeRatesList != null && CurrencyExchangeRatesList.Count() > 0)
                {
                    using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
                    {
                        sqliteConnection.Execute("DELETE FROM CurrencyExchangeRates");

                        sqliteConnection.InsertAll(objects: CurrencyExchangeRatesList, runInTransaction: false);

                        //Populate TableRefreshLog Table - For CurrencyExchangeRates Table
                        TableRefreshLog tableRefreshLog = new TableRefreshLog()
                        {
                            TableName = Constants.Constants.CurrencyExchangeRatesTable,
                            RefreshDate = DateTime.Now.ToString(Constants.Constants.DateFormat)
                        };

                        sqliteConnection.Execute("DELETE FROM TableRefreshLog WHERE TableName = \"" + Constants.Constants.CurrencyExchangeRatesTable + "\" AND RefreshDate = \"" + DateTime.Now.ToString(Constants.Constants.DateFormat) + "\"");

                        sqliteConnection.Insert(tableRefreshLog);

                        //Success
                        returnValue = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Error
                returnValue = false;

                throw ex;
            }

            //Return
            return returnValue;
        }

        public bool CheckIfTableRefreshLogExists(string paramTableName)
        {
            //Declare Return Variable
            bool returnValue = false;
            int recordCount = 0;
            string currentDate = DateTime.Now.Date.ToString(Constants.Constants.DateFormat);

            try
            {
                using (SQLiteConnection sqliteConnection = new SQLiteConnection(GetSQLiteDatabaseConnectionString()))
                {
                    recordCount = sqliteConnection.ExecuteScalar<int>("SELECT COUNT(1) FROM TableRefreshLog WHERE TableName = \"" + paramTableName + "\" AND RefreshDate = \"" + currentDate + "\"");
                    if (recordCount > 0)
                    {
                        //Success
                        returnValue = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Error
                returnValue = false;

                throw ex;
            }

            //Return
            return returnValue;
        }

        #endregion Insert Records InTo Tables
    }

    public class DateValueString
    {
        public string Date { get; set; }
    }
}