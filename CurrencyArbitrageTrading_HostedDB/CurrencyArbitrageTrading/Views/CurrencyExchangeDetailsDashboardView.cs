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
    public class CurrencyExchangeDetailsDashboardView : ContentPage
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

        private Button button_CurrencyExchangeRatesHomePage;

        private Picker picker_BaseCurrencyPicker;
        private BoxView boxView_BlackLine_BaseCurrencyPicker;

        private Grid gridHeader;
        private Grid gridDetails;
        private ScrollView scrollView_Grid_AllLatestCurrencyExchangeRates;

        private Label label_DisplayMessage;
        private BoxView boxView_BlackLine_DisplayMessage;
        private StackLayout stackLayout;
        private ScrollView scrollView_HomePage;

        public CurrencyExchangeDetailsDashboardView()
        {
            try
            {
                Stopwatch stopWatchApplicationLoad = Stopwatch.StartNew();

                this.Title = "Currency Exchange Details Dashboard";

                //Initialize DatabaseServices
                DatabaseServices = new DatabaseServices();

                //Reset SQLite Database
                DatabaseServices.ResetSQLiteDatabase(paramDeleteExistingDatabase: false, paramUseEmbeddedSQLiteDatabase: false);

                //Populate Initial Currency Exchange Rates Dataset
                InitialCurrencyExchangeDatasetLoad();

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
                    Text = "Currency Exchange Details Dashboard",
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

                button_CurrencyExchangeRatesHomePage = new Button();
                button_CurrencyExchangeRatesHomePage.Text = "Currency Exchange Rates Dashboard";
                button_CurrencyExchangeRatesHomePage.FontAttributes = FontAttributes.Bold;
                button_CurrencyExchangeRatesHomePage.Clicked += Button_CurrencyExchangeRatesHomePage_Clicked;

                picker_BaseCurrencyPicker = new Picker();
                picker_BaseCurrencyPicker.Title = "Select Base Currency";
                picker_BaseCurrencyPicker.ItemsSource = this.PreferredCurrencyCodesList;
                picker_BaseCurrencyPicker.SelectedIndex = 12; //PreSelect (Default) Index 12 // USD - United States Dollar
                picker_BaseCurrencyPicker.SelectedIndexChanged += Picker_BaseCurrencyPicker_SelectedIndexChanged;
                picker_BaseCurrencyPicker.FontAttributes = FontAttributes.Bold;
                picker_BaseCurrencyPicker.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                picker_BaseCurrencyPicker.TextColor = Color.DarkBlue;
                picker_BaseCurrencyPicker.HorizontalOptions = LayoutOptions.FillAndExpand;
                picker_BaseCurrencyPicker.BackgroundColor = Color.LightGoldenrodYellow;

                //Box View Line
                boxView_BlackLine_BaseCurrencyPicker = new BoxView()
                {
                    Color = Color.DarkBlue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = true,
                };

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
                scrollView_Grid_AllLatestCurrencyExchangeRates = new ScrollView()
                {
                    Orientation = ScrollOrientation.Both,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Default,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Default,
                    IsVisible = false,
                    IsEnabled = false,
                    Content = gridDetails
                };

                if (this.LatestCurrencyExchangeRatesList != null && this.LatestCurrencyExchangeRatesList.Count() > 0)
                {
                    string dateValue = this.LatestCurrencyExchangeRatesList.Select(type => type.Date).FirstOrDefault();

                    //Display Label
                    label_DisplayMessage = new Label()
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        Text = "Currency Exchange Rates (" + dateValue + ")",
                        IsEnabled = false,
                        IsVisible = true,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.DarkBlue
                    };
                }
                else
                {
                    //Display Label
                    label_DisplayMessage = new Label()
                    {
                        LineBreakMode = LineBreakMode.WordWrap,
                        Text = "Invalid Inputs For Displaying Exchange Rates",
                        IsEnabled = false,
                        IsVisible = true,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        TextColor = Color.Red
                    };
                }

                //Box View Line
                boxView_BlackLine_DisplayMessage = new BoxView()
                {
                    Color = Color.DarkBlue,
                    HeightRequest = 3,
                    IsEnabled = false,
                    IsVisible = true,
                };

                stackLayout = new StackLayout();

                //stackLayout.Children.Add(image_CurrencyExchangeImage);
                //stackLayout.Children.Add(label_CurrencyExchangeApplication);
                //stackLayout.Children.Add(boxView_BlackLine_Header);
                //stackLayout.Children.Add(button_CurrencyExchangeRatesHomePage);

                stackLayout.Children.Add(label_DisplayMessage);
                stackLayout.Children.Add(boxView_BlackLine_DisplayMessage);

                stackLayout.Children.Add(picker_BaseCurrencyPicker);
                stackLayout.Children.Add(boxView_BlackLine_BaseCurrencyPicker);

                stackLayout.Children.Add(gridHeader);
                stackLayout.Children.Add(scrollView_Grid_AllLatestCurrencyExchangeRates);

                //Get ALL Latest Exchange Rates Data
                GetCurrencyExchangeDetails();

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

        private void Picker_BaseCurrencyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get Currency Exchange Details
            GetCurrencyExchangeDetails();
        }

        private void GetCurrencyExchangeDetails()
        {
            //Hide Unused Controls
            HideUnusedControls();

            ////Initialize LatestCurrencyExchangeRatesList
            //this.LatestCurrencyExchangeRatesList = new List<CurrencyExchangeRates>();

            ////Get Latest Currency Exchange Rates From HistoricCurrencyExchangeRatesList
            //PopulateLatestFromHistoricExchangeRates();

            if (this.LatestCurrencyExchangeRatesList != null && this.LatestCurrencyExchangeRatesList.Count() > 0 && picker_BaseCurrencyPicker.SelectedItem != null)
            {
                string dateValue = this.LatestCurrencyExchangeRatesList.Select(type => type.Date).FirstOrDefault();
                string baseCurrencyCode = ((CurrencyCodes)picker_BaseCurrencyPicker.SelectedItem).CurrencyCode;
                string baseCurrencyDescription = ((CurrencyCodes)picker_BaseCurrencyPicker.SelectedItem).CurrencyDescription;

                label_DisplayMessage.Text = baseCurrencyDescription + " (" + dateValue + ")";

                //Grid - Header
                gridHeader = new Grid()
                {
                    //VerticalOptions = LayoutOptions.FillAndExpand,
                    ColumnDefinitions =
                    {
                    new ColumnDefinition { Width = new GridLength(0.38, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(0.38, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(0.24, GridUnitType.Star) }
                    }
                };

                int rowHeaderNumber = 0;

                Color labelHeaderBackgroundColor = Color.Orange;

                Label BaseCurrencyCodeHeaderLabel = new Label { Text = "BaseCurrency", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };
                Label TargetCurrencyCodeHeaderLabel = new Label { Text = "TargetCurrency", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };
                Label ExchangeRateHeaderLabel = new Label { Text = "ExchangeRate", TextColor = Color.DarkBlue, BackgroundColor = labelHeaderBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Label)), FontAttributes = FontAttributes.Bold };
                gridHeader.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                gridHeader.Children.Add(BaseCurrencyCodeHeaderLabel, 0, rowHeaderNumber);
                gridHeader.Children.Add(TargetCurrencyCodeHeaderLabel, 1, rowHeaderNumber);
                gridHeader.Children.Add(ExchangeRateHeaderLabel, 2, rowHeaderNumber);
                stackLayout.Children.Add(gridHeader);

                //Grid - Details
                gridDetails = new Grid()
                {
                    //VerticalOptions = LayoutOptions.FillAndExpand,
                    ColumnDefinitions =
                    {
                    new ColumnDefinition { Width = new GridLength(0.38, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(0.38, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(0.24, GridUnitType.Star) }
                    }
                };

                int rowDetailNumber = 0;
                foreach (CurrencyExchangeRates eachValue in this.LatestCurrencyExchangeRatesList.Where(type => type.BaseCurrencyCode == baseCurrencyCode).ToList())
                {
                    string BaseCurrencyCode = eachValue.BaseCurrencyCode;
                    string TargetCurrencyCode = eachValue.TargetCurrencyCode;
                    decimal ExchangeRate = eachValue.ExchangeRate;

                    string BaseCurrencyDescription = this.PreferredCurrencyCodesList.Where(type => type.CurrencyCode == BaseCurrencyCode).Select(type => type.CurrencyDescription).FirstOrDefault();
                    string TargetCurrencyDescription = this.PreferredCurrencyCodesList.Where(type => type.CurrencyCode == TargetCurrencyCode).Select(type => type.CurrencyDescription).FirstOrDefault();

                    Color labelBackgroundColor = Color.Default;
                    if (rowDetailNumber % 2 == 0) //For Even & Odd Number Rows - Set Set Different Row Background Color
                    {
                        labelBackgroundColor = Color.LightGray;
                    }
                    else
                    {
                        labelBackgroundColor = Color.LightSkyBlue;
                    }

                    Label BaseCurrencyCodeLabel = new Label { Text = BaseCurrencyDescription, TextColor = Color.Black, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                    Label TargetCurrencyCodeLabel = new Label { Text = TargetCurrencyDescription, TextColor = Color.DarkBlue, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
                    Label ExchangeRateLabel = new Label { Text = ExchangeRate.ToString("N10"), TextColor = Color.Blue, BackgroundColor = labelBackgroundColor, FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };

                    gridDetails.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    gridDetails.Children.Add(BaseCurrencyCodeLabel, 0, rowDetailNumber);
                    gridDetails.Children.Add(TargetCurrencyCodeLabel, 1, rowDetailNumber);
                    gridDetails.Children.Add(ExchangeRateLabel, 2, rowDetailNumber);

                    rowDetailNumber++;
                }

                //Scroll View
                scrollView_Grid_AllLatestCurrencyExchangeRates = new ScrollView()
                {
                    Orientation = ScrollOrientation.Both,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Always,
                    IsVisible = true,
                    IsEnabled = true,
                    Content = gridDetails
                };
                stackLayout.Children.Add(scrollView_Grid_AllLatestCurrencyExchangeRates);
            }
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
            stackLayout.Children.Remove(gridHeader);
            stackLayout.Children.Remove(scrollView_Grid_AllLatestCurrencyExchangeRates);
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
    }
}