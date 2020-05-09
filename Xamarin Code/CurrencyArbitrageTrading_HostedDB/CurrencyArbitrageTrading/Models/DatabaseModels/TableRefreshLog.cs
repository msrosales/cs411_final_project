using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyArbitrageTrading.Models.DatabaseModels
{
    public class TableRefreshLog
    {
        public string TableName { get; set; }
        public string RefreshDate { get; set; }

        public TableRefreshLog()
        {

        }

        public override string ToString()
        {
            return this.TableName + " - " + this.RefreshDate;
        }
    }
}
