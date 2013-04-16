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
        public string Name { get; private set; }
        public string Ticker { get; private set; }

        private List<TransposedSecurity> _transposedSecurity = new List<TransposedSecurity>();
        private DateTime _lastBackColorChg = DateTime.Now;
        private bool _backColorNeedsCheck;

        public int HighlightTimeInSecs
        {
            get { return _highlightTimeInSecs; }
            set
            {
                if (value > 1)
                    _highlightTimeInSecs = value;
                else
                    _highlightTimeInSecs = 4;
            }
        }

        public double HighlightTimeInsecs = 4;

        private double _ask;
        private bool _prevAskInit;
        private double _bid;
        private bool _prevBidInit;
        private double _mid;
        protected bool MidInit;
        private bool _prevMidInit;
        private double _chg;
        private double _prevMid;
        private int _highlightTimeInSecs;

        public double Ask
        {
            get { return _ask; }
            private set
            {
                if (!_prevAskInit)
                    _prevAskInit = true;

                _ask = Math.Round(value, 6);

                lock (this)
                {
                    if ((_prevBidInit) && (_prevAskInit))
                    {
                        double tempMid = Math.Round((_bid + _ask)/2, 6);
                        if (tempMid != Mid) Mid = tempMid;
                    }
                }
            }
        }

        public double Bid
        {
            get { return _bid; }
            private set
            {
                if (!_prevBidInit)
                    _prevBidInit = true;

                _bid = Math.Round(value, 6);
                ;

                lock (this)
                {
                    if ((_prevBidInit) && (_prevAskInit))
                    {
                        double tempMid = Math.Round((_bid + _ask)/2, 6);
                        if (tempMid != Mid) Mid = tempMid;
                    }
                }
            }
        }

        public virtual double Mid
        {
            get { return _mid; }
            set
            {
                if (_prevMidInit)
                {
                    // if the new value for mid is different from the prev mid
                    if (Math.Abs(_mid - value) > 1E-4)
                    {
                        double chg = value - _mid;

                        //Console.WriteLine(@"{0} {1} NewVal {2} Mid {3} Prev {4} Chg {5}",
                        //                  DateTime.Now.ToString("hh:mm:ss.fffff"), Name, value.ToString("#.0000"),
                        //                  _mid.ToString("#.0000"),PrevMid.ToString("#.0000"), chg.ToString("#.0000"));

                        _prevMid = _mid;
                        _mid = value;
                        UpdateMid();
                        Change = chg;

                        if (GetType() == typeof(BloombergSecurity))
                        {
                            foreach (var transposedSecurity in _transposedSecurity)
                            {
                                transposedSecurity.BaseMid = Mid;
                            }
                        }
                    }
                }
                else
                {
                    _mid = value;
                    MidInit = true;
                    PrevMid = value;
                    _prevMidInit = true;
                    UpdateMid();
                }
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
            private set
            {
                if (value != 0)
                {
                    _chg = Math.Round(value, 5);
                    UpdateChange();
                }
            }
        }

        public BloombergSecurity(DataGridViewRow dataGridrow, DataRow dataRow, List<string> securityField)
        {
            DataGridRow = dataGridrow;
            SecurityData = dataRow;
            Name = dataRow[1].ToString();
            Ticker = dataRow[0].ToString();
            SecurityFields = securityField;
        }

        public void AddTransposedSecurity(TransposedSecurity transposedSecurity)
        {
            _transposedSecurity.Add(transposedSecurity);
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
            //foreach (DataGridViewCell cell in DataGridRow.Cells)
            //    cell.Style.BackColor = Color.Yellow;
        }

        protected virtual void UpdateMid()
        {
            SecurityData["Mid"] = Mid;
        }

        private void UpdateChange()
        {
            SecurityData["Change"] = Change;
            if (Change < 0)
            {
                HighlightBackColor(false);
            }
            else
            {
                if (Change > 0)
                {
                    HighlightBackColor(true);
                }
            }
        }

        private void HighlightBackColor(bool isUp)
        {
            if (isUp)
            {
                SecurityData["Highlight"] = 1;
            }
            else
            {
                SecurityData["Highlight"] = -1;
            }
            _lastBackColorChg = DateTime.Now;
            _backColorNeedsCheck = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_backColorNeedsCheck)
            {
                var timeSinceChange = DateTime.Now - _lastBackColorChg;
                if (timeSinceChange.Seconds >= HighlightTimeInsecs)
                {
                    _backColorNeedsCheck = false;
                    SecurityData["Highlight"] = 0;
                }
            }
        }
    }

    public class TransposedSecurity : BloombergSecurity
    {
        private BloombergSecurity _baseSecurity;
        private double _emaAlpha;
        private int _lookBack;
        private double _baseMid;

        private bool _baseMidInit;
        private bool _avgSpreadInit;
        private bool _spreadInit;
        private double _spread;

        public int LookBack
        {
            get { return _lookBack; }
            set
            {
                if (value > 1)
                {
                    _lookBack = value;
                    
                    _emaAlpha = 2 / (value + 1.0);
                }
            }
        }

        public override double Mid
        {
            get
            {
                return base.Mid;
            }
            set
            {
                base.Mid = value;
                UpdateMidSpread();
            }
        }

        public double BaseMid
        {
            get { return _baseMid; }
            set
            {
                if (!_baseMidInit) _baseMidInit = true;
                _baseMid = value;
                UpdateMidSpread();
            }
        }

        public double TransposedMid { get; private set; }

        public double Spread
        {
            get { return _spread; }
            private set
            {
                if (!_spreadInit)
                {
                    _spreadInit = true;
                    _spread = value;
                }
                else
                {
                    _spread = value;

                    if (!_avgSpreadInit)
                    {
                        _avgSpreadInit = true;
                        AvgSpread = value;
                    }
                    else
                    {
                        double avgSpread = (AvgSpread*(1 - _emaAlpha)) + (value*_emaAlpha);
                        AvgSpread = avgSpread;
                    }
                }
            }
        }

        public double AvgSpread { get; private set; }

        public TransposedSecurity(BloombergSecurity baseSecurity, DataGridViewRow dataGridrow, DataRow dataRow,
                                  List<string> securityField)
            : base(dataGridrow, dataRow, securityField)
        {
            _baseSecurity = baseSecurity;
        }

        private void UpdateMidSpread()
        {
            if (MidInit && _baseMidInit)
            {
                Spread = _baseMid - base.Mid;
            }
        }

        protected override void UpdateMid()
        {
            if (_avgSpreadInit)
            {
                SecurityData["Mid"] = Mid + AvgSpread; 
            }
            else
            {
                base.UpdateMid();
            }
        }

    }
}

