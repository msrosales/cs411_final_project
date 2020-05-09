from bs4 import BeautifulSoup
import os
import pandas as pd
import requests

''' Tags '''
CUR_CODE = 'Currency Code'
CUR_NAME = 'Currency Name'

''' Inputs '''
currencies = ['USD', 'EUR', 'GBP', 'INR', 'AUD', 'CAD', 'SGD', 'CHF', 'MYR', 'JPY', 'CNY', 'NZD', 'THB', 'HUF', 'AED', 'HKD', 'MXN', 'ZAR', 'PHP', 'SEK', 'IDR', 'SAR', 'BRL', 'TRY', 'KES', 'KRW', 'EGP', 'IQD', 'NOK', 'KWD', 'RUB', 'DKK', 'PKR', 'ILS', 'PLN', 'QAR', 'XAU', 'OMR', 'COP', 'CLP', 'TWD', 'ARS', 'CZK', 'VND', 'MAD', 'JOD', 'BHD', 'XOF', 'LKR', 'UAH', 'NGN', 'TND', 'UGX', 'RON', 'BDT', 'PEN', 'GEL', 'XAF', 'FJD', 'VEF', 'VES', 'BYN', 'HRK', 'UZS', 'BGN', 'DZD', 'IRR', 'DOP', 'ISK', 'XAG', 'CRC', 'SYP', 'LYD', 'JMD', 'MUR', 'GHS', 'AOA', 'UYU', 'AFN', 'LBP', 'XPF', 'TTD', 'TZS', 'ALL', 'XCD', 'GTQ', 'NPR', 'BOB', 'ZWD', 'BBD', 'CUC', 'LAK', 'BND', 'BWP', 'HNL', 'PYG', 'ETB', 'NAD', 'PGK', 'SDG', 'MOP', 'NIO', 'BMD', 'KZT', 'PAB', 'BAM', 'GYD', 'YER', 'MGA', 'KYD', 'MZN', 'RSD', 'SCR', 'AMD', 'SBD', 'AZN', 'SLL', 'TOP', 'BZD', 'MWK', 'GMD', 'BIF', 'SOS', 'HTG', 'GNF', 'MVR', 'MNT', 'CDF', 'STN', 'TJS', 'KPW', 'MMK', 'LSL', 'LRD', 'KGS', 'GIP', 'XPT', 'MDL', 'CUP', 'KHR', 'MKD', 'VUV', 'MRU', 'ANG', 'SZL', 'CVE', 'SRD', 'XPD', 'SVC', 'BSD', 'XDR', 'RWF', 'AWG', 'DJF', 'BTN', 'KMF', 'WST', 'SPL', 'ERN', 'FKP', 'SHP', 'JEP', 'TMT', 'TVD', 'IMP', 'GGP', 'ZMW']
# base_cur: str = 'EUR'  # Base currency
date_exc: str = '2020-02-16'  # date of exchange

''' Main Methods '''


def request_exchange_data(crn_code: str, date: str):
    """Request currency data for a specific date

    Args:
        crn_code (str): base currency code
        date (str): exchange date
    Returns:
        pd.DataFrame: exchange data
    """

    # Define url to parse from
    # https://www.xe.com/currencytables/?from=EUR&date=2020-02-16
    url = f'https://www.xe.com/currencytables/?from={crn_code}&date={date}'

    # Request response from url
    res = requests.get(url)

    # Grab the table from the response content
    # <table id="historicalRateTbl" class="tablesorter historicalRateTable-table">
    soup = BeautifulSoup(res.content, 'lxml')
    table = soup.find_all('table')[0]

    # Convert to a dataframe
    df = pd.read_html(str(table))[0]

    # Clean tag names
    df.columns = [CUR_CODE, CUR_NAME, f'Units per {crn_code}', f'{crn_code} per Unit']

    return df


def export_exchange_data(currencies: list):
    """Save currency exchange data"""

    for currency in currencies:

        print(f'Currency Code: {currency}')

        # Define output path
        filepath_out: str = os.path.join('output', currency + '.csv')

        # Grab data
        df = request_exchange_data(currency, date_exc)

        print(f' Saving file: {filepath_out}')
        df.to_csv(filepath_out, index=False)


export_exchange_data(currencies)
