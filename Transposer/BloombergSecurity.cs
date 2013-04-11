using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Transposer
{
    public class BloombergSecurity
    {
        public DataRow SecurityData { get; private set; }
        public string Name { get; private set; }
        public string Ticker { get; private set; }

        public BloombergSecurity(string name, string bbTicker)
        {
            Name = name;
            Ticker = bbTicker;
        }

        public BloombergSecurity(DataRow dataRow)
        {
            SecurityData = dataRow;
            Name = dataRow[1].ToString();
            Ticker = dataRow[0].ToString();
        }
    }
}
