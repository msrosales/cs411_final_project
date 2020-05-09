import MySQLdb
import re
import random
import string

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
INTER = "IntermediateCurrencyCodes"
VAL_IMP = "ImpliedValue"
VAL_ACT = "ActualValue"


def describe(cursor, table_name: str):
    """Get a description of the sql table"""
    print(f"Describing table: {table_name}")

    cmd = f"DESCRIBE {table_name};"
    print(' ', cmd)

    cursor.execute(cmd)
    row = cursor.fetchall()

    for item in row:
        print(" ", item)

    print("")


def delete(table_name: str, date: str = None):

    if date is None:
        cmd = f"DELETE FROM {table_name};"
    else:
        cmd = f"DELETE FROM {table_name} WHERE Date = '{date}';"

    print(' ', cmd)
    cursor.execute(cmd)


def delete_older_date(table_name: str, days: int):
    """Delete data older than a month

    References:
        https://mariadb.com/kb/en/date_sub/
        https://benperove.com/delete-mysql-rows-older-than-date/
    """

    print(' Deleting older data')

    cmd = f"DELETE FROM {table_name} WHERE {DATE} < DATE_SUB(NOW(), INTERVAL {days} DAY);"

    print(' ', cmd)
    cursor.execute(cmd)

    row = cursor.fetchall()

    for item in row:
        print(" ", item)

    # Save changes
    db.commit()


def alter_table(cursor, table_name):

    describe(cursor, table_name)

    print(f" Creating table: {table_name}")
    cmd = """ALTER TABLE {table} MODIFY {date} DATE NOT NULL;""".format(
        table=table_name,
        date=DATE,
    )

    print(' ', cmd)
    # cmd = re.sub("\\n|\\t|\s{2,9}", "", cmd)
    cursor.execute(cmd)
    db.commit()

    describe(cursor, table_name)


def set_trigger(cusor):
    print(f" Creating trigger: {TABLE_RATES_HISTORICAL}")
    cmd = f"""CREATE TRIGGER ARCHIVE_EXCHANGE_RATES
                AFTER
                    INSERT ON {TABLE_RATES_CURRENT} for each row
                BEGIN
                    INSERT into {TABLE_RATES_HISTORICAL}(
                        {DATE},
                        {BASE},
                        {TARGET},
                        {RATE}
                    )
                    values(
                        new.{DATE},
                        new.{BASE},
                        new.{TARGET},
                        new.{RATE}
                        );
                END;"""

    print(' ', cmd)
    cmd = re.sub("\\n|\\t", "", cmd)
    cmd = re.sub("\s{2,9}", " ", cmd)
    cursor.execute(cmd)
    db.commit()


def show_trigger(cursor, db_name):

    print(f"Showing trigger: {db_name}")

    cmd = f"SHOW TRIGGERS FROM {db_name};"
    print(' ', cmd)

    cursor.execute(cmd)
    row = cursor.fetchall()

    for item in row:
        print(" ", item)

    print("")


def get_dates(cursor, table_name, show: bool = False) -> str:
    """Get the latest date from the local database"""

    cmd = """SELECT DISTINCT {date} FROM {table}""".format(table=table_name, date=DATE)

    print(cmd)

    cursor.execute(cmd)
    row = cursor.fetchall()

    dates = []

    for item in row:
        # print(item, type(item))
        item = item[0].strftime("%Y-%m-%d")

        if show:
            print(f" {item}")
        dates.append(item)

    return dates


def get_date_latest_local(cursor) -> str:
    """Get the latest date from the local database"""

    cmd = """SELECT DISTINCT Date FROM {table}""".format(
        table=TABLE_RATES_CURRENT, date=DATE
    )

    print(cmd)

    row = cursor.execute(cmd)
    row = cursor.fetchone()

    print(row)

    return row[0]


def check_record_count(table_name: str, date: str = None) -> int:
    """Check record count on a specific day"""

    if date is None:
        cmd = f"SELECT COUNT(*) FROM {table_name}"
    else:
        cmd = f"SELECT COUNT(*) FROM {table_name} WHERE {DATE} = '{date}'"

    print(cmd)

    cursor.execute(cmd)
    row = cursor.fetchone()

    print(row)

    return row[0]


def get_counts_for_dates(cursor):

    dates = get_dates(cursor, TABLE_RATES_HISTORICAL, show=False)

    for date in dates:
        print(f"{date}")
        count = check_record_count(date, TABLE_RATES_HISTORICAL)


def select_older_date(cursor, table_name: str):
    """Delete data older than a month

    References:
        https://mariadb.com/kb/en/date_sub/
        https://benperove.com/delete-mysql-rows-older-than-date/
    """

    # cmd = f"DELETE FROM {table_name} WHERE {DATE} < DATE_SUB(NOW(), INTERVAL 1 MONTH);"
    cmd = f"SELECT * FROM {table_name} WHERE {DATE} < DATE_SUB(NOW(), INTERVAL 6 DAY);"

    print(' ', cmd)
    cursor.execute(cmd)

    row = cursor.fetchall()

    for item in row:
        print(" ", item)


def select_all_from(table_name: str, date: str = None):

    if date is None:
        cmd = f"SELECT * FROM {table_name};"
    else:
        cmd = f"SELECT * FROM {table_name} WHERE Date = '{date}';"

    print(' ', cmd)
    cursor.execute(cmd)
    row = cursor.fetchall()

    for item in row:
        print(item)


''' Prepared Statements '''


def prepare_statement(cursor, table_name: str, statement_name: str, n_vars: int):
    """Create a prepared statement for efficient

    References:
        https://mariadb.com/kb/en/prepare-statement/
        https://mariadb.com/kb/en/execute-statement/
        https://dev.mysql.com/doc/connector-net/en/connector-net-programming-prepared-preparing.html
    """

    var_str = ', '.join(['?'] * n_vars) 

    # cmd = f"""PREPARE {statement_name} FROM "INSERT INTO CurrencyExchangeRatesArchive (@in_date, @in_base, @in_target, @in_rate)";"""
    # cmd = f"""PREPARE {statement_name} FROM "INSERT INTO CurrencyExchangeRatesArchive VALUES (?, ?, ?, ?)";"""
    cmd = f"""PREPARE {statement_name} FROM "INSERT INTO {table_name} VALUES ({var_str})";"""
    print(' ', cmd)
    cursor.execute(cmd)


def randomword(length):
   letters = string.ascii_lowercase
   return ''.join(random.choice(letters) for i in range(length))


def execute_statement(cursor, statement_name: str):

    cmd = "SET @in_date='2020-01-01';"
    print(' ', cmd)
    cursor.execute(cmd)

    cmd = "SET @in_base='EUR';"
    print(' ', cmd)
    cursor.execute(cmd)

    cmd = f"SET @in_inter='{randomword(6)}';"
    print(' ', cmd)
    cursor.execute(cmd)

    cmd = "SET @in_target='USD';"
    print(' ', cmd)
    cursor.execute(cmd)

    cmd = f"SET @in_deg={random.randrange(10)};"
    print(' ', cmd)
    cursor.execute(cmd)

    cmd = f"SET @in_rate_dir={random.random()};"
    print(' ', cmd)
    cursor.execute(cmd)

    cmd = f"SET @in_rate_ind={random.random()};"
    print(' ', cmd)
    cursor.execute(cmd)

    # cmd = f"EXECUTE {statement_name};"
    # cmd = f"EXECUTE {statement_name} USING @in_date, @in_base, @in_target, @in_rate;"
    cmd = f"EXECUTE {statement_name} USING @in_date, @in_base, @in_inter, @in_target, @in_deg, @in_rate_dir, @in_rate_ind;"
    print(' ', cmd)
    cursor.execute(cmd)


def set_sql_variables(cursor, d: dict):
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
        cursor.execute(cmd)


def execute_prepared_statement(cursor, name: str, order: List[str]):
    """Execute a prepared statement in this order

    Args:
        cursor: sql cursor
        name: name of the prepared statement
        order: statement key order
    """

    print('Executing prepared statement')

    key_str = ', '.join(f'@{key}' for key in order)

    cmd = f"EXECUTE {name} USING {key_str};"
    print(cmd)
    cursor.execute(cmd)


''' Table Size Queries '''


def get_table_sizes(cursor, database: str) -> dict:
    """Get table sizes for a given database
    Args:
        database (str): name of the sql database
    Returns:
        dict: size of each table in the database
    """

    print(f'Getting table sizes: {database}')

    data = {}

    # Define and execute the command
    cmd = f"""SELECT table_name " Table Name" , table_rows " Rows Count" , round(((data_length + index_length)/1024/1024),2) " Table Size (MB)"  FROM information_schema.TABLES WHERE table_schema = "{database}";"""
    cursor.execute(cmd)

    # Grab the results
    rows = cursor.fetchall()

    for row_ix, row in enumerate(rows):

        print(" ", row_ix, row)

        # Rename for ease
        table_name, n_rows, size = row

        if size is None:
            continue

        # Save the results
        data[table_name] = {'Table Name': table_name, 'Row Count': n_rows, 'Table Size': float(size) * 10**6}

    print("")

    # Grab number of dates for each
    for table_name in [TABLE_RATES_HISTORICAL, TABLE_RATES_ARB_LOG]:
        dates = get_dates(cursor, table_name)

        data[table_name]['Days'] = len(dates)

    return data


''' Load Database '''

# filepath: str = "cs411arbitrage_testAdvanced"
filepath: str = "cs411arbitrage_ServerTest"
print(f"Loading database: {filepath}")

db = MySQLdb.connect(
    host="localhost",
    user="cs411arbitrage_admin",
    passwd="scatteredBrains",
    db=filepath,
)

cursor = db.cursor()

# Grab table sizes
data = get_table_sizes(cursor, filepath)

for key in data:
    print(data[key])


# date = "2020-04-09"
# count = check_record_count(TABLE_RATES_CURRENT)
# count = check_record_count(TABLE_RATES_HISTORICAL, date)

# count = check_record_count(TABLE_RATES_ARB_LOG)
# delete_older_date(TABLE_RATES_ARB_LOG, days=15)
# count = check_record_count(TABLE_RATES_ARB_LOG)


# Create prepare
# select_all_from(TABLE_RATES_ARB_LOG, '2020-01-01')
# statement_name = 'InsertArbitrage'
# prepare_statement(cursor, TABLE_RATES_ARB_LOG, statement_name, 7)

# db.commit()
# cursor.close()
# db.close()

quit()

# Set variables and use
order = ['in_date', 'in_base', 'in_inter', 'in_target', 'in_deg', 'in_rate_dir', 'in_rate_ind']

for _ in range(5):

    d = {'in_date': '2020-01-01',
    'in_base': 'EUR',
    'in_inter': f'{randomword(6)}',
    'in_target': 'USD',
    'in_deg': random.randrange(10),
    'in_rate_dir': random.random(),
    'in_rate_ind': random.random(),
    }

    set_sql_variables(cursor, d)
    execute_prepared_statement(cursor, statement_name, order)

select_all_from(TABLE_RATES_ARB_LOG, '2020-01-01')

# db.commit()
# cursor.close()
# db.close()

# select_all_from(TABLE_RATES_ARB_LOG, '2020-01-01')

# Add trigger to historical
# set_trigger(cursor)
# show_trigger(cursor, filepath)

# select_older_date(cursor, TABLE_RATES_HISTORICAL)

# date_latest = get_date_latest_local(cursor)
# count = check_record_count(date_latest, TABLE_RATES_CURRENT)
# count = check_record_count(date_latest, TABLE_RATES_HISTORICAL)
# count = check_record_count(date_latest, TABLE_RATES_ARB_LOG)

# Alter tables
# alter_table(cursor, TABLE_RATES_CURRENT)
# alter_table(cursor, TABLE_RATES_HISTORICAL)
# alter_table(cursor, TABLE_RATES_ARB_LOG)
