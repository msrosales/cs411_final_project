# import MySQLdb
import matplotlib as mpl
from matplotlib import pyplot as plt
import numpy as np
import pandas as pd
import seaborn as sns

mpl.style.use("ggplot")

# Table names
TABLE_CURRENCY_CODE = "CurrencyCodes"
TABLE_RATES_CURRENT = "CurrencyExchangeRates"
TABLE_RATES_HISTORICAL = "CurrencyExchangeRatesArchive"
TABLE_RATES_ARB_LOG = "CurrencyArbitrageLog"

# Column names
DATE = "Date"

# Sizing names
DAYS = 'Days'
NAME = 'Table Name'
ROWS = 'Row Count'
SIZE = 'Table Size'
SIZE_PER_ROW = 'Size per row'
SIZE_PER_DAY = 'Size per day'

# SQL data optimization
ARB_LIMIT = 'Arbitrage Day Limit'
MAX_DEG = 'Maximum Degree'
N_CUR = 'Number of Currencies'

# SQL data types
LENGTH = 'Length'
INTEGER = 'INTEGER'
DECIMAL = 'DECIMAL'
VAR_CHAR = 'VARCHAR'


''' Load Database '''


def load_database():

    filepath: str = "cs411arbitrage_ServerTest"
    print(f"Loading database: {filepath}")

    db = MySQLdb.connect(
        host="localhost",
        user="cs411arbitrage_admin",
        passwd="scatteredBrains",
        db=filepath,
    )

    cursor = db.cursor()
    return cursor


''' Table Sizes: Manual '''


def get_row_estimate(data: dict) -> float:
    """Get the row estimates for a data type

    Args:
        data (dict): length for each data type
    Returns:
        float: number of bytes per row
    References:
        https://www.w3schools.com/sql/sql_datatypes.asp
    """

    sizes = {VAR_CHAR: None, INTEGER: 4, DECIMAL: 17, DATE: 8}

    n_bytes = 0

    for key in data:

        # Add bytes dependent on a variable input
        if key == VAR_CHAR:
            n_bytes += (2 + data[VAR_CHAR])

        # Add bytes for this data type
        else:
            n_bytes += sizes[key]

    return n_bytes


''' Table Sizes: Estimated from Maria DB '''


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
        data[table_name]['Days'] = len(dates)


    return data


def set_table_sizes() -> dict:
    """Use database server query information to define local
    dictionary for debugging
    """

    data = {}

    data[TABLE_RATES_ARB_LOG] = {NAME: TABLE_RATES_ARB_LOG, ROWS: 1551791, SIZE: 580480000.0, DAYS: 8}
    data[TABLE_CURRENCY_CODE] = {NAME: TABLE_CURRENCY_CODE, ROWS: 10, SIZE: 20000.0}
    data[TABLE_RATES_CURRENT] = {NAME: TABLE_RATES_CURRENT, ROWS: 100, SIZE: 50000.0}
    data[TABLE_RATES_HISTORICAL] = {NAME: TABLE_RATES_HISTORICAL, ROWS: 3800, SIZE: 730000.0, DAYS: 41}

    # Compute size per row, and size per day
    for table in data:

        data[table][SIZE_PER_ROW] = data[table][SIZE] / data[table][ROWS]

        if DAYS in data[table]:
            data[table][SIZE_PER_DAY] = data[table][SIZE] / data[table][DAYS]

    return data


def get_database_size(n_days: int, max_degree: int, n_cur: int, n_arb: int):
    """Estimate the databse size after n days

    Args:
        n_days (int): number of days database has been running
        max_degree (int): maximum degree for the arbitrage calculation
        n_cur (int): number of currencies
        n_arb (int): number of days to maintain in the arbitrage log
    Returns:
        float: database size in bytes
    """

    # Grab data sizes from table
    data = set_table_sizes()

    # Compute the base size
    # Currency code, and current table size stays constant
    # ie: size per row * number of rows
    size_codes = data[TABLE_CURRENCY_CODE][SIZE_PER_ROW] * n_cur
    size_current = data[TABLE_RATES_CURRENT][SIZE_PER_ROW] * (n_cur * n_cur)
    size_hist = size_current * n_days

    # Compute arbitrage for each degree up to max degree
    size_arb = 0
    for degree in range(2, max_degree + 1):

        # Compute number of rows
        rows_i = np.math.factorial(n_cur) / (np.math.factorial(n_cur - degree - 1))
        size_arb += rows_i * data[TABLE_RATES_ARB_LOG][SIZE_PER_ROW]

    # We haven't hit the arbitrage limit
    if n_days < n_arb:
        size_arb *= n_days

    # We've hit the limit and only holding x days
    else:
        size_arb *= n_arb

    total = size_codes + size_current + size_hist + size_arb

    return total


''' Analysis '''


def plot_table_size(data: dict, n_days: int, n_days_trim_arb: int, capacity: int):
    """Plot an expected trend for table size using current defaults

    Args:
        data (dict):
        n_days_plot (int): number of days to plot
        n_days_trim_arb (int): number of days to maintain in arbitrage log
        capacity (int): server capacity in bytes
    """

    # Compute the base size
    # Currency code, and current table size stays constant
    size_start = data[TABLE_CURRENCY_CODE][SIZE] + data[TABLE_RATES_CURRENT][SIZE]

    # Generate arrays
    sizes = []
    days = np.arange(1, n_days + 1)

    for day_ix in days:

        size_i = size_start

        ''' Compute arbitrage size '''
        if day_ix < n_days_trim_arb:
            size_i += day_ix * data[TABLE_RATES_ARB_LOG][SIZE_PER_DAY]

        # Size only contains number of kept days
        else:
            size_i += n_days_trim_arb * data[TABLE_RATES_ARB_LOG][SIZE_PER_DAY]

        ''' Compute historical size '''
        size_i += day_ix * data[TABLE_RATES_HISTORICAL][SIZE_PER_DAY]

        # Save the size
        sizes.append(size_i)

    # Create the figure
    # Grab axes
    fig = plt.figure()
    axes = plt.gca()

    # Plot the size
    axes.plot(days, sizes)

    if capacity:
        axes.axhline(capacity, color='firebrick', label='Server Capacity')

    # Set labels
    axes.set_xlabel('Number of days')
    axes.set_ylabel('Database size, s [bytes]')

    # Set grid
    axes.grid(which='minor', color='grey', ls='--')
    axes.grid(which='major', color='grey', ls='--')

    plt.show()


def plot_table_size2(n_days: int, n_arb: int, capacity: int):
    """Plot an expected trends for estimated table size

    Args:
        data (dict):
        n_days_plot (int): number of days to plot
        n_arb (int): number of days to maintain in arbitrage log
        capacity (int): server capacity in bytes
    """

    # Generate arrays
    sizes = []
    days = np.arange(1, n_days + 1)

    for day_ix in days:

        # Save the size
        size_i = get_database_size(day_ix, max_degree=5, n_cur=10, n_arb=n_arb)
        sizes.append(size_i)

    # Create the figure
    # Grab axes
    fig = plt.figure()
    axes = plt.gca()

    # Plot the size
    axes.plot(days, sizes)

    if capacity:
        axes.axhline(capacity, color='firebrick', label='Server Capacity')

    # Set labels
    axes.set_xlabel('Number of days')
    axes.set_ylabel('Database size, s [bytes]')

    # Set grid
    axes.grid(which='minor', color='grey', ls='--')
    axes.grid(which='major', color='grey', ls='--')

    plt.show()


def database_optimization():
    """Heatmap of degrees and optimization"""

    # Table bounds
    currencies = np.arange(10, 35, 5)  # number of currencies
    degrees = np.arange(2, 8)  # maximum degree
    n_arb: int = 15  # days to keep for arbitrage log
    day_ix: int = 30  # which day to analyze
    style: str = 'seaborn'

    # Create an empty grid
    # data = np.random.rand(len(degrees), len(currencies))
    grid = np.zeros((len(degrees), len(currencies)))

    db = {N_CUR: [], MAX_DEG: [], SIZE: [], ARB_LIMIT: []}

    # For each grid input
    for row_ix in range(len(degrees)):
        for col_ix in range(len(currencies)):

            # Convert bytes to Gb
            size_i = get_database_size(day_ix, degrees[row_ix], currencies[col_ix], n_arb) / 10**9
            grid[row_ix][col_ix] = size_i

            # Save into df
            db[N_CUR].append(currencies[col_ix])
            db[MAX_DEG].append(degrees[row_ix])
            db[SIZE].append(size_i)
            db[ARB_LIMIT].append(day_ix)

    # Select plot style
    if style == 'seaborn':

        # Convert to dataframe for ease
        df = pd.DataFrame(db)
        df = df.pivot(MAX_DEG, N_CUR, SIZE)
        # df.to_csv('output.csv')

        sns.heatmap(df, annot=True, linewidths=1.0, vmin=0.0, vmax=2.0)
        plt.show()

    elif style == 'matplotlib':

        # Create the figure
        fig = plt.figure()

        # Grab axes
        axes = plt.gca()

        # Plot the matrix
        cax = axes.imshow(
            grid, cmap="jet", interpolation="none", aspect="auto", vmin=0, vmax=2.0
        )

        # Add color bar
        fig.colorbar(cax)

        # Add extras
        axes.set_xticks(range(len(currencies)))
        axes.set_yticks(range(len(degrees)))
        # axes.set_aspect('equal')

        # Axes Labels
        axes.set_xticklabels(currencies, fontsize=15)
        axes.set_yticklabels(degrees, fontsize=15)

        axes.set_xlabel('Number of Currencies')
        axes.set_ylabel('Maximum Degree')

        axes.set_title(f"Database Size on Day {day_ix}", fontsize=10, fontweight="bold")

        # Show me the plots!
        # plt.axis('equal')
        plt.show()


if __name__ == '__main__':

    # Default settings plot estimate
    # data = set_table_sizes()
    # plot_table_size(data, 60, 15, capacity=583.34*10**6)

    # Generalized data base estimates
    # plot_table_size2(60, 15, capacity=583.34*10**6)

    database_optimization()
