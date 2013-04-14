using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Transposer
{
    public partial class Main : Form
    {
        private const string SymbolPath = "Symbols.txt";
        private const string ParamsPath = "Parameters.txt";
        private const string bckClrCol = "Highlight";

        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly List<BloombergSecurity> _securities = new List<BloombergSecurity>();
        private List<string> Fields = new List<string>();
        private DataTable _transposerTable = new DataTable();
        private BloombergRealTimeData _bloombergRealTimeData = new BloombergRealTimeData();
        public DataGridView DataGridViewTrnspsr;

        public Main()
        {
            InitializeComponent();
            LoadParamsFromTextFile();
            SetFields();
            InitializeDataGrid();
            InitializeSymbols();
            InitTimer();
            _bloombergRealTimeData.SendRequest();

        }

        #region Initialization

        private void LoadParamsFromTextFile()
        {
            _parameters = ReadAndParseTextFile(ParamsPath);
        }

        private void InitializeDataGrid()
        {
            DataGridViewTrnspsr = dataGridViewTrnspsr;
            _transposerTable = FormatDataTable();
            AddSymbols(_transposerTable);

            var bindingSource1 = new BindingSource {DataSource = _transposerTable};
            dataGridViewTrnspsr.DataSource = bindingSource1;
            bindingSource1.ListChanged += new ListChangedEventHandler(bindingSource1_ListChanged); 


            FormatDatagrid();
        }

        public void bindingSource1_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (String.Equals(e.PropertyDescriptor.Name,bckClrCol))
            {
                int direction;
                var cellStyle = new DataGridViewCellStyle();
                int.TryParse(_transposerTable.Rows[e.NewIndex][bckClrCol].ToString(), out direction);
                if (direction < 0)
                {
                    cellStyle.BackColor = Color.Red;
                    for (int i = 0; i < dataGridViewTrnspsr.Columns.Count; i++)
                    {
                        dataGridViewTrnspsr.Rows[e.NewIndex].Cells[i].Style = cellStyle;
                    }
                }
                else
                {
                    if (direction > 0)
                    {
                        cellStyle.BackColor = Color.LawnGreen;
                        for (int i = 0; i < dataGridViewTrnspsr.Columns.Count; i++)
                        {
                            dataGridViewTrnspsr.Rows[e.NewIndex].Cells[i].Style = cellStyle;
                        }
                    }
                    else
                    {
                        cellStyle.BackColor = Color.White;
                        for (int i = 0; i < dataGridViewTrnspsr.Columns.Count; i++)
                        {
                            dataGridViewTrnspsr.Rows[e.NewIndex].Cells[i].Style = cellStyle;
                        }
                    }
                }
            }
        }

        private DataTable FormatDataTable()
        {
            var transposerTable = new DataTable();

            var symbol = transposerTable.Columns.Add("Symbol", typeof(string));
            symbol.Unique = true;

            var name = transposerTable.Columns.Add("Name", typeof(string));
            name.Unique = true;

            foreach (var field in Fields)
            {
                transposerTable.Columns.Add(field, typeof(double));
            }

            var change = transposerTable.Columns.Add("Change", typeof(string));
            var highlight = transposerTable.Columns.Add(bckClrCol, typeof(int));

            return transposerTable;
        }

        private void SetFields()
        {
            Fields = new List<string>() { "Mid", "Bid", "Ask" };
        }

        private void FormatDatagrid()
        {

            dataGridViewTrnspsr.ScrollBars = ScrollBars.Horizontal;
            dataGridViewTrnspsr.RowHeadersVisible = false;

            dataGridViewTrnspsr.Columns[0].ReadOnly = true;
            dataGridViewTrnspsr.Columns[0].DefaultCellStyle.BackColor = Color.LightGray;
            dataGridViewTrnspsr.Columns[1].ReadOnly = true;


            dataGridViewTrnspsr.Columns[2].ReadOnly = true;
            dataGridViewTrnspsr.Columns[2].DefaultCellStyle.Format = "#.00##";
            dataGridViewTrnspsr.Columns[3].ReadOnly = true;
            dataGridViewTrnspsr.Columns[3].DefaultCellStyle.Format = "#.00##";
            dataGridViewTrnspsr.Columns[4].ReadOnly = true;
            dataGridViewTrnspsr.Columns[4].DefaultCellStyle.Format = "#.00##";
            dataGridViewTrnspsr.Columns[5].ReadOnly = true;
            dataGridViewTrnspsr.Columns[5].DefaultCellStyle.Format = "#.0000";

            dataGridViewTrnspsr.Columns[0].Visible = false;
            dataGridViewTrnspsr.Columns[3].Visible = false;
            dataGridViewTrnspsr.Columns[4].Visible = false;
            dataGridViewTrnspsr.Columns[6].Visible = false;


            CorrectWindowSize();
        }

        private Dictionary<string, string> GetSymbols()
        {
            return ReadAndParseTextFile(SymbolPath);
        }

        private void AddSymbols(DataTable transposerTable)
        {
            var symbols = GetSymbols();
            foreach (var symbol in symbols)
            {
                DataRow row = transposerTable.Rows.Add();
                row[0] = symbol.Key;
                row[1] = symbol.Value;
            }
        }

        private void InitializeSymbols()
        {
            //dataGridViewTrnspsr.Rows
            for (int i = 0; i < dataGridViewTrnspsr.Rows.Count; i++)
            {
                DataGridViewRow dataGridrow = dataGridViewTrnspsr.Rows[i];
                DataRow dataRow = _transposerTable.Rows[i];
                var security = new BloombergSecurity(dataGridrow, dataRow, Fields, this);
                _securities.Add(security);
                _bloombergRealTimeData.AddSecurity(security);
            }
        }

        private Dictionary<string, string> ReadAndParseTextFile(string path)
        {
            var parsedText = new Dictionary<string, string>();

            try
            {
                using (var sr = new StreamReader(path))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] items = line.Split(';');
                        if (items[0].ToString().Substring(0, 1) != "#")
                        {
                            parsedText.Add(items[0], items[1]);
                            Console.WriteLine("{0} {1}", items[0], items[1]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return parsedText;
        }

        private void InitTimer()
        {
            //Instantiate the timer
            foreach (var bloombergSecurity in _securities)
            {
                timer1 = new Timer();
                timer1.Interval = 100; //1000ms = 1sec
                bloombergSecurity.IntiTimer(timer1);
                timer1.Start();
            }

            timer2 = new Timer();
            timer2.Interval = new Random().Next(4000, 7000); //1000ms = 1sec
            _securities[0].IntiTimer2(timer2);
            timer2.Start();

            timer2 = new Timer();
            timer2.Interval = new Random().Next(4000, 7500); //1000ms = 1sec
            _securities[1].IntiTimer2(timer2);
            timer2.Start();

            timer2 = new Timer();
            timer2.Interval = new Random().Next(4000, 8000); //1000ms = 1sec
            _securities[2].IntiTimer2(timer2);
            timer2.Start();

            timer2 = new Timer();
            timer2.Interval = new Random().Next(4000, 8100); //1000ms = 1sec
            _securities[3].IntiTimer2(timer2);
            timer2.Start();

            timer2 = new Timer();
            timer2.Interval = new Random().Next(4000, 8200); //1000ms = 1sec
            _securities[4].IntiTimer2(timer2);
            timer2.Start();

            timer2 = new Timer();
            timer2.Interval = new Random().Next(4000, 8300); //1000ms = 1sec
            _securities[5].IntiTimer2(timer2);
            timer2.Start();
        }

        #endregion

        #region Form Auto Sizing

        public void CorrectWindowSize()
        {
            int width = WinObjFunctions.CountGridWidth(dataGridViewTrnspsr);
            int height = WinObjFunctions.CountGridHeight(dataGridViewTrnspsr);
            ClientSize = new Size(width, height);
        }

        public static class WinObjFunctions
        {
            public static int CountGridWidth(DataGridView dgv)
            {
                int width = 0;
                foreach (DataGridViewColumn column in dgv.Columns)
                    if (column.Visible == true)
                        width += column.Width;
                return width += 3;
            }

            public static int CountGridHeight(DataGridView dgv)
            {
                int width = 0;
                foreach (DataGridViewRow rows in dgv.Rows)
                    if (rows.Visible == true)
                        width += rows.Height;
                return width += 44;
            }
        }

        #endregion




    }
}
