from database import DatabaseMethods

import numpy as np
import unittest


class TestInitialization(unittest.TestCase):
    def setUp(self):
        """Randomly select test cases

        References:
            https://docs.scipy.org/doc/numpy-1.14.0/reference/routines.random.html
        """
        self.output: bool = True  # toggle outputs

        # Valid currency code symbols
        self.codes_all = ['AUD', 'BGN', 'BRL', 'CAD', 'CHF', 'CNY', 'CZK', 'DKK', 'GBP', 'HKD', 'HRK', 'HUF', 'IDR', 'ILS', 'INR', 'ISK', 'JPY', 'KRW', 'MXN', 'MYR', 'NOK', 'NZD', 'PHP', 'PLN', 'RON', 'RUB', 'SEK', 'SGD', 'THB', 'TRY', 'USD', 'ZAR']

        # Grab a random base value
        self.base = np.random.choice(self.codes_all)

        # Grab a random subset of currency values
        size = np.random.randint(1, len(self.codes_all))
        self.codes_sel = np.random.choice(self.codes_all, size=size)

        pass

    def test_webapi_get_latest_default(self):
        """Get the latest rates using default inputs"""
        data = DatabaseMethods.get_currency_data_latest()

        if self.output:
            print(data)

        self.assertTrue(isinstance(data, dict))

    def test_webapi_get_latest_specified_base(self):
        """Get the latest rates using specified base currency"""
        data = DatabaseMethods.get_currency_data_latest(base=self.base)

        if self.output:
            print(data)

        self.assertTrue(isinstance(data, dict))

    def test_webapi_get_latest_specified_target(self):
        """Get the latest rates using specified targets"""
        data = DatabaseMethods.get_currency_data_latest(symbols=self.codes_sel)

        if self.output:
            print(data)

        self.assertTrue(isinstance(data, dict))

    def test_webapi_get_latest_specified_base_target(self):
        """Get the latest rates using specified base & targets"""
        data = DatabaseMethods.get_currency_data_latest(
            base=self.base, symbols=self.codes_sel
        )

        if self.output:
            print(data)

        self.assertTrue(isinstance(data, dict))
