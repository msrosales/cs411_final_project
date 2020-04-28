using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CurrencyArbitrageTrading.Services;
using CurrencyArbitrageTrading.Views;

using CurrencyArbitrageTrading.LibraryMethods;
using CurrencyArbitrageTrading.Models;
using CurrencyArbitrageTrading.Constants;
using CurrencyArbitrageTrading.DatabaseInterfaces;

namespace CurrencyArbitrageTrading
{
    public partial class App : Application
    {
        public DatabaseServices DatabaseServices { get; set; }

        public App()
        {
            InitializeComponent();
            DependencyService.Register<MockDataStore>();

            //Initialize DatabaseServices
            DatabaseServices = new DatabaseServices();

            //Root Page Of Mobile Application
            //MainPage = new MainPage();
            MainPage = new NavigationPage(new CurrencyExchangeRateCalculatorView());
            //StackOverflow - Xamarin Forms Page Navigation: https://stackoverflow.com/questions/25165106/how-do-you-switch-pages-in-xamarin-forms
            //https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/navigation/
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
