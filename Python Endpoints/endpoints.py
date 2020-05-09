from flask import Flask
import simplejson as json
from flask import request
import MySQLdb
app = Flask(__name__)

#db=MySQLdb.connect(host="localhost",user="cs411arbitrage_admin",passwd="scatteredBrains",db="cs411arbitrage_testDb_03282020")
db=MySQLdb.connect(host="localhost",user="cs411arbitrage_admin",passwd="scatteredBrains",db="cs411arbitrage_ServerTest")

@app.route("/GetCurrencyCodes")
def hello():
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("""SELECT CurrencyCode, CurrencyDescription FROM CurrencyCodes""")
        data = cursor.fetchall()
    return (json.dumps(data,indent=4))
    #return ("{\"content\":"+json.dumps(data,indent=4)+"}")
    #return "{\"output\":\"hulloWorld\"}"

@app.route("/GetCurrencyExchangeRates")
def getHist():
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates")
        data = cursor.fetchall()
        #results = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"].strftime("%Y-%m-%d"), 'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"], 'ExchangeRate': result["ExchangeRate"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4))
        #return "{\"output\":\"hulloWorld1\"}"
        
@app.route("/GetLatestCurrencyExchangeRates")
def getLatest():
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("SELECT Date FROM CurrencyExchangeRates ORDER BY DATE(Date) DESC LIMIT 1")
        data = cursor.fetchall()
        date = data[0]["Date"].strftime("%Y-%m-%d")
        cursor.execute("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates WHERE Date=\""+date+"\"")
        data = cursor.fetchall()
        #results = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"].strftime("%Y-%m-%d"), 'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"], 'ExchangeRate': result["ExchangeRate"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4))        
    
    
@app.route("/GetHistoricCurrencyExchangeRates")
def getArchive():
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRatesArchive")
        data = cursor.fetchall()
        #results = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"].strftime("%Y-%m-%d"), 'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"], 'ExchangeRate': result["ExchangeRate"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4))
    

@app.route("/getCurrencyDescription/<string:currencyCode>")
def getCurrencyDescription(currencyCode):
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        que= "SELECT CurrencyDescription FROM CurrencyCodes WHERE CurrencyCode=\""+currencyCode+"\""
        cursor.execute (que)
        data = cursor.fetchall()
    return (json.dumps(data,indent=4))
    
@app.route("/SelectHistoricCurrencyExchangeRates")
def historicByBaseTarget():
    baseCode = request.args.get('baseCode')
    targetCode = request.args.get('targetCode')
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRatesArchive WHERE BaseCurrencyCode=\""+baseCode+"\" AND TargetCurrencyCode=\""+targetCode+"\"")
        data = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"].strftime("%Y-%m-%d"), 'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"], 'ExchangeRate': result["ExchangeRate"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4))

@app.route("/SelectCurrencyExchangeRates")
def exchangeByBaseTarget():
    baseCode = request.args.get('baseCode')
    targetCode = request.args.get('targetCode')
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates WHERE BaseCurrencyCode=\""+baseCode+"\" AND TargetCurrencyCode=\""+targetCode+"\"")
        data = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"].strftime("%Y-%m-%d"), 'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"], 'ExchangeRate': result["ExchangeRate"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4))
    
@app.route("/SelectHistoricCurrencyExchangeRatesByDate")
def historicByDate():
    date = request.args.get('date')
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRatesArchive WHERE Date=\""+date+"\"")
        data = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"].strftime("%Y-%m-%d"), 'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"], 'ExchangeRate': result["ExchangeRate"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4))
    #return "{\"output\":\"hulloWorld1\"}"
    
@app.route("/SelectCurrencyExchangeRatesByDate")
def exchangeByDate():
    date = request.args.get('date')
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        cursor.execute("SELECT Date, BaseCurrencyCode, TargetCurrencyCode, ExchangeRate FROM CurrencyExchangeRates WHERE Date=\""+date+"\"")
        data = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"].strftime("%Y-%m-%d"), 'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"], 'ExchangeRate': result["ExchangeRate"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4))
    #return "{\"output\":\"hulloWorld1\"}"

@app.route("/SelectCurrencyArbitrageLog")
def selectArbitrage():
    baseCode = request.args.get('baseCode')
    targetCode = request.args.get('targetCode')
    degree = request.args.get('degree')
    sortBy = request.args.get('sortBy',default = '', type = str)
    with db.cursor(MySQLdb.cursors.DictCursor) as cursor:
        #cursor.execute("SELECT Date FROM CurrencyExchangeRates ORDER BY DATE(Date) DESC LIMIT 1")
        cursor.execute("SELECT Date FROM CurrencyArbitrageLog ORDER BY DATE(Date) DESC LIMIT 1")
        data = cursor.fetchall()
        date = data[0]["Date"].strftime("%Y-%m-%d")
        if sortBy=="code":
            cursor.execute("CALL selectArbitrageLogView('IntermediateCurrencyCodes','"+targetCode+"','"+baseCode+"','"+date+"',"+degree+");")
        else:
            cursor.execute("CALL selectArbitrageLogView('ActualValue','"+targetCode+"','"+baseCode+"','"+date+"',"+degree+");")
            
        data = cursor.fetchall()
        json_data=[]
        content={}
        for result in data:
            content={'Date':result["Date"],'BaseCurrencyCode':result["BaseCurrencyCode"], 'TargetCurrencyCode':result["TargetCurrencyCode"],'IntermediateCurrencyCodes':result["IntermediateCurrencyCodes"],'Degree':result["Degree"],'ImpliedValue':result["ImpliedValue"],'ActualValue':result["ActualValue"]}
            json_data.append(content)
            content={}
    return (json.dumps(json_data,indent=4)) 
    
    
if __name__ == "__main__":
    app.run(debug=True)
    
    
