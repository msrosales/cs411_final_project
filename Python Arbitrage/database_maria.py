import datetime
import decimal
import itertools
import json
import requests
import re
import MySQLdb

import time
from typing import List


""" Database Tags """

# Table names
TABLE_CURRENCY_CODE = "CurrencyCodes"
TABLE_RATES_CURRENT = "CurrencyExchangeRates"
TABLE_RATES_HISTORICAL = "CurrencyExchangeRatesArchive"
TABLE_RATES_ARB_LOG = "CurrencyArbitrageLog"

# Table: CurrencyCodes
CODE = "CurrencyCode"
DESC = "CurrencyDescription"

# Table: CurrencyExchangeRates
BASE = "BaseCurrencyCode"
TARGET = "TargetCurrencyCode"
DATE = "Date"
RATE = "ExchangeRate"

# Table: CurrencyArbitrageLog
DEG = "Degree"
DELTA = "Delta"
INTER = "IntermediateCurrencyCodes"
VAL_IMP = "ImpliedValue"
VAL_ACT = "ActualValue"

# Default parameters
SIGFIGS = "sigfigs"

# Prepared statements
STMNT_INSERT_ARB = 'InsertArbitrage'

# SQL variables
SQL_DATE = 'in_date'
SQL_DELTA = 'in_delta'
SQL_BASE = 'in_base'
SQL_INTR = 'in_inter'
SQL_TARGET = 'in_target'
SQL_DEG = 'in_deg'
SQL_RATE_DIR = 'in_rate_dir'
SQL_RATE_IND = 'in_rate_ind'


class DatabaseMethods(object):
    def __init__(self, debug: bool = False):

        # Database objects
        self.db = None  # database
        self.cursor = None  # database cursor
        self.debug = debug

        # Default parameters
        # - maximum number of significant figures
        # - maximum arbitrage degree
        # - available currency codes
        self.defaults = {
            SIGFIGS: None,
            DEG: 4,
            CODE: [
                "USD",
                "EUR",
                "JPY",
                "GBP",
                "AUD",
                "CAD",
                "CHF",
                "CNY",
                "HKD",
                "NZD",
            ],
        }

        # Prepared statement key order
        self.key_order_arb = [SQL_DATE, SQL_BASE, SQL_INTR, SQL_TARGET, SQL_DEG, SQL_DELTA, SQL_RATE_IND]

        # Currency codes (reduced set)
        # https://en.wikipedia.org/wiki/Template:Most_traded_currencies
        self.defaults[CODE] = sorted(self.defaults[CODE])

        # Server execution with MySQL
        if self.debug is False:

            filepath_db: str = "cs411arbitrage_ServerTest"
            self.load_database(filepath_db)
            self.populate()

        # Testing
        else:
            create: bool = False  # create a database from scratch
            filepath_db: str = "cs411arbitrage_ServerTest"  # location of test database

            if create:
                self.load_database(filepath_db)
                self.drop_all()
                self.create_database(filepath_db)
                self.populate_initial()
            else:

                self.load_database(filepath_db)

                for ix in [5, 7]: 

                    date = datetime.date(2020, 5, ix)
                    date = date.strftime("%Y-%m-%d")

                    self.populate_historical_all(symbols=self.defaults[CODE], start_at=date, end_at=date)
                    # self.populate_arbitrage(date, max_degree = self.defaults[DEG])

    """ Load & Close """

    def create_database(self, filepath: str):
        """Generate a database from scratch"""

        print(f" Creating table: {TABLE_CURRENCY_CODE}")
        cmd = """CREATE TABLE IF NOT EXISTS {table} (
                    {code} VARCHAR(3) NOT NULL,
                    {desc} VARCHAR(50) NOT NULL,
                    PRIMARY KEY({code})
                );""".format(
            table=TABLE_CURRENCY_CODE, code=CODE, desc=DESC
        )

        # Clean command, execute and check
        print(' ', cmd)
        cmd = re.sub("\\n|\\t|\s{2,9}", "", cmd)
        self.cursor.execute(cmd)
        self.db.commit()

        self.describe(TABLE_CURRENCY_CODE)

        print(f" Creating table: {TABLE_RATES_CURRENT}")
        cmd = """CREATE TABLE IF NOT EXISTS {table} (
                    {date} DATE NOT NULL,
                    {base} VARCHAR(3) NOT NULL,
                    {target} VARCHAR(3) NOT NULL,
                    {rate} DECIMAL(65,30) NOT NULL,
                    FOREIGN KEY({base}) REFERENCES {table_codes}({code}),
                    FOREIGN KEY({target}) REFERENCES {table_codes}({code}),
                    PRIMARY KEY({date},{base},{target})
                );""".format(
            table=TABLE_RATES_CURRENT,
            table_codes=TABLE_CURRENCY_CODE,
            date=DATE,
            code=CODE,
            base=BASE,
            target=TARGET,
            rate=RATE,
        )

        print(' ', cmd)
        cmd = re.sub("\\n|\\t|\s{2,9}", "", cmd)
        self.cursor.execute(cmd)
        self.db.commit()

        self.describe(TABLE_RATES_CURRENT)

        print(f" Creating table: {TABLE_RATES_HISTORICAL}")
        cmd = """CREATE TABLE IF NOT EXISTS {table} (
                    {date} DATE NOT NULL,
                    {base} VARCHAR(3) NOT NULL,
                    {target} VARCHAR(3) NOT NULL,
                    {rate} DECIMAL(65,30) NOT NULL,
                    FOREIGN KEY({base}) REFERENCES {table_codes}({code}),
                    FOREIGN KEY({target}) REFERENCES {table_codes}({code}),
                    PRIMARY KEY({date},{base},{target})
                );""".format(
            table=TABLE_RATES_HISTORICAL,
            table_codes=TABLE_CURRENCY_CODE,
            date=DATE,
            code=CODE,
            base=BASE,
            target=TARGET,
            rate=RATE,
        )

        print(' ', cmd)
        cmd = re.sub("\\n|\\t|\s{2,9}", "", cmd)
        self.cursor.execute(cmd)
        self.db.commit()

        self.describe(TABLE_RATES_HISTORICAL)

        print(f" Creating table: {TABLE_RATES_ARB_LOG}")
        cmd = """CREATE TABLE IF NOT EXISTS {table} (
                    {date} DATE NOT NULL,
                    {base} VARCHAR(3) NOT NULL,
                    {inter} VARCHAR(50) NOT NULL,
                    {target} VARCHAR(3) NOT NULL,
                    {degree} INTEGER NOT NULL,
                    {DELTA} DECIMAL(65,30) NOT NULL,
                    {actual} DECIMAL(65,30) NOT NULL,
                    PRIMARY KEY({date},{base},{inter},{target},{degree}),
                    FOREIGN KEY({base}) REFERENCES {table_codes}({code}),
                    FOREIGN KEY({target}) REFERENCES {table_codes}({code})
                );""".format(
            table=TABLE_RATES_ARB_LOG,
            table_codes=TABLE_CURRENCY_CODE,
            actual=VAL_ACT,
            base=BASE,
            code=CODE,
            date=DATE,
            degree=DEG,
            inter=INTER,
            implied=VAL_IMP,
            rate=RATE,
            target=TARGET,
        )

        print(' ', cmd)
        cmd = re.sub("\\n|\\t|\s{2,9}", "", cmd)
        self.cursor.execute(cmd)
        self.db.commit()

        self.set_prepared_statement(TABLE_RATES_ARB_LOG, STMNT_INSERT_ARB, 7)

        self.describe(TABLE_RATES_ARB_LOG)

    def load_database(self, filepath: str):
        """Load the currency database"""

        print(f"Loading database: {filepath}")

        self.db = MySQLdb.connect(
            host="localhost",
            user="cs411arbitrage_admin",
            passwd="scatteredBrains",
            db=filepath,
        )

        self.cursor = self.db.cursor()

    def close(self, save: bool = False):
        """Close connections to database"""

        if save:
            self.db.commit()

        # Close database
        self.cursor.close()
        self.db.close()

    """ Clear & Drop Data """

    def clear(self, table: str):
        """Clear data from this table

        References:
            https://mariadb.com/kb/en/if/
        """

        print(f" Clearing table: {table}")

        # Check if table exists
        # cmd = f"SELECT 1 FROM {table} LIMIT 1;"
        # print(cmd)

        try:
            cmd = f"DELETE FROM {table};"
            self.cursor.execute(cmd)

            # Save changes
            self.db.commit()

            count = self.check_record_count(TABLE_RATES_CURRENT)

        except:
            pass

    def clear_all(self):
        """Clear updateable tables"""

        self.clear_codes()
        self.clear_latest()
        self.clear_historical()
        self.clear_arbitrage()

    def drop(self, table: str):
        """Drop table

        References:
            https://mariadb.com/kb/en/drop-table/
        """

        print(f" Dropping table: {table}")

        try:
            cmd = f"DROP TABLE {table};"
            # cmd = f"DROP {table} TABLE IF EXISTS;" # doesn't work
            print(' ', cmd)
            self.cursor.execute(cmd)

            # Save changes
            self.db.commit()

        except Exception as e:

            print(f' Error dropping table: {e}')

    def drop_all(self):
        """Drop all tables"""

        for table in [TABLE_CURRENCY_CODE, TABLE_RATES_CURRENT, TABLE_RATES_HISTORICAL, TABLE_RATES_ARB_LOG]:
            self.drop(table)

    def delete_older_date(self, table_name: str):
        """Delete data older than 15 days

        References:
            https://mariadb.com/kb/en/date_sub/
            https://benperove.com/delete-mysql-rows-older-than-date/
        """

        print(' Deleting older data')

        cmd = f"DELETE FROM {table_name} WHERE {DATE} < DATE_SUB(NOW(), INTERVAL 15 DAY);"

        print(' ', cmd)
        self.cursor.execute(cmd)

        row = self.cursor.fetchall()

        for item in row:
            print(" ", item)

        # Save changes
        self.db.commit()

    """ Clear Data: Shortcuts """

    def clear_arbitrage(self):
        """Clear the arbitrage exchange rates"""
        self.clear(TABLE_RATES_ARB_LOG)

    def clear_codes(self):
        """Clear the arbitrage exchange rates"""
        self.clear(TABLE_CURRENCY_CODE)

    def clear_historical(self):
        """Clear the historical exchange rates"""
        self.clear(TABLE_RATES_HISTORICAL)

    def clear_latest(self):
        """Clear the latest exchange rates"""
        self.clear(TABLE_RATES_CURRENT)

    """ Describe """

    def describe(self, table_name: str):
        """Get a description of the sql table"""
        print(f"Describing table: {table_name}")

        cmd = f"DESCRIBE {table_name};"
        print(' ', cmd)

        self.cursor.execute(cmd)
        row = self.cursor.fetchall()

        for item in row:
            print(" ", item)

        print("")

    def describe_all(self):
        """Describe all critical tables"""

        for table in [
            TABLE_CURRENCY_CODE,
            TABLE_RATES_CURRENT,
            TABLE_RATES_HISTORICAL,
            TABLE_RATES_ARB_LOG,
        ]:
            self.describe(table)

    """ Pull Data: Web API """

    @staticmethod
    def get_currency_codes() -> dict:
        """Grab currency code and description"""
        # TODO: grab from ISO 4217
        return {
            "USD": "United States Dollar",
            "EUR": "Euro Member Countries",
            "JPY": "Japan Yen",
            "GBP": "United Kingdom Pound",
            "AUD": "Australia Dollar",
            "CAD": "Canada Dollar",
            "CHF": "Switzerland Franc",
            "CNY": "China Yuan Renminbi",
            "HKD": "Hong Kong Dollar",
            "NZD": "New Zealand Dollar",
        }

    @staticmethod
    def get_currencies_all_api() -> List[str]:
        """Grab all available currencies from the web API"""

        data = DatabaseMethods.get_currency_data_latest()
        currencies = sorted(data["rates"].keys())

        print(" Getting all currencies")
        print("  ", currencies)

        return currencies

    @staticmethod
    def get_currency_data_historical(
        symbols: List[str] = None,
        start_at: str = None,
        end_at: str = None,
        base: str = None,
    ) -> dict:
        """Pull historical currency data from web api

        Args:
            symbols (List[str]): currency codes ie: 'USD, GBP'
            start_at (str): starting date
            end_at (str): ending date
            base (str): base currency code ie: 'EUR'
        References:
            https://exchangeratesapi.io
            https://requests.readthedocs.io/en/master/user/quickstart
        """

        flag: bool = False

        print(" Getting historical currency data")
        print("  base: {}".format(base))
        print("  symbols: {}".format(symbols))
        print("  start_at: {}".format(start_at))
        print("  end_at: {}".format(end_at))

        # Workaround for api
        if symbols and "EUR" in symbols and base == "EUR":

            flag = True

            # Remove 'EUR' from symbols temporarily
            symbols = set(symbols)
            symbols.remove("EUR")

        data = None
        response = None
        url = "https://api.exchangeratesapi.io/history"

        # GET https://api.exchangeratesapi.io/history?start_at=2018-01-01&end_at=2018-09-01&base=USD
        params = {
            "start_at": start_at,
            "end_at": end_at,
            "base": base,
            "symbols": symbols,
        }

        try:
            # Send get request
            response = requests.get(url, params)
            print("  status code: {0}".format(response.status_code))

        except ConnectionError:
            raise ConnectionError(" reason: {0}".format(response.reason))

        else:
            # Parse response with json
            data = json.loads(response.text)

            # Add euro data for workaround
            if flag:

                for key in data["rates"].keys():
                    data["rates"][key]["EUR"] = 1.0

        return data

    @staticmethod
    def get_currency_data_latest(symbols: List[str] = None, base: str = None) -> dict:
        """Pull the latest currency data from web api

        Args:
            symbols (List[str]): currency codes ie: 'USD, GBP'
            base (str): base currency code ie: 'EUR'
        References:
            https://exchangeratesapi.io
            https://requests.readthedocs.io/en/master/user/quickstart
        """

        flag: bool = False

        print(" Getting latest currency data")
        print("  base: {}".format(base))
        print("  symbols: {}".format(symbols))

        # Workaround for api
        if symbols and "EUR" in symbols and base == "EUR":

            flag = True

            # Remove 'EUR' from symbols temporarily
            symbols = set(symbols)
            symbols.remove("EUR")

        data = None
        response = None
        url = "https://api.exchangeratesapi.io/latest"

        # GET https://api.exchangeratesapi.io/latest?symbols=USD,GBP
        params = {"base": base, "symbols": symbols}

        try:
            # Send get request
            response = requests.get(url, params)
            print("  status code: {0}".format(response.status_code))

        except ConnectionError:
            raise ConnectionError(" reason: {0}".format(response.reason))

        else:
            # Parse response with json
            data = json.loads(response.text)

            # Add euro data for workaround
            if flag:
                data["rates"]["EUR"] = 1.0

        return data

    """ Pull Data: Local Database """

    def get_conversion_factor(self, base: str, target: str, date: str) -> float:
        """Get conversion factor between two currencies

        Args:
            base (str): starting currency code
            target (str): ending currency code
            date (str): date of conversion
        Returns:
            float: conversion factor
        """

        print(" Getting rate: {} base: {} target: {}".format(date, base, target))

        # Check that currency code is available
        for cur_code in [base, target]:
            if cur_code not in self.defaults[CODE]:
                raise ValueError(f"Currency code '{cur_code}' is not available")

        # Define command
        cmd = f"SELECT {RATE} FROM {TABLE_RATES_HISTORICAL} WHERE {DATE} = '{date}' AND {BASE} = '{base}' AND {TARGET} = '{target}'"
        self.cursor.execute(cmd)

        # Extract conversion factor from row
        row = self.cursor.fetchone()
        return row[0]

    def get_currencies_latest_local(self):
        """Get the latest currencies rates from the local database"""

        cmd = """SELECT DISTINCT {target} FROM {table} ORDER BY {target}""".format(
            table=TABLE_RATES_CURRENT, target=TARGET, date=DATE
        )

        arr = [row[0] for row in self.cursor.execute(cmd)]

        return arr

    def get_currencies_by_day(self, query_date: str) -> List[str]:
        """Get the currencies available for a specific day"""

        cmd = """SELECT DISTINCT {target} FROM {table} WHERE {date} ='{query_date}' ORDER BY {target}""".format(
            table=TABLE_RATES_HISTORICAL,
            target=TARGET,
            date=DATE,
            query_date=query_date,
        )

        arr = [row[0] for row in self.cursor.execute(cmd)]

        return arr

    def get_currencies_all_local(self) -> List[str]:
        """Grab all the latest target currency codes

        Returns:
            List(str): currency codes in database ie: ['EUR', 'USD', ..., 'ZWD']

        References:
            https://docs.python.org/3/library/sqlite3.html
        """

        cmd = """SELECT DISTINCT {target} FROM {table} ORDER BY {target}""".format(
            table=TABLE_RATES_CURRENT, target=TARGET
        )

        arr = [row[0] for row in self.cursor.execute(cmd)]

        return arr

    def get_date_latest_local(self) -> str:
        """Get the latest date from the local database"""

        cmd = """SELECT DISTINCT Date FROM {table}""".format(
            table=TABLE_RATES_CURRENT, date=DATE
        )

        row = self.cursor.execute(cmd)
        row = self.cursor.fetchone()

        if row is None:
            return None
        else:
            return row[0]

    """ Push Data: Local Database """

    def push_currency_codes(self, data: dict):
        """Push currency codea and description

        Args
            data (dict): currency code and description
        References:
            https://exchangeratesapi.io
        """

        print("Pushing currency codes:")

        for key in data:

            print(f"  code: {key} description: {data[key]}")

            # Define command
            cmd = f"INSERT INTO {TABLE_CURRENCY_CODE} VALUES ('{key}', '{data[key]}');"

            try:
                self.cursor.execute(cmd)

            except Exception as e:
                print(f"  error: {e}")

        # Save changes
        self.db.commit()

    def push_currency_data_historical(self, data: dict, sigs: int = None):
        """Push historical currency data pulled from API

        Args
            data (dict): historical data
            sigs (int): number of significant figures
        References:
            https://exchangeratesapi.io
        """

        print("Pushing historical data:")

        if "base" not in data:
            print(" Missing 'base' key in latest currency data")
            return

        base = data["base"]

        for date in data["rates"].keys():

            for target in data["rates"][date].keys():

                # Grab values for ease
                rate = data["rates"][date][target]

                # Round significant figures
                if sigs is not None:
                    rate = round(rate, sigs)

                print(f"  date: {date} base: {base} target: {target} rate: {rate}")

                # Define command
                cmd = f"INSERT INTO {TABLE_RATES_HISTORICAL} VALUES ('{date}', '{base}', '{target}', {rate});"

                try:
                    self.cursor.execute(cmd)

                except Exception as e:
                    print(f"  error: {e}")

        # Save changes
        self.db.commit()

    def push_currency_latest(self, data: dict, sigs: int = None):
        """Push latest currency data pulled from API

        Args
            data (dict): historical data
            sigs (int): number of significant figures
        References:
            https://exchangeratesapi.io
        """

        print(" Pushing latest data")

        if "base" not in data:
            print("  Missing 'base' key in latest currency data")
            return

        if "date" not in data:
            print("  Missing 'date' key in latest currency data")
            return

        base = data["base"]
        date = data["date"]

        for target in data["rates"].keys():

            # Grab values for ease
            rate = data["rates"][target]

            # Round significant figures
            if sigs is not None:
                rate = round(rate, sigs)

            print(f"  date: {date} base: {base} target: {target} rate: {rate}")

            # Push to current table
            cmd = f"INSERT INTO {TABLE_RATES_CURRENT} VALUES ('{date}', '{base}', '{target}', {rate});"

            try:
                self.cursor.execute(cmd)

            except Exception as e:
                print(f"  error: {e}")

        # Save changes
        self.db.commit()

    def push_arbitrage(self, data: dict):
        """Push arbitrage calculations to local database"""

        self.set_prepared_statement(TABLE_RATES_ARB_LOG, STMNT_INSERT_ARB, 7)

        # ('2020-03-27', ('AUD', 'BGN', 'BRL'))
        for key in data.keys():

            # '2020-03-27'
            date = key[0]

            # Get base and target from path
            # ('ZAR', 'USD', 'SEK')
            cur_path = key[1]
            base = cur_path[0]
            target = cur_path[-1]
            degree = len(cur_path) - 1

            # Grab intermediates
            # ('ZAR', 'USD', 'SEK') => ZAR,USD,SEK'
            int_path = cur_path[1:degree]
            int_path = " ".join(int_path)

            # Grab direct conversion
            val_dir = self.get_conversion_factor(base, target, date)

            # Grab indirect conversion
            val_ind = data[key][-1]

            # Calculate arbitrage amount
            val_arb = val_ind - val_dir

            print(f"  date: {date} path: {cur_path}")

            # Create dictionary for prepared statement
            d = {SQL_DATE: date, SQL_BASE: base, SQL_INTR: int_path, SQL_TARGET: target, SQL_DEG: degree, SQL_RATE_IND: val_ind, SQL_DELTA: val_arb}

            # old method
            # cmd = f"INSERT INTO {TABLE_RATES_ARB_LOG} VALUES ('{date}', '{base}', '{int_path}', '{target}', '{degree}', '{val_dir}', '{val_ind}');"

            try:

                # Push to current table
                self.set_sql_variables(d)
                self.execute_prepared_statement(STMNT_INSERT_ARB, self.key_order_arb)

                # TODO: remove this, if it works
                # self.cursor.execute(cmd)

            except Exception as e:
                print(f"  error: {e}")

        # Deallocate
        cmd = f"DEALLOCATE PREPARE {STMNT_INSERT_ARB};"
        self.cursor.execute(cmd)

        # Save changes
        self.db.commit()

    """ Populate Data: Local Database"""

    def populate(self):
        """Run common populate commands"""
        self.populate_latest_all(symbols=self.defaults[CODE])
        # self.populate_historical_all(symbols=self.defaults[CODE])
        self.populate_arbitrage()

        # Delete older arbitrage data
        self.delete_older_date(TABLE_RATES_ARB_LOG)

    def populate_initial(self):
        """Iniitial population of database"""
        self.populate_currency_code()
        self.populate_latest_all(symbols=self.defaults[CODE])
        # self.populate_historical_all(symbols=self.defaults[CODE])
        self.populate_arbitrage()

    def populate_arbitrage(self, date: str = None, max_degree: int = None):
        """Calculate all arbitrage for a specific day"""

        # TODO: see if combined get set can further improve run time

        # Use latest date by default
        if date is None:
            date = self.get_date_latest_local()

        if max_degree is None:
            if self.debug:
                max_degree = 2
            else:
                max_degree = self.defaults[DEG]

        count = self.check_arbitrage_record_count(date)

        # Skip arbitrage calculation if relevent
        # Correct value = 187200
        if count > 10000:
            print(" Arbitrage data exists: skipping {}".format(date))
        else:
            print(" Populating arbitrage data: {}".format(date))
            data = self.calculate_arbitrage(date, max_degree=max_degree)
            self.push_arbitrage(data)

    def populate_currency_code(self):
        """Get and set currency code data"""

        print(" Populating currency codes")

        self.clear_codes()
        data = self.get_currency_codes()
        self.push_currency_codes(data)

    def populate_historical(
        self,
        base: str = None,
        symbols: List[str] = None,
        start_at: str = None,
        end_at: str = None,
    ):
        """ Get and set historical currency data using a single base value

        References:
            https://docs.python.org/3.6/library/datetime.html#strftime-and-strptime-behavior
            https://www.guru99.com/date-time-and-datetime-classes-in-python.html#5
        """

        print(" Populating historical data: {}".format(base))

        date_today = datetime.date.today()

        # Set default start date
        if start_at is None:
            start_at = date_today.strftime("%Y-%m-%d")
            # date_prev = date_today - datetime.timedelta(days=5)
            # start_at = date_prev.strftime("%Y-%m-%d")

        # Set default end date
        if end_at is None:
            end_at = date_today.strftime("%Y-%m-%d")

        # Query information (base -> targets)
        data = self.get_currency_data_historical(
            base=base, symbols=symbols, start_at=start_at, end_at=end_at
        )

        self.push_currency_data_historical(data, self.defaults[SIGFIGS])

    def populate_historical_all(
        self, symbols: List[str] = None, start_at: str = None, end_at: str = None
    ):
        """ Get and set historical currency data using all symbols"""

        print(" Populating historical data: all")

        for base in self.defaults[CODE]:
            self.populate_historical(
                base=base, symbols=symbols, start_at=start_at, end_at=end_at
            )

    def populate_latest_default(self, symbols=None):
        """ Get and set the latest currency data using default base"""

        # Clear the table before insertion
        self.clear_latest()

        # Grab todays data and push (default base)
        data = self.get_currency_data_latest(symbols=symbols)
        self.push_currency_latest(data, self.defaults[SIGFIGS])

    def populate_latest_all(self, symbols=None):
        """ Get and set the latest currency data for all symbols"""

        # Grab the date for the latest information from the API and server
        data = self.get_currency_data_latest()

        # Check if we have records there
        count = self.check_record_count(TABLE_RATES_CURRENT, data["date"])

        # We don't have the latest data
        if count == 0:

            # Clear the table before insertion
            self.clear_latest()

            for base in self.defaults[CODE]:
                data = self.get_currency_data_latest(base=base, symbols=symbols)
                self.push_currency_latest(data, self.defaults[SIGFIGS])

                self.check_record_count(TABLE_RATES_CURRENT)
        else:
            print("CurrencyExchangeRates already up to date!")

    """ Prepared Statements """

    def execute_prepared_statement(self, name: str, order: List[str]):
        """Execute a prepared statement in this order

        Args:
            name: name of the prepared statement
            order: order of sql variables
        """

        print('Executing prepared statement')

        key_str = ', '.join(f'@{key}' for key in order)

        cmd = f"EXECUTE {name} USING {key_str};"
        print(cmd)
        self.cursor.execute(cmd)

    def set_prepared_statement(self, table_name: str, statement_name: str, n_vars: int):
        """Create a prepared statement for efficient

        References:
            https://mariadb.com/kb/en/prepare-statement/
            https://mariadb.com/kb/en/execute-statement/
            https://dev.mysql.com/doc/refman/5.7/en/sql-prepared-statements.html
            https://dev.mysql.com/doc/connector-net/en/connector-net-programming-prepared-preparing.html
        """

        var_str = ', '.join(['?'] * n_vars)

        cmd = f"""PREPARE {statement_name} FROM "INSERT INTO {table_name} VALUES ({var_str})";"""
        print(' ', cmd)
        self.cursor.execute(cmd)

    def set_sql_variables(self, d: dict):
        """Set SQL variables for later use

        Args:
            cursor: sql cursor
            d: hash of values for each statemnt key
        """

        print("Setting SQL variables")

        for key, val in d.items():
            # print(f'key: {key:<12} val:{val} type: {type(val)}')

            # String type
            if isinstance(val, str):
                cmd = f"SET @{key}='{val}';"

            # Assumes numeric
            else:
                cmd = f"SET @{key}={val};"

            print(cmd)
            self.cursor.execute(cmd)

    """ Record Check """

    def check_record_count(self, table_name: str, date: str = None) -> int:
        """Check record count on a specific day"""

        print('Checking record count')

        if date is None:
            cmd = f"SELECT COUNT(*) FROM {table_name}"
        else:
            cmd = f"SELECT COUNT(*) FROM {table_name} WHERE {DATE} = '{date}'"

        print(f' cmd: {cmd}')

        self.cursor.execute(cmd)
        row = self.cursor.fetchone()

        print(f' count: {row}')

        return row[0]

    def check_arbitrage_record_count(self, date: str) -> int:
        """Check arbitrage record count on a specific day"""

        # Define command
        cmd = f"SELECT COUNT(*) FROM {TABLE_RATES_ARB_LOG} WHERE {DATE} = '{date}'"
        self.cursor.execute(cmd)

        # Extract conversion factor from row
        row = self.cursor.fetchone()

        return row[0]

    def check_historical_record_count(self, date: str) -> int:
        """Check record count on a specific day"""

        # Define command
        cmd = f"SELECT COUNT(*) FROM {TABLE_RATES_HISTORICAL} WHERE {DATE} = '{date}'"
        self.cursor.execute(cmd)

        # Extract conversion factor from row
        row = self.cursor.fetchone()

        return row[0]

    """ Arbitrage """

    def calculate_arbitrage(
        self, date: str, currencies: List[str] = None, max_degree: int = 4
    ) -> dict:
        """Calculate all arbitrage values for the given day

        Args:
            date (str): exchange date
            currencies (List[str]): valid currencies
            max_degree (int): maximum number of intermediate currencies
        References:
            https://docs.python.org/3.6/library/itertools.html#itertools.permutations

        TODO:
            - calculate arbitrage backwards, and populate database with subarrays
            - data size may get large, may need to push and pull on fly
            - only keep the latest degree
        Notes:
            time_delta: 1055 
        """

        print(" Calculating arbitrage")

        if currencies is None:
            # currencies = self.get_currencies_by_day(date)
            currencies = self.defaults[CODE]

        data = {}

        # Go through all sizes
        for size in range(3, max_degree + 3):

            print("  Permutation size: {}".format(size))

            # Calculate all permutations
            perms = itertools.permutations(currencies, size)

            # Iterate through permutations
            # arr = ['AUD', 'BGN', 'BRL', 'CAD', 'CHF', 'CNY']
            for perm in perms:

                print("  Permutation: {}".format(perm))

                # Smaller sizes calculate
                if size <= 3:
                    arr = self.calculate_manual(perm, date)

                # Leverage previously calculated values
                else:

                    # Grab previous calculations
                    # 'AUD', 'EUR', 'GBP', 'USD' -> 'AUD', 'EUR', 'GBP'
                    perm_sub = perm[:-1]
                    arr_sub = data[(date, perm_sub)]

                    # Create dummy array
                    arr = arr_sub + [None]

                    # Grab conversion factor
                    # 'EUR', 'GBP'
                    k = self.get_conversion_factor(perm[-2], perm[-1], date)

                    # Update array
                    # 'EUR' * k
                    arr[-1] = arr[-2] * k

                # Save data
                data[(date, perm)] = arr

        return data

    def calculate_manual(
        self,
        currencies: List[str],
        date: str,
        value: float = 1.0,
        verbose: bool = False,
    ):
        """Manually calculate the arbitrage value

        Output the currency value as its converted between intermediate
        currency codes

        Args:
            currencies (List[str]): currencies
            date (str): date of conversion
            value (float): amount of money to convert
            verbose (bool): output intermediate information
        Returns
            List[float]: converted currencies values
        Notes:
            using decimal since query returns "accurate" numbers
        """

        if verbose:
            print("calculate arbitrage manual")
            print(f" date: {date}")
            print(f" value: {value}")
            print(f" currencies: {currencies}")

        n = len(currencies)  # number of currencies
        arr = [None] * n  # intermediate conversion values
        arr[0] = decimal.Decimal(value)  # starting currency value

        if n < 2:
            raise ValueError(
                "More than two currencies required for arbitrage calculation"
            )

        for ix in range(n - 1):

            # Grab the appropriate conversion factor
            k = self.get_conversion_factor(currencies[ix], currencies[ix + 1], date)

            # print(f' arr: {arr}  k: {k} type: {type(k)}')

            # Convert value to current currency
            arr[ix + 1] = arr[ix] * k

        if verbose:
            print(f" results: {arr}")

        return arr


# TODO: add external log capability for script

if __name__ == "__main__":

    time_start = time.time()

    db = DatabaseMethods(debug=False)
    db.close(save=True)

    time_delta = time.time() - time_start
    print(f"time delta: {time_delta:.6f}")
