using System;

namespace CurrencyArbitrageTrading.Models
{
    public class DegreeKeyValue
    {
        public int Key { get; set; }
        public string Value { get; set; }

        public DegreeKeyValue()
        {

        }

        public override string ToString()
        {
            return "Degree " + this.Key.ToString();
        }
    }
}