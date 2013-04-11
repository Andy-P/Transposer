using System;
using System.Collections.Generic;
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
            
        }

        public void SetDelayedStream()
        {
            foreach (DataGridViewCell cell in DataGridRow.Cells)
                cell.Style.BackColor = Color.Yellow;
        }
    }
}
