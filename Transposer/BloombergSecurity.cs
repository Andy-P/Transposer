using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Transposer
{
    public class BloombergSecurity
    {
        public DataRow SecurityData { get; set; }
        public DataGridViewRow DataGridRow { get; set; }
        public List<string> SecurityFields = new List<string>();
        private readonly Form _parent;
        public string Name { get; private set; }
        public string Ticker { get; private set; }

        private DateTime LastBackColorChg = DateTime.Now;
        private bool backColorNeedsCheck = false;
        private Color DefltBackgroundColor;
        private double HighlightTimeInMillisecs = 3000;

        private double _ask;
        private bool _prevAskInit;
        private double _bid;
        private bool _prevBidInit;
        private double _mid;
        private double _prevMid;
        private bool _prevMidInit;
        private double _chg;

        public double Ask
        {
            get { return _ask; }
            private set
            {
                if (!_prevAskInit)
                    _prevAskInit = true;

                _ask = Math.Round(value, 6);

                if ((_prevBidInit) && (_prevAskInit))
                    Mid = (_bid + _ask)/2;
            }
        }

        public double Bid
        {
            get { return _bid; }
            private set
            {
                if (!_prevBidInit)
                    _prevBidInit = true;

                _bid = Math.Round(value, 6); ;

                if ((_prevBidInit) && (_prevAskInit))
                    Mid = (_bid + _ask)/2;
            }
        }

        public double Mid
        {
            get { return _mid; }
            private set
            {
                if (_prevMidInit)
                {
                    PrevMid = _mid;
                }
                else
                {
                    PrevMid = value;
                    _prevMidInit = true;
                }
                _mid = value;
                UpdateMid();
                Change = _mid - PrevMid;
            }
        }

        public double PrevMid
        {
            get { return _prevMid; }
            private set { _prevMid = value; }
        }

        public double Change
        {
            get { return _chg; }
            private set {
                if (value != 0)
                {
                    _chg = Math.Round(value, 5); 
                    UpdateChange();
                }
            }
        }
        
        public BloombergSecurity(DataGridViewRow dataGridrow, DataRow dataRow, List<string> securityField, Form parent)
        {
            DataGridRow = dataGridrow;
            SecurityData = dataRow;
            Name = dataRow[1].ToString();
            Ticker = dataRow[0].ToString();
            SecurityFields = securityField;
            _parent = parent;

            DefltBackgroundColor = DataGridRow.DefaultCellStyle.BackColor;

        }

        public void IntiTimer(Timer timer1)
        {
            //Instantiate the timer
            timer1.Tick += new EventHandler(timer1_Tick);
        }

        public void Setfield(String field, string value)
        {
            SecurityData[field] = value;
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
            SecurityData["Mid"] = Mid;
        }

        private void UpdateChange()
        {
            SecurityData["Change"] = Change;
            if (Change < 0)
            {
                HighlightBackColor(true);
            }
            else
            {
                if (Change > 0)
                {
                    HighlightBackColor(false);
                }
            }
        }

        private void HighlightBackColor(bool isUp)
        {
            if (isUp)
            {
                DataGridRow.DefaultCellStyle.BackColor = Color.LightGreen;
            }
            else
            {
                DataGridRow.DefaultCellStyle.BackColor = Color.PaleVioletRed;
            }
            LastBackColorChg = DateTime.Now;
            backColorNeedsCheck = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (backColorNeedsCheck)
            {

                //Console.WriteLine("{0} needs check",Name);
                TimeSpan timeSinceChange = DateTime.Now - LastBackColorChg;
                if (timeSinceChange.Milliseconds >= HighlightTimeInMillisecs)
                {
                    backColorNeedsCheck = false;
                    DataGridRow.DefaultCellStyle.BackColor = DefltBackgroundColor;
                }
                
            }
        }
    }
}
