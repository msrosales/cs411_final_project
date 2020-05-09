using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Text;
using Xamarin.Forms;
using Microcharts.Forms;
using CurrencyArbitrageTrading.LibraryMethods;
using CurrencyArbitrageTrading.Models;
using CurrencyArbitrageTrading.Models.DatabaseModels;
using CurrencyArbitrageTrading.Constants;
using CurrencyArbitrageTrading.DatabaseInterfaces;

namespace CurrencyArbitrageTrading.Views
{
    public class CurrencyArbitrageDashboardView : ContentPage
    {
        public DatabaseServices DatabaseServices { get; set; }

        private List<CurrencyExchangeRates> LatestCurrencyExchangeRatesList { get; set; }
        private List<CurrencyExchangeRates> HistoricCurrencyExchangeRatesList { get; set; }
        private List<CurrencyExchangeRatesArchive> ArchiveCurrencyExchangeRatesList { get; set; }
        private List<CurrencyCodes> PreferredCurrencyCodesList { get; set; }
        private List<Microcharts.Entry> microchartEntries { get; set; }
        private List<DegreeKeyValue> DegreeList { get; set; }

        private Image image_CurrencyExchangeImage;
        private Label label_CurrencyExchangeApplication;
        private BoxView boxView_BlackLine_Header;

        private Picker picker_BaseCurrencyPicker;
        private Picker picker_TargetCurrencyPicker;
        private Picker picker_DegreePicker;
        private Button button_CalculateCurrencyArbitrage;
        private Button button_CurrencyExchangeRatesHomePage;

        private Grid gridHeader;
        private Grid gridDetails;
        private ScrollView scrollView_Grid_CurrencyArbitrage;

        private Label label_DisplayMessage;
        private Label label_AdditionalDisplayMessage;
        private BoxView boxView_BlackLine_DisplayMessage;
        private StackLayout stackLayout;
        private ScrollView scrollView_HomePage;

        public CurrencyArbitrageDashboardView()
        {
            try
            {
                Stopwatch stopWatchApplicationLoad = Stopwatch.StartNew();

                this.Title = "Currency Arbitrage Rates Dashboard";

                //Initialize DatabaseServices
                DatabaseServices = new DatabaseServices();

                //Reset SQLite Database
                DatabaseServices.ResetSQLiteDatabase(paramDeleteExistingDatabase: false, paramUseEmbeddedSQLiteDatabase: false);

                //Populate Initial Currency Exchange Rates Dataset //Get Historic & Latest Exchange Rates Data
                InitialCurrencyExchangeDatasetLoad();

                DegreeList = new List<DegreeKeyValue>();
                DegreeList.Add(new DegreeKeyValue() { Key = 2, Value = "Degree 2" });
                DegreeList.Add(new DegreeKeyValue() { Key = 3, Value = "Degree 3" });
                DegreeList.Add(new DegreeKeyValue() { Key = 4, Value = "Degree 4" });
                DegreeList.Add(new DegreeKeyValue() { Key = 5, Value = "Degree 5" });

                this.PreferredCurrencyCodesList = new List<CurrencyCodes>();
                this.PreferredCurrencyCodesList.AddRange(Constants.Constants.CurrencyKeyValuePairs
                                                            .Select(type => new CurrencyCodes()
                                                            {
                                                                CurrencyCode = type.Key,
                                                                CurrencyDescription = type.Value
                                                            })
                                                            .ToList());

                //this.PreferredCurrencyCodesList = new List<CurrencyCodes>();
                //this.PreferredCurrencyCodesList.AddRange(CurrencyExchangeRateCalculatorLibrary.GetCurrencyCodesFromDatabase_HostedDB());

                image_CurrencyExchangeImage = new Image();
                image_CurrencyExchangeImage.Source = "CurrencyExchange_Ver1.png";
                image_CurrencyExchangeImage.IsEnabled = false;

                label_CurrencyExchangeApplication = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "Currency Arbitrage Rates Dashboard",
                    IsEnabled = false,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.Black
                };

                boxView_BlackLine_Header = new BoxView()
                {
                    Color = Color.DarkBlue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = true,
                };

                picker_BaseCurrencyPicker = new Picker();
                picker_BaseCurrencyPicker.Title = "Select Base Currency";
                picker_BaseCurrencyPicker.ItemsSource = this.PreferredCurrencyCodesList;
                picker_BaseCurrencyPicker.SelectedIndexChanged += Picker_BaseCurrencyPicker_SelectedIndexChanged;
                picker_BaseCurrencyPicker.FontAttributes = FontAttributes.Bold;
                picker_BaseCurrencyPicker.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                picker_BaseCurrencyPicker.TextColor = Color.DarkBlue;
                picker_BaseCurrencyPicker.HorizontalOptions = LayoutOptions.FillAndExpand;

                picker_TargetCurrencyPicker = new Picker();
                picker_TargetCurrencyPicker.Title = "Select Target Currency";
                picker_TargetCurrencyPicker.ItemsSource = this.PreferredCurrencyCodesList;
                picker_TargetCurrencyPicker.SelectedIndexChanged += Picker_TargetCurrencyPicker_SelectedIndexChanged;
                picker_TargetCurrencyPicker.FontAttributes = FontAttributes.Bold;
                picker_TargetCurrencyPicker.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                picker_TargetCurrencyPicker.TextColor = Color.DarkBlue;
                picker_TargetCurrencyPicker.HorizontalOptions = LayoutOptions.FillAndExpand;

                picker_DegreePicker = new Picker();
                picker_DegreePicker.Title = "Degree For Currency Arbitrage";
                picker_DegreePicker.ItemsSource = this.DegreeList;
                picker_DegreePicker.SelectedIndex = 0; //Default To Degree 2 (Index 0)
                picker_DegreePicker.SelectedIndexChanged += Picker_DegreePicker_SelectedIndexChanged;
                picker_DegreePicker.FontAttributes = FontAttributes.Bold;
                picker_DegreePicker.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                picker_DegreePicker.TextColor = Color.DarkBlue;
                picker_DegreePicker.HorizontalOptions = LayoutOptions.FillAndExpand;

                button_CalculateCurrencyArbitrage = new Button();
                button_CalculateCurrencyArbitrage.Text = "Calculate Currency Arbitrage";
                button_CalculateCurrencyArbitrage.FontAttributes = FontAttributes.Bold;
                button_CalculateCurrencyArbitrage.Clicked += Button_CalculateCurrencyArbitrage_Clicked;

                button_CurrencyExchangeRatesHomePage = new Button();
                button_CurrencyExchangeRatesHomePage.Text = "Currency Exchange Rates Dashboard";
                button_CurrencyExchangeRatesHomePage.FontAttributes = FontAttributes.Bold;
                button_CurrencyExchangeRatesHomePage.Clicked += Button_CurrencyExchangeRatesHomePage_Clicked;

                gridHeader = new Grid()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    ColumnDefinitions =
                    {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                    }
                };
                gridDetails = new Grid()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    ColumnDefinitions =
                    {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                    }
                };
                scrollView_Grid_CurrencyArbitrage = new ScrollView()
                {
                    Orientation = ScrollOrientation.Both,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Default,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Default,
                    IsVisible = false,
                    IsEnabled = false,
                    Content = gridDetails
                };

                label_DisplayMessage = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "Currency Arbitrage Dashboard",
                    IsEnabled = false,
                    IsVisible = false,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.Black
                };

                label_AdditionalDisplayMessage = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "Currency Arbitrage Additional Details",
                    IsEnabled = false,
                    IsVisible = false,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.Black
                };

                boxView_BlackLine_DisplayMessage = new BoxView()
                {
                    Color = Color.DarkBlue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = false,
                };

                stackLayout = new StackLayout();
                //stackLayout.Children.Add(image_CurrencyExchangeImage);
                //stackLayout.Children.Add(label_CurrencyExchangeApplication);
                //stackLayout.Children.Add(boxView_BlackLine_Header);

                stackLayout.Children.Add(picker_BaseCurrencyPicker);
                stackLayout.Children.Add(picker_TargetCurrencyPicker);
                stackLayout.Children.Add(picker_DegreePicker);

                //stackLayout.Children.Add(button_CalculateCurrencyArbitrage);
                //stackLayout.Children.Add(button_CurrencyExchangeRatesHomePage);
                stackLayout.Children.Add(label_DisplayMessage);
                stackLayout.Children.Add(label_AdditionalDisplayMessage);
                stackLayout.Children.Add(boxView_BlackLine_DisplayMessage);
                stackLayout.Children.Add(gridHeader);
                stackLayout.Children.Add(scrollView_Grid_CurrencyArbitrage);

                scrollView_HomePage = new ScrollView()
                {
                    Orientation = ScrollOrientation.Both,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                    IsVisible = true,
                    Content = stackLayout
                };

                stopWatchApplicationLoad.Stop();
                Content = scrollView_HomePage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Button_CalculateCurrencyArbitrage_Clicked(object sender, EventArgs e)
        {
            //Calculate Currency Arbitrage
            CalculateCurrencyArbitrage(ignoreMissingInputs: false);
        }

        private void Picker_BaseCurrencyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Calculate Currency Arbitrage
            CalculateCurrencyArbitrage(ignoreMissingInputs: true);
        }

        private void Picker_TargetCurrencyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Calculate Currency Arbitrage
            CalculateCurrencyArbitrage(ignoreMissingInputs: true);
        }

        private void Picker_DegreePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Calculate Currency Arbitrage
            CalculateCurrencyArbitrage(ignoreMissingInputs: true);
        }

        private async void Button_CurrencyExchangeRatesHomePage_Clicked(object sender, EventArgs e)
        {
            //Hide Unused Controls
            HideUnusedControls();

            /*
            //Currency Exchange Rates Calculator Home Page
            label_DisplayMessage = new Label()
            {
                LineBreakMode = LineBreakMode.WordWrap,
                Text = "Go To Currency Exchange Rates Home Page",
                IsEnabled = false,
                IsVisible = true,
                FontAttributes = FontAttributes.Bold,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Color.Red
            };
            stackLayout.Children.Add(label_DisplayMessage);

            boxView_BlackLine_DisplayMessage = new BoxView()
            {
                Color = Color.Blue,
                HeightRequest = 3,
                IsEnabled = false,
                IsVisible = true,
            };
            stackLayout.Children.Add(boxView_BlackLine_DisplayMessage);
            */

            //Page Re-direction Alert
            //await DisplayAlert("Currency Exchange Rates Dashboard", "Page Re-direction: You are being re-directed to Currency Exchange Rates Dashboard.", "OK");

            //Navigate Back To Currency Exchange Rates Home Page
            await Navigation.PopAsync();
        }

        private void HideUnusedControls()
        {
            //Hide All Other Display Controls In The Current Form
            stackLayout.Children.Remove(label_DisplayMessage);
            stackLayout.Children.Remove(label_AdditionalDisplayMessage);
            stackLayout.Children.Remove(boxView_BlackLine_DisplayMessage);
            stackLayout.Children.Remove(gridHeader);
            stackLayout.Children.Remove(scrollView_Grid_CurrencyArbitrage);
        }

        private void InitialCurrencyExchangeDatasetLoad()
        {
            try
            {
                //Initialize HistoricCurrencyExchangeRatesList
                this.HistoricCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();
                this.ArchiveCurrencyExchangeRatesList = new List<CurrencyExchangeRatesArchive>();

                try
                {
                    //Get Currency Exchange Rates From Hosted Database
                    this.HistoricCurrencyExchangeRatesList = CurrencyExchangeRateCalculatorLibrary.GetCurrencyExchangeRatesFromDatabase_HostedDB();

                    this.ArchiveCurrencyExchangeRatesList = CurrencyExchangeRateCalculatorLibrary.GetCurrencyExchangeRatesArchiveFromDatabase_HostedDB();
                    if (this.ArchiveCurrencyExchangeRatesList != null && this.ArchiveCurrencyExchangeRatesList.Count() > 0)
                    {
                        foreach (var eachArchiveCurrencyExchangeRates in this.ArchiveCurrencyExchangeRatesList)
                        {
                            if (this.HistoricCurrencyExchangeRatesList.Where(type => type.Date == eachArchiveCurrencyExchangeRates.Date
                                                                                && type.BaseCurrencyCode == eachArchiveCurrencyExchangeRates.BaseCurrencyCode
                                                                                && type.TargetCurrencyCode == eachArchiveCurrencyExchangeRates.TargetCurrencyCode)
                                                                      .Any() == false)
                            {
                                this.HistoricCurrencyExchangeRatesList.Add(new CurrencyExchangeRates()
                                {
                                    Date = eachArchiveCurrencyExchangeRates.Date,
                                    BaseCurrencyCode = eachArchiveCurrencyExchangeRates.BaseCurrencyCode,
                                    TargetCurrencyCode = eachArchiveCurrencyExchangeRates.TargetCurrencyCode,
                                    ExchangeRate = eachArchiveCurrencyExchangeRates.ExchangeRate
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //string ExceptionMessage = string.Empty;
                    //if (string.IsNullOrWhiteSpace(ex.Message) == false)
                    //{
                    //    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                    //}
                    //DisplayAlert("Exception! Currency Exchange Dataset Load", "Error During Currency Exchange Dataset Load." + ExceptionMessage, "OK");
                }

                if (this.HistoricCurrencyExchangeRatesList == null || this.HistoricCurrencyExchangeRatesList.Count == 0)
                {
                    if (DatabaseServices.CheckIfTableRefreshLogExists(paramTableName: Constants.Constants.CurrencyExchangeRatesTable) == false)
                    {
                        //Get Historic Currency Exchange Rates Using WebAPI Call
                        this.HistoricCurrencyExchangeRatesList = CurrencyExchangeRateCalculatorLibrary.GetHistoricCurrencyExchangeRatesFromWebAPI();

                        //Save Historic Exchange Rates To Database                
                        Task.Run(() => DatabaseServices.SaveCurrencyExchangeRatesToDatabase(this.HistoricCurrencyExchangeRatesList));
                    }
                    else
                    {
                        //Get Historic Currency Exchange Rates From SQLite Database
                        this.HistoricCurrencyExchangeRatesList = DatabaseServices.GetHistoricCurrencyExchangeRatesFromDatabase();
                    }
                }

                //Initialize LatestCurrencyExchangeRatesList
                this.LatestCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();

                //Get Latest Currency Exchange Rates From HistoricCurrencyExchangeRatesList
                PopulateLatestFromHistoricExchangeRates();
            }
            catch (Exception ex)
            {
                string ExceptionMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(ex.Message) == false)
                {
                    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                }
                DisplayAlert("Exception! InitialCurrencyExchangeDatasetLoad", "Error During Currency Exchange Dataset Load." + ExceptionMessage, "OK");
            }
        }

        private void PopulateLatestFromHistoricExchangeRates()
        {
            try
            {
                if (this.HistoricCurrencyExchangeRatesList != null && this.HistoricCurrencyExchangeRatesList.Count() > 0)
                {
                    //Get Latest Currency Exchange Rates From HistoricCurrencyExchangeRatesList
                    string latestDate = this.HistoricCurrencyExchangeRatesList.Select(type => DateTime.Parse(type.Date)).Max(type => type.Date).ToString(Constants.Constants.DateFormat);
                    this.LatestCurrencyExchangeRatesList = this.HistoricCurrencyExchangeRatesList.Where(type => type.Date == latestDate).ToList();
                }

                if (this.LatestCurrencyExchangeRatesList == null || this.LatestCurrencyExchangeRatesList.Count() <= 0)
                {
                    try
                    {
                        //Get Latest Currency Exchange Rates From Hosted Database
                        this.LatestCurrencyExchangeRatesList = CurrencyExchangeRateCalculatorLibrary.GetLatestCurrencyExchangeRatesFromDatabase_HostedDB();
                    }
                    catch (Exception ex)
                    {
                        //string ExceptionMessage = string.Empty;
                        //if (string.IsNullOrWhiteSpace(ex.Message) == false)
                        //{
                        //    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                        //}
                        //DisplayAlert("Exception! Currency Exchange Dataset Load", "Error During Currency Exchange Dataset Load." + ExceptionMessage, "OK");
                    }

                    if (this.LatestCurrencyExchangeRatesList == null || this.LatestCurrencyExchangeRatesList.Count() <= 0)
                    {
                        //Get Latest Currency Exchange Rates From WebAPI Call
                        this.LatestCurrencyExchangeRatesList = CurrencyExchangeRateCalculatorLibrary.GetLatestCurrencyExchangeRatesFromWebAPI();
                    }

                    if (this.LatestCurrencyExchangeRatesList == null || this.LatestCurrencyExchangeRatesList.Count() <= 0)
                    {
                        //Get Latest Currency Exchange Rates From Database
                        this.LatestCurrencyExchangeRatesList = DatabaseServices.GetLatestCurrencyExchangeRatesFromDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                string ExceptionMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(ex.Message) == false)
                {
                    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                }
                DisplayAlert("Exception! PopulateLatestFromHistoricExchangeRates", "Error During Latest Currency Exchange Rates Load." + ExceptionMessage, "OK");
            }
        }

        private void CalculateCurrencyArbitrage(bool ignoreMissingInputs = false)
        {
            //Hide Unused Controls
            HideUnusedControls();

            if (picker_BaseCurrencyPicker.SelectedItem != null
                && picker_TargetCurrencyPicker.SelectedItem != null
                && picker_DegreePicker.SelectedItem != null)
            {
                string baseCurrencyCode = ((CurrencyCodes)picker_BaseCurrencyPicker.SelectedItem).CurrencyCode;
                string baseCurrencyDescription = ((CurrencyCodes)picker_BaseCurrencyPicker.SelectedItem).CurrencyDescription;
                string targetCurrencyCode = ((CurrencyCodes)picker_TargetCurrencyPicker.SelectedItem).CurrencyCode;
                string targetCurrencyDescription = ((CurrencyCodes)picker_TargetCurrencyPicker.SelectedItem).CurrencyDescription;
                int degreeKey = ((DegreeKeyValue)picker_DegreePicker.SelectedItem).Key;
                string degreeValue = ((DegreeKeyValue)picker_DegreePicker.SelectedItem).Value;

                string latestDate = this.LatestCurrencyExchangeRatesList.Select(type => DateTime.Parse(type.Date)).Max(type => type.Date).ToString(Constants.Constants.DateFormat);

                List<CurrencyArbitrageLog> CurrencyArbitrageLogList = new List<CurrencyArbitrageLog>();

                try
                {
                    //Get Currency Arbitrage Rates From Hosted Database
                    CurrencyArbitrageLogList = CurrencyExchangeRateCalculatorLibrary.GetCurrencyArbitrageLogFromDatabase_HostedDB
                                                                                        (
                                                                                        paramBaseCurrencyCode: baseCurrencyCode,
                                                                                        paramTargetCurrencyCode: targetCurrencyCode,
                                                                                        paramDegree: degreeKey
                                                                                        );

                    if (CurrencyArbitrageLogList != null && CurrencyArbitrageLogList.Count() > 0)
                    {
                        //Get Latest CurrencyArbitrageLogDate From CurrencyArbitrageLogList
                        string latestCurrencyArbitrageLogDate = CurrencyArbitrageLogList.Select(type => DateTime.Parse(type.Date)).Max(type => type.Date).ToString(Constants.Constants.DateFormat);
                        CurrencyArbitrageLogList = CurrencyArbitrageLogList.Where(type => type.Date == latestCurrencyArbitrageLogDate && type.ActualValue > type.ImpliedValue).OrderByDescending(type => type.ActualValue).ThenByDescending(type => type.ImpliedValue).ToList();
                    }
                }
                catch (Exception ex)
                {
                    //string ExceptionMessage = string.Empty;
                    //if (string.IsNullOrWhiteSpace(ex.Message) == false)
                    //{
                    //    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                    //}
                    //DisplayAlert("Exception! Calculate Currency Arbitrage", "Error While Calculating Currency Arbitrage From Hosted DB." + ExceptionMessage, "OK");
                }

                if (CurrencyArbitrageLogList == null || CurrencyArbitrageLogList.Count() == 0)
                {
                    CurrencyArbitrageLogList = CurrencyExchangeRateCalculatorLibrary.CalculateCurrencyArbitrage
                                                                                        (
                                                                                        paramCurrencyExchangeRates: this.LatestCurrencyExchangeRatesList,
                                                                                        paramDate: latestDate,
                                                                                        paramBaseCurrencyCode: baseCurrencyCode,
                                                                                        paramTargetCurrencyCode: targetCurrencyCode,
                                                                                        paramDegree: degreeKey
                                                                                        );
                }

                if (CurrencyArbitrageLogList != null && CurrencyArbitrageLogList.Count() > 0)
                {
                    string dateValue = this.LatestCurrencyExchangeRatesList.Select(type => type.Date).FirstOrDefault();

                    //Display Label
                    label_DisplayMessage = new Label()
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        Text = "Currency Arbitrage Rates (" + dateValue + ")",
                        IsEnabled = false,
                        IsVisible = true,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.DarkBlue
                    };
                    stackLayout.Children.Add(label_DisplayMessage);

                    label_AdditionalDisplayMessage = new Label()
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        Text = "Base Currency: " + baseCurrencyDescription + Environment.NewLine + "Target Currency: " + targetCurrencyDescription,
                        IsEnabled = false,
                        IsVisible = true,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.DarkBlue
                    };
                    stackLayout.Children.Add(label_AdditionalDisplayMessage);

                    //Box View Line
                    boxView_BlackLine_DisplayMessage = new BoxView()
                    {
                        Color = Color.DarkBlue,
                        HeightRequest = 3,
                        IsEnabled = false,
                        IsVisible = true,
                    };
                    stackLayout.Children.Add(boxView_BlackLine_DisplayMessage);

                    //Grid - Header
                    gridHeader = new Grid()
                    {
                        //VerticalOptions = LayoutOptions.FillAndExpand,
                        ColumnDefinitions =
                        {
                        //new ColumnDefinition { Width = new GridLength (0.22, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength (0.22, GridUnitType.Star) },
                        //new ColumnDefinition { Width = new GridLength (0.22, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength (0.17, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength (0.17, GridUnitType.Star) }
                        }
                    };

                    int rowHeaderNumber = 0;

                    Color labelHeaderBackgroundColor = Color.Orange;

                    Label BaseCurrencyCodeHeaderLabel = new Label { Text = "BaseCurrency", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };
                    Label IntermediateCurrencyCodeHeaderLabel = new Label { Text = "IntermediateCurrency", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };
                    Label TargetCurrencyCodeHeaderLabel = new Label { Text = "TargetCurrency", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };
                    Label ImpliedValueHeaderLabel = new Label { Text = "ImpliedValue", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };
                    Label ActualValueHeaderLabel = new Label { Text = "ActualValue", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };

                    gridHeader.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    //gridHeader.Children.Add(BaseCurrencyCodeHeaderLabel, 0, rowHeaderNumber);
                    //gridHeader.Children.Add(IntermediateCurrencyCodeHeaderLabel, 1, rowHeaderNumber);
                    //gridHeader.Children.Add(TargetCurrencyCodeHeaderLabel, 2, rowHeaderNumber);
                    //gridHeader.Children.Add(ImpliedValueHeaderLabel, 3, rowHeaderNumber);
                    //gridHeader.Children.Add(ActualValueHeaderLabel, 4, rowHeaderNumber);

                    gridHeader.Children.Add(IntermediateCurrencyCodeHeaderLabel, 0, rowHeaderNumber);
                    gridHeader.Children.Add(ImpliedValueHeaderLabel, 1, rowHeaderNumber);
                    gridHeader.Children.Add(ActualValueHeaderLabel, 2, rowHeaderNumber);

                    stackLayout.Children.Add(gridHeader);

                    //Highlight Top Profitable Currency Arbitrage Exchanges
                    int highlightedProfitableExchanges = 0;

                    //Grid - Details
                    gridDetails = new Grid()
                    {
                        //VerticalOptions = LayoutOptions.FillAndExpand,
                        ColumnDefinitions =
                    {
                    //new ColumnDefinition { Width = new GridLength (0.22, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength (0.22, GridUnitType.Star) },
                    //new ColumnDefinition { Width = new GridLength (0.22, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength (0.17, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength (0.17, GridUnitType.Star) }
                    }
                    };
                    int rowDetailNumber = 0;
                    foreach (CurrencyArbitrageLog eachValue in CurrencyArbitrageLogList)
                    {
                        string BaseCurrencyCode = eachValue.BaseCurrencyCode;
                        string IntermediateCurrencyCodes = eachValue.IntermediateCurrencyCodes;
                        string TargetCurrencyCode = eachValue.TargetCurrencyCode;
                        decimal ImpliedValue = eachValue.ImpliedValue;
                        decimal ActualValue = eachValue.ActualValue;

                        string BaseCurrencyDescription = this.PreferredCurrencyCodesList.Where(type => type.CurrencyCode == BaseCurrencyCode).Select(type => type.CurrencyDescription).FirstOrDefault();
                        string TargetCurrencyDescription = this.PreferredCurrencyCodesList.Where(type => type.CurrencyCode == TargetCurrencyCode).Select(type => type.CurrencyDescription).FirstOrDefault();

                        string IntermediateCurrencyDescription = string.Empty;
                        foreach (string eachIntermediateCurrencyCode in eachValue.IntermediateCurrencyCodes.Split(' ').ToList())
                        {
                            if (string.IsNullOrWhiteSpace(IntermediateCurrencyDescription) == false)
                            {
                                IntermediateCurrencyDescription = IntermediateCurrencyDescription + Environment.NewLine;
                            }

                            IntermediateCurrencyDescription = IntermediateCurrencyDescription + this.PreferredCurrencyCodesList.Where(type => type.CurrencyCode == eachIntermediateCurrencyCode).Select(type => type.CurrencyDescription).FirstOrDefault();
                        }

                        Color labelBackgroundColor = Color.Default;
                        if (highlightedProfitableExchanges >= Constants.Constants.HighlightProfitableExchanges)
                        {
                            if (rowDetailNumber % 2 == 0) //For Even & Odd Number Rows - Set Set Different Row Background Color
                            {
                                labelBackgroundColor = Color.LightGray;
                            }
                            else
                            {
                                labelBackgroundColor = Color.LightSkyBlue;
                            }
                        }
                        else
                        {
                            labelBackgroundColor = Color.LightGreen;
                        }
                        highlightedProfitableExchanges = highlightedProfitableExchanges + 1;

                        Label BaseCurrencyCodeLabel = new Label { Text = BaseCurrencyDescription, TextColor = Color.DarkBlue, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                        Label IntermediateCurrencyCodeLabel = new Label { Text = IntermediateCurrencyDescription, TextColor = Color.DarkBlue, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                        Label TargetCurrencyCodeLabel = new Label { Text = TargetCurrencyDescription, TextColor = Color.DarkBlue, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                        Label ImpliedValueLabel = new Label { Text = ImpliedValue.ToString("N12"), TextColor = Color.DarkBlue, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                        Label ActualValueLabel = new Label { Text = ActualValue.ToString("N12"), TextColor = Color.DarkBlue, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };

                        gridDetails.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        //gridDetails.Children.Add(BaseCurrencyCodeLabel, 0, rowDetailNumber);
                        //gridDetails.Children.Add(IntermediateCurrencyCodeLabel, 1, rowDetailNumber);
                        //gridDetails.Children.Add(TargetCurrencyCodeLabel, 2, rowDetailNumber);
                        //gridDetails.Children.Add(ImpliedValueLabel, 3, rowDetailNumber);
                        //gridDetails.Children.Add(ActualValueLabel, 4, rowDetailNumber);

                        gridDetails.Children.Add(IntermediateCurrencyCodeLabel, 0, rowDetailNumber);
                        gridDetails.Children.Add(ImpliedValueLabel, 1, rowDetailNumber);
                        gridDetails.Children.Add(ActualValueLabel, 2, rowDetailNumber);

                        rowDetailNumber++;
                    }

                    //Scroll View
                    scrollView_Grid_CurrencyArbitrage = new ScrollView()
                    {
                        Orientation = ScrollOrientation.Both,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                        IsVisible = true,
                        IsEnabled = true,
                        Content = gridDetails
                    };
                    stackLayout.Children.Add(scrollView_Grid_CurrencyArbitrage);
                }
                else
                {
                    //No Potential Currency Arbitrage Found From Latest ExchangeRates
                    label_DisplayMessage = new Label()
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        Text = "No Potential Currency Arbitrage Found",
                        IsEnabled = false,
                        IsVisible = true,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.Red
                    };
                    stackLayout.Children.Add(label_DisplayMessage);

                    label_AdditionalDisplayMessage = new Label()
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        Text = "Base Currency: " + baseCurrencyDescription + Environment.NewLine + "Target Currency: " + targetCurrencyDescription,
                        IsEnabled = false,
                        IsVisible = true,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.DarkBlue
                    };
                    stackLayout.Children.Add(label_AdditionalDisplayMessage);

                    boxView_BlackLine_DisplayMessage = new BoxView()
                    {
                        Color = Color.Blue,
                        HeightRequest = 3,
                        IsEnabled = false,
                        IsVisible = true,
                    };
                    stackLayout.Children.Add(boxView_BlackLine_DisplayMessage);
                }
            }
            else if (ignoreMissingInputs == false)
            {
                //Invalid Inputs For Calculating Currency Arbitrage
                label_DisplayMessage = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "Invalid Inputs For Calculating Currency Arbitrage",
                    IsEnabled = false,
                    IsVisible = true,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.Red
                };
                stackLayout.Children.Add(label_DisplayMessage);

                boxView_BlackLine_DisplayMessage = new BoxView()
                {
                    Color = Color.Blue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = true,
                };
                stackLayout.Children.Add(boxView_BlackLine_DisplayMessage);
            }
        }
    }
}