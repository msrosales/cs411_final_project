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
    public class CurrencyExchangeRateCalculatorView : ContentPage
    {
        public DatabaseServices DatabaseServices { get; set; }

        private List<CurrencyExchangeRates> LatestCurrencyExchangeRatesList { get; set; }
        private List<CurrencyExchangeRates> HistoricCurrencyExchangeRatesList { get; set; }
        private List<CurrencyExchangeRatesArchive> ArchiveCurrencyExchangeRatesList { get; set; }
        private List<CurrencyCodes> PreferredCurrencyCodesList { get; set; }
        private List<Microcharts.Entry> microchartEntries { get; set; }

        private Image image_CurrencyExchangeImage;
        private Label label_CurrencyExchangeApplication;
        private BoxView boxView_BlackLine_Header;

        private Picker picker_BaseCurrencyPicker;
        private Entry entry_BaseCurrencyValue;
        private Picker picker_TargetCurrencyPicker;
        private Entry entry_TargetCurrencyValue;
        private Button button_CalculateCurrencyExchangeRates;
        private Button button_CurrencyExchangeDetailsDashboard;
        private Button button_CurrencyArbitrageDashboard;
        private Button button_ResetSQLiteDatabase;

        private ListView listView_AllLatestCurrencyExchangeRates;
        private ScrollView scrollView_ListView_AllLatestCurrencyExchangeRates;

        private List<ChartPeriod> ChartPeriodInDays { get; set; }
        private Picker picker_ChartPeriodInDays;

        private Label label_DisplayMessage;
        private BoxView boxView_BlackLine_DisplayMessage;

        private Label label_DisplayMessage_LineChart;
        private BoxView boxView_BlackLine_LineChart;

        private Label label_DisplayMessage_BarChart;
        private BoxView boxView_BlackLine_BarChart;

        private ChartView chartView_CurrencyExchangeRates_LineChart;
        private ChartView chartView_CurrencyExchangeRates_BarChart;
        private StackLayout stackLayout;
        private ScrollView scrollView_HomePage;

        public CurrencyExchangeRateCalculatorView()
        {
            try
            {
                Stopwatch stopWatchApplicationLoad = Stopwatch.StartNew();

                this.Title = "Currency Exchange Rates Dashboard";

                //Initialize DatabaseServices
                DatabaseServices = new DatabaseServices();

                //Reset SQLite Database
                DatabaseServices.ResetSQLiteDatabase(paramDeleteExistingDatabase: false, paramUseEmbeddedSQLiteDatabase: false);

                //Populate Initial Currency Exchange Rates Dataset
                InitialCurrencyExchangeDatasetLoad();

                this.ChartPeriodInDays = new List<ChartPeriod>();
                this.ChartPeriodInDays.Add(new ChartPeriod() { Key = 5, Value = "5 Days" });
                this.ChartPeriodInDays.Add(new ChartPeriod() { Key = 10, Value = "10 Days" });
                this.ChartPeriodInDays.Add(new ChartPeriod() { Key = 15, Value = "15 Days" });
                this.ChartPeriodInDays.Add(new ChartPeriod() { Key = 20, Value = "20 Days" });
                this.ChartPeriodInDays.Add(new ChartPeriod() { Key = 25, Value = "25 Days" });
                this.ChartPeriodInDays.Add(new ChartPeriod() { Key = 30, Value = "30 Days" });

                picker_ChartPeriodInDays = new Picker();
                picker_ChartPeriodInDays.Title = "Chart Period";
                picker_ChartPeriodInDays.ItemsSource = this.ChartPeriodInDays;
                picker_ChartPeriodInDays.SelectedIndex = 5; //Default To 30 Days
                picker_ChartPeriodInDays.SelectedIndexChanged += Picker_ChartPeriodInDays_SelectedIndexChanged;
                picker_ChartPeriodInDays.IsEnabled = false;
                picker_ChartPeriodInDays.IsVisible = false;
                picker_ChartPeriodInDays.FontAttributes = FontAttributes.Bold;
                picker_ChartPeriodInDays.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                picker_ChartPeriodInDays.TextColor = Color.DarkBlue;
                picker_ChartPeriodInDays.HorizontalOptions = LayoutOptions.Center;

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
                    Text = "Currency Exchange Rates Dashboard",
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

                entry_BaseCurrencyValue = new Entry();
                entry_BaseCurrencyValue.Keyboard = Keyboard.Numeric;
                entry_BaseCurrencyValue.Text = "1";
                entry_BaseCurrencyValue.Placeholder = "Base Currency Value";
                entry_BaseCurrencyValue.TextChanged += Entry_BaseCurrencyValue_TextChanged;
                entry_BaseCurrencyValue.FontAttributes = FontAttributes.Bold;
                entry_BaseCurrencyValue.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                entry_BaseCurrencyValue.TextColor = Color.DarkBlue;
                entry_BaseCurrencyValue.HorizontalOptions = LayoutOptions.FillAndExpand;

                picker_TargetCurrencyPicker = new Picker();
                picker_TargetCurrencyPicker.Title = "Select Target Currency";
                picker_TargetCurrencyPicker.ItemsSource = this.PreferredCurrencyCodesList;
                picker_TargetCurrencyPicker.SelectedIndexChanged += Picker_TargetCurrencyPicker_SelectedIndexChanged;
                picker_TargetCurrencyPicker.FontAttributes = FontAttributes.Bold;
                picker_TargetCurrencyPicker.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                picker_TargetCurrencyPicker.TextColor = Color.DarkBlue;
                picker_TargetCurrencyPicker.HorizontalOptions = LayoutOptions.FillAndExpand;

                entry_TargetCurrencyValue = new Entry();
                entry_TargetCurrencyValue.Keyboard = Keyboard.Numeric;
                entry_TargetCurrencyValue.Text = string.Empty;
                entry_TargetCurrencyValue.Placeholder = "Target Currency Value";
                entry_TargetCurrencyValue.IsReadOnly = true;
                entry_TargetCurrencyValue.FontAttributes = FontAttributes.Bold;
                entry_TargetCurrencyValue.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                entry_TargetCurrencyValue.TextColor = Color.Blue;
                entry_TargetCurrencyValue.HorizontalOptions = LayoutOptions.FillAndExpand;

                button_CalculateCurrencyExchangeRates = new Button();
                button_CalculateCurrencyExchangeRates.Text = "Calculate Currency Exchange Rates";
                button_CalculateCurrencyExchangeRates.FontAttributes = FontAttributes.Bold;
                button_CalculateCurrencyExchangeRates.Clicked += Button_CalculateCurrencyExchangeRates_Clicked;

                button_CurrencyExchangeDetailsDashboard = new Button();
                button_CurrencyExchangeDetailsDashboard.Text = "Currency Exchange Details Dashboard";
                button_CurrencyExchangeDetailsDashboard.FontAttributes = FontAttributes.Bold;
                button_CurrencyExchangeDetailsDashboard.Clicked += Button_CurrencyExchangeDetailsDashboard_Clicked;

                button_CurrencyArbitrageDashboard = new Button();
                button_CurrencyArbitrageDashboard.Text = "Currency Arbitrage Dashboard";
                button_CurrencyArbitrageDashboard.FontAttributes = FontAttributes.Bold;
                button_CurrencyArbitrageDashboard.Clicked += Button_CurrencyArbitrageDashboard_Clicked;

                button_ResetSQLiteDatabase = new Button();
                button_ResetSQLiteDatabase.Text = "Reset Local SQLite Database";
                button_ResetSQLiteDatabase.FontAttributes = FontAttributes.Bold;
                button_ResetSQLiteDatabase.Clicked += Button_ResetSQLiteDatabase_Clicked;

                listView_AllLatestCurrencyExchangeRates = new ListView()
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                    ItemsSource = this.LatestCurrencyExchangeRatesList,
                    Header = "Latest Exchange Rates",
                    BackgroundColor = Color.LightYellow,
                    SelectionMode = ListViewSelectionMode.None,
                    IsVisible = false
                };

                scrollView_ListView_AllLatestCurrencyExchangeRates = new ScrollView()
                {
                    Orientation = ScrollOrientation.Both,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Default,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Default,
                    IsVisible = false,
                    Content = listView_AllLatestCurrencyExchangeRates
                };

                label_DisplayMessage = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "Currency Exchange Rates",
                    IsEnabled = false,
                    IsVisible = false,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.Black
                };

                label_DisplayMessage_LineChart = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "* Line Chart *",
                    IsEnabled = false,
                    IsVisible = false,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.Black
                };

                boxView_BlackLine_DisplayMessage = new BoxView()
                {
                    Color = Color.Blue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = false,
                };

                boxView_BlackLine_LineChart = new BoxView()
                {
                    Color = Color.Blue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = false,
                };

                boxView_BlackLine_BarChart = new BoxView()
                {
                    Color = Color.Blue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = false,
                };

                chartView_CurrencyExchangeRates_LineChart = new ChartView()
                {
                    MinimumHeightRequest = 150,
                    HeightRequest = 150,
                    IsEnabled = false,
                    IsVisible = false
                };

                label_DisplayMessage_BarChart = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "* Bar Chart *",
                    IsEnabled = false,
                    IsVisible = false,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.Black
                };

                chartView_CurrencyExchangeRates_BarChart = new ChartView()
                {
                    MinimumHeightRequest = 150,
                    HeightRequest = 150,
                    IsEnabled = false,
                    IsVisible = false
                };

                stackLayout = new StackLayout();
                stackLayout.Children.Add(image_CurrencyExchangeImage);
                stackLayout.Children.Add(label_CurrencyExchangeApplication);
                stackLayout.Children.Add(boxView_BlackLine_Header);
                stackLayout.Children.Add(picker_BaseCurrencyPicker);
                stackLayout.Children.Add(entry_BaseCurrencyValue);
                stackLayout.Children.Add(picker_TargetCurrencyPicker);
                stackLayout.Children.Add(entry_TargetCurrencyValue);
                stackLayout.Children.Add(button_CalculateCurrencyExchangeRates);
                stackLayout.Children.Add(button_CurrencyExchangeDetailsDashboard);
                stackLayout.Children.Add(button_CurrencyArbitrageDashboard);
                stackLayout.Children.Add(button_ResetSQLiteDatabase);

                stackLayout.Children.Add(scrollView_ListView_AllLatestCurrencyExchangeRates);
                stackLayout.Children.Add(picker_ChartPeriodInDays);
                stackLayout.Children.Add(label_DisplayMessage);
                stackLayout.Children.Add(boxView_BlackLine_DisplayMessage);
                stackLayout.Children.Add(label_DisplayMessage_LineChart);
                stackLayout.Children.Add(chartView_CurrencyExchangeRates_LineChart);
                stackLayout.Children.Add(boxView_BlackLine_LineChart);
                stackLayout.Children.Add(label_DisplayMessage_BarChart);
                stackLayout.Children.Add(chartView_CurrencyExchangeRates_BarChart);
                stackLayout.Children.Add(boxView_BlackLine_BarChart);

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
                string ExceptionMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(ex.Message) == false)
                {
                    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                }
                DisplayAlert("Exception! CurrencyExchangeRateCalculatorView", "Error while loading the Android Application Dashboard Page." + ExceptionMessage, "OK");
            }
        }

        private void Picker_ChartPeriodInDays_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Calculate Currency Exchange Rate
            CalculateCurrencyExchangeRate(ignoreMissingInputs: true);
        }

        private void Picker_BaseCurrencyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Calculate Currency Exchange Rate
            CalculateCurrencyExchangeRate(ignoreMissingInputs: true);
        }

        private void Entry_BaseCurrencyValue_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            //Calculate Currency Exchange Rate
            CalculateCurrencyExchangeRate(ignoreMissingInputs: true);
        }

        private void Picker_TargetCurrencyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Calculate Currency Exchange Rate
            CalculateCurrencyExchangeRate(ignoreMissingInputs: true);
        }

        private void Button_CalculateCurrencyExchangeRates_Clicked(object sender, EventArgs e)
        {
            //Calculate Currency Exchange Rate
            CalculateCurrencyExchangeRate(ignoreMissingInputs: false);
        }

        private async void Button_CurrencyExchangeDetailsDashboard_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Hide Unused Controls
                HideUnusedControls();

                /*
                //Latest Exchange Rates View Page

                //Initialize LatestCurrencyExchangeRatesList
                this.LatestCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();

                //Get Latest Currency Exchange Rates From HistoricCurrencyExchangeRatesList
                PopulateLatestFromHistoricExchangeRates();

                if (this.LatestCurrencyExchangeRatesList != null && this.LatestCurrencyExchangeRatesList.Count() > 0)
                {
                    string dateValue = this.LatestCurrencyExchangeRatesList.Select(type => type.Date).FirstOrDefault();

                    listView_AllLatestCurrencyExchangeRates = new ListView()
                    {
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                        ItemsSource = this.LatestCurrencyExchangeRatesList,
                        Header = "Latest Currency Exchange Rates (" + dateValue + ")",
                        BackgroundColor = Color.LightYellow,                    
                        SelectionMode = ListViewSelectionMode.None,
                        IsVisible = true
                    };

                    scrollView_ListView_AllLatestCurrencyExchangeRates = new ScrollView()
                    {
                        Orientation = ScrollOrientation.Both,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                        IsVisible = true,
                        Content = listView_AllLatestCurrencyExchangeRates
                    };

                    stackLayout.Children.Add(scrollView_ListView_AllLatestCurrencyExchangeRates);
                }
                */

                //Page Re-direction Alert
                //await DisplayAlert("Latest Exchange Rates", "Page Re-direction: You are being re-directed to Latest Exchange Rates.", "OK");

                //Navigate To Latest Exchange Rates View Page
                await Navigation.PushAsync(new CurrencyExchangeDetailsDashboardView());
            }
            catch (Exception ex)
            {
                string ExceptionMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(ex.Message) == false)
                {
                    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                }
                await DisplayAlert("Exception! GetAllLatestCurrencyExchangeRates", "Please wait for few moments for the Database Operations to Complete." + ExceptionMessage, "OK");
            }
        }

        private async void Button_CurrencyArbitrageDashboard_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Hide Unused Controls
                HideUnusedControls();

                /*
                //Currency Arbitrage Rates Calculator Page Under Construction
                label_DisplayMessage = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "Go To Currency Arbitrage Dashboard Page",
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
                //await DisplayAlert("Currency Arbitrage Dashboard", "Page Re-direction: You are being re-directed to Currency Arbitrage Dashboard.", "OK");

                //Navigate To Currency Arbitrage Rates Calculator Page
                await Navigation.PushAsync(new CurrencyArbitrageDashboardView());
            }
            catch (Exception ex)
            {
                string ExceptionMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(ex.Message) == false)
                {
                    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                }
                await DisplayAlert("Exception! CurrencyArbitrageDashboardPage", "Please wait for few moments for the Database Operations to Complete." + ExceptionMessage, "OK");
            }
        }

        private async void Button_ResetSQLiteDatabase_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (await DisplayAlert("Confirm Local Database Reset", "Are you sure you want to RESET your Local SQLite Database? This could take few moments to complete.", "RESET", "CANCEL"))
                {
                    //Hide Unused Controls
                    HideUnusedControls();

                    //Reset SQLite Database
                    bool isResetSuccess = DatabaseServices.ResetSQLiteDatabase(paramDeleteExistingDatabase: true, paramUseEmbeddedSQLiteDatabase: false);

                    //Populate Initial Currency Exchange Rates Dataset
                    InitialCurrencyExchangeDatasetLoad();

                    if (isResetSuccess == true)
                    {
                        label_DisplayMessage = new Label()
                        {
                            LineBreakMode = LineBreakMode.WordWrap,
                            Text = "Local SQLite Database Reset Successfully",
                            IsEnabled = false,
                            IsVisible = true,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = Color.DarkGreen
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
                else
                {
                    //Do Nothing
                    await DisplayAlert("Local Database Reset Cancelled", "Local Database Reset Cancelled.", "OK");
                }
            }
            catch (Exception ex)
            {
                string ExceptionMessage = string.Empty;
                if (string.IsNullOrWhiteSpace(ex.Message) == false)
                {
                    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                }
                await DisplayAlert("Exception! Reset Local SQLite Database", "Please wait for few moments for the Local Database Reset Operations to Complete." + ExceptionMessage, "OK");
            }
        }

        private void HideUnusedControls()
        {
            //Hide All Other Display Controls In The Current Form
            stackLayout.Children.Remove(scrollView_ListView_AllLatestCurrencyExchangeRates);

            picker_ChartPeriodInDays.IsEnabled = false;
            picker_ChartPeriodInDays.IsVisible = false;
            stackLayout.Children.Remove(label_DisplayMessage);
            stackLayout.Children.Remove(boxView_BlackLine_DisplayMessage);

            stackLayout.Children.Remove(label_DisplayMessage_LineChart);
            stackLayout.Children.Remove(chartView_CurrencyExchangeRates_LineChart);
            stackLayout.Children.Remove(boxView_BlackLine_LineChart);

            stackLayout.Children.Remove(label_DisplayMessage_BarChart);
            stackLayout.Children.Remove(chartView_CurrencyExchangeRates_BarChart);
            stackLayout.Children.Remove(boxView_BlackLine_BarChart);
        }

        private void CalculateCurrencyExchangeRate(bool ignoreMissingInputs = false)
        {
            //Hide Unused Controls
            HideUnusedControls();

            //Reset Target Currency Value
            entry_TargetCurrencyValue.Text = string.Empty;

            if (picker_BaseCurrencyPicker.SelectedItem != null
                && picker_TargetCurrencyPicker.SelectedItem != null
                && string.IsNullOrWhiteSpace(entry_BaseCurrencyValue.Text) == false
                && decimal.TryParse(entry_BaseCurrencyValue.Text, out decimal baseCurrencyValueToConvert))
            {
                string baseCurrencyCode = ((CurrencyCodes)picker_BaseCurrencyPicker.SelectedItem).CurrencyCode;
                string baseCurrencyDescription = ((CurrencyCodes)picker_BaseCurrencyPicker.SelectedItem).CurrencyDescription;
                string targetCurrencyCode = ((CurrencyCodes)picker_TargetCurrencyPicker.SelectedItem).CurrencyCode;
                string targetCurrencyDescription = ((CurrencyCodes)picker_TargetCurrencyPicker.SelectedItem).CurrencyDescription;

                if (baseCurrencyCode.Equals(targetCurrencyCode))
                {
                    entry_TargetCurrencyValue.Text = entry_BaseCurrencyValue.Text;
                }
                else
                {
                    decimal exchangeRatePerUnitBase = this.LatestCurrencyExchangeRatesList
                                                            .Where(type => type.BaseCurrencyCode == baseCurrencyCode
                                                                            && type.TargetCurrencyCode == targetCurrencyCode)
                                                            .Select(type => type.ExchangeRate)
                                                            .FirstOrDefault();
                    decimal exchangeRateInTargetCurrency = exchangeRatePerUnitBase * baseCurrencyValueToConvert;
                    entry_TargetCurrencyValue.Text = exchangeRateInTargetCurrency.ToString("N10");

                    #region Plot Chart

                    int chartPeriodInDays = 30;
                    if (picker_ChartPeriodInDays.SelectedItem != null)
                    {

                        picker_ChartPeriodInDays.IsEnabled = true;
                        picker_ChartPeriodInDays.IsVisible = true;

                        chartPeriodInDays = ((ChartPeriod)picker_ChartPeriodInDays.SelectedItem).Key;
                    }

                    List<CurrencyExchangeRates> exchangeRatesList = GetExchangeHistoryFromHistoricExchangeRates(paramBaseCurrencyCode: baseCurrencyCode, paramTargetCurrencyCode: targetCurrencyCode, paramChartPeriodInDays: chartPeriodInDays);

                    if (exchangeRatesList != null && exchangeRatesList.Count() > 0)
                    {
                        //Display Label
                        label_DisplayMessage = new Label()
                        {
                            LineBreakMode = LineBreakMode.WordWrap,
                            Text = baseCurrencyDescription + " vs. " + targetCurrencyDescription,
                            IsEnabled = false,
                            IsVisible = true,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = Color.DarkBlue
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

                        this.microchartEntries = new List<Microcharts.Entry>();

                        foreach (var eachExchangeRates in exchangeRatesList)
                        {
                            var random = new Random();
                            var chartColor = String.Format("#{0:X6}", random.Next(0x1000000));
                            var textColor = String.Format("#{0:X6}", random.Next(0x1000000));

                            this.microchartEntries.Add(new Microcharts.Entry((float)eachExchangeRates.ExchangeRate)
                            {
                                Color = SkiaSharp.SKColor.Parse(chartColor),
                                TextColor = SkiaSharp.SKColor.Parse(textColor),
                                Label = eachExchangeRates.Date,
                                ValueLabel = eachExchangeRates.ExchangeRate.ToString("N10")
                            });
                        }

                        decimal minValue = exchangeRatesList.Min(type => type.ExchangeRate);
                        decimal maxValue = exchangeRatesList.Max(type => type.ExchangeRate);

                        //Display Label
                        label_DisplayMessage_LineChart = new Label()
                        {
                            LineBreakMode = LineBreakMode.WordWrap,
                            Text = " * Line Chart * ",
                            IsEnabled = false,
                            IsVisible = true,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = Color.DarkBlue
                        };
                        stackLayout.Children.Add(label_DisplayMessage_LineChart);

                        //Line Chart
                        chartView_CurrencyExchangeRates_LineChart = new ChartView()
                        {
                            BackgroundColor = Color.FromHex("#DEDEDC"),
                            MinimumHeightRequest = 150,
                            HeightRequest = 150,
                            IsEnabled = false,
                            IsVisible = true,
                            Chart = new Microcharts.LineChart()
                            {
                                MinValue = (float)(minValue - (minValue * 5 / 100)),
                                MaxValue = (float)(maxValue + (maxValue * 5 / 100)),
                                Margin = 2,
                                Entries = this.microchartEntries,
                                LineMode = Microcharts.LineMode.Spline,
                                LineSize = 8,
                                PointMode = Microcharts.PointMode.Circle,
                                PointSize = 12
                            }
                        };
                        stackLayout.Children.Add(chartView_CurrencyExchangeRates_LineChart);

                        boxView_BlackLine_LineChart = new BoxView()
                        {
                            Color = Color.Blue,
                            HeightRequest = 3,
                            IsEnabled = false,
                            IsVisible = true,
                        };
                        stackLayout.Children.Add(boxView_BlackLine_LineChart);

                        //Display Label
                        label_DisplayMessage_BarChart = new Label()
                        {
                            LineBreakMode = LineBreakMode.WordWrap,
                            Text = " * Bar Chart * ",
                            IsEnabled = false,
                            IsVisible = true,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center,
                            TextColor = Color.DarkBlue
                        };
                        stackLayout.Children.Add(label_DisplayMessage_BarChart);

                        //Bar Chart
                        chartView_CurrencyExchangeRates_BarChart = new ChartView()
                        {
                            BackgroundColor = Color.FromHex("#DEDEDC"),
                            MinimumHeightRequest = 150,
                            HeightRequest = 150,
                            IsEnabled = false,
                            IsVisible = true,
                            Chart = new Microcharts.BarChart()
                            {
                                MinValue = (float)(minValue - (minValue * 5 / 100)),
                                MaxValue = (float)(maxValue + (maxValue * 5 / 100)),
                                Margin = 2,
                                Entries = this.microchartEntries,
                                BarAreaAlpha = 25,
                                PointMode = Microcharts.PointMode.Circle,
                                PointSize = 12
                            }
                        };
                        stackLayout.Children.Add(chartView_CurrencyExchangeRates_BarChart);

                        boxView_BlackLine_BarChart = new BoxView()
                        {
                            Color = Color.Blue,
                            HeightRequest = 3,
                            IsEnabled = false,
                            IsVisible = true,
                        };
                        stackLayout.Children.Add(boxView_BlackLine_BarChart);
                    }

                    #endregion Plot Chart

                    if (exchangeRateInTargetCurrency == 0 || exchangeRatesList == null || exchangeRatesList.Count() == 0)
                    {
                        DisplayAlert("Exchange Rates Unavailable!", "Currency Exchange Rates Unavailable For The Selected Currencies.", "OK");
                    }
                }
            }
            else if (ignoreMissingInputs == false)
            {
                //Invalid Inputs For Calculating ExchangeRates
                label_DisplayMessage = new Label()
                {
                    LineBreakMode = LineBreakMode.WordWrap,
                    Text = "Invalid Inputs For Calculating ExchangeRates",
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
                        foreach( var eachArchiveCurrencyExchangeRates in this.ArchiveCurrencyExchangeRatesList)
                        {
                            if (this.HistoricCurrencyExchangeRatesList.Where(type => type.Date == eachArchiveCurrencyExchangeRates.Date
                                                                                && type. BaseCurrencyCode == eachArchiveCurrencyExchangeRates.BaseCurrencyCode
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

        private List<CurrencyExchangeRates> GetExchangeHistoryFromHistoricExchangeRates(string paramBaseCurrencyCode, string paramTargetCurrencyCode, int paramChartPeriodInDays = 30)
        {
            List<CurrencyExchangeRates> exchangeRatesList = new List<CurrencyExchangeRates>();
            List<CurrencyExchangeRates> returnExchangeRatesList = new List<CurrencyExchangeRates>();

            try
            {
                if (this.HistoricCurrencyExchangeRatesList != null && this.HistoricCurrencyExchangeRatesList.Count() > 0)
                {
                    //Get Currency Exchange Rates From HistoricCurrencyExchangeRatesList
                    exchangeRatesList = this.HistoricCurrencyExchangeRatesList.Where(type => type.BaseCurrencyCode == paramBaseCurrencyCode
                                                                                          && type.TargetCurrencyCode == paramTargetCurrencyCode)
                                                                              .OrderBy(type =>
                                                                              {
                                                                                  DateTime.TryParse(type.Date, out DateTime dateValue);
                                                                                  return dateValue;
                                                                              })
                                                                              .ToList();
                }
                else
                {
                    try
                    {
                        if (exchangeRatesList == null || exchangeRatesList.Count() == 0)
                        {
                            //Get Historic Currency Exchange Rates From Hosted Database
                            exchangeRatesList = CurrencyExchangeRateCalculatorLibrary.GetHistoricCurrencyExchangeRatesFromDatabase_HostedDB(paramBaseCurrencyCode, paramTargetCurrencyCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        //string ExceptionMessage = string.Empty;
                        //if (string.IsNullOrWhiteSpace(ex.Message) == false)
                        //{
                        //    ExceptionMessage = Environment.NewLine + "Exception Message: " + ex.Message;
                        //}
                        //DisplayAlert("Exception! Historic Currency Exchange Dataset Load", "Error During Historic Currency Exchange Dataset Load." + ExceptionMessage, "OK");
                    }

                    if (exchangeRatesList == null || exchangeRatesList.Count() == 0)
                    {
                        exchangeRatesList = DatabaseServices.GetHistoricCurrencyExchangeRatesFromDatabase(paramBaseCurrencyCode, paramTargetCurrencyCode);
                    }
                }

                DateTime chartMinDate = DateTime.Now.Date.AddDays(-paramChartPeriodInDays);
                foreach (CurrencyExchangeRates eachExchangeRate in exchangeRatesList)
                {
                    if (DateTime.TryParse(eachExchangeRate.Date, out DateTime dateValue))
                    {
                        if (dateValue >= chartMinDate)
                        {
                            returnExchangeRatesList.Add(eachExchangeRate);
                        }
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
                DisplayAlert("Exception! Historic Currency Exchange Dataset Load", "Error During Historic Currency Exchange Dataset Load." + ExceptionMessage, "OK");
            }

            return returnExchangeRatesList;
        }
    }
}