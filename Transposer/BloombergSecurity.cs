using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace Transposer
{
    public class BloombergSecurity
    {
        public DataRow SecurityData { get; set; }
        public DataGridViewRow DataGridRow { get; set; }
        public List<string> SecurityFields = new List<string>();
        public string Name { get; private set; }
        public string Ticker { get; private set; }

        private double _ask;
        private double _bid;
        private double _mid;

        public double Ask
        {
            get { return _ask; }
            private set
            {
                _ask = value;
                Mid = (_bid + _ask)/2;
            }
        }
        public double Bid
        {
            get { return _bid; }
            private set
            {
                _bid = value;
                Mid = (_bid + _ask)/2;
            }
        }
        public double Mid
        {
            get { return _mid; }
            private set
            {
                _mid = value;
                UpdateMid();
            }
        }

        public BloombergSecurity(string name, string bbTicker)
        {
            Name = name;
            Ticker = bbTicker;
        }

        public BloombergSecurity(DataGridViewRow dataGridrow, DataRow dataRow, List<string> securityField)
        {
            DataGridRow = dataGridrow;
            SecurityData = dataRow;
            Name = dataRow[1].ToString();
            Ticker = dataRow[0].ToString();
            SecurityFields = securityField;

        }

        public void Setfield(String field, string value)
        {
            Type type = SecurityData[field].GetType();
            //Type propType = SecurityData[field].PropertyType;
            var converter = TypeDescriptor.GetConverter(type);

            SecurityData[field] = value; //= converter.ConvertFrom(value);
            if (field == "Bid") Bid = Double.Parse(value);
            if (field == "Ask") Ask = Double.Parse(value);
        }

        public void SetDelayedStream()
        {
            foreach (DataGridViewCell cell in DataGridRow.Cells)
                cell.Style.BackColor = Color.Yellow;
        }

        private void UpdateMid()
        {
            SecurityData["Mid"] = _mid;
        }
    }
}
