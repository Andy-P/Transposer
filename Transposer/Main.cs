using System;
using System.Collections.Generic;
//using System.ComponentModel;
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

        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private readonly List<BloombergSecurity> _securities = new List<BloombergSecurity>() ;

        DataTable _transposerTable = new DataTable();

        public Main()
        {
            InitializeComponent();
            LoadParamsFromTextFile();
            InitializeDataGrid();
            InitializeSymbols();
        }

        #region Initialization

        private void LoadParamsFromTextFile()
        {
            _parameters = ReadAndParseTextFile(ParamsPath);
        }

        private void InitializeDataGrid()
        {
            _transposerTable = FormatDataTable();
            AddSymbols(_transposerTable);
            dataGridViewTrnspsr.DataSource = _transposerTable;
            FormatDatagrid();
        }

        private static DataTable FormatDataTable()
        {
            var transposerTable = new DataTable();

            var symbol = transposerTable.Columns.Add("Symbol", typeof(string));
            symbol.Unique = true;

            var name = transposerTable.Columns.Add("Name", typeof(string));
            name.Unique = true;

            transposerTable.Columns.Add("Mid", typeof(double));
            transposerTable.Columns.Add("Bid", typeof(double));
            transposerTable.Columns.Add("Ask", typeof(double));

            return transposerTable;
        }

        private void FormatDatagrid()
        {

            dataGridViewTrnspsr.ScrollBars = ScrollBars.Horizontal;
            dataGridViewTrnspsr.RowHeadersVisible = false;

            dataGridViewTrnspsr.Columns[0].ReadOnly = true;
            dataGridViewTrnspsr.Columns[1].ReadOnly = true;
            dataGridViewTrnspsr.Columns[2].ReadOnly = true;
            dataGridViewTrnspsr.Columns[3].ReadOnly = true;
            dataGridViewTrnspsr.Columns[4].ReadOnly = true;

            dataGridViewTrnspsr.Columns[3].Visible = false;
            dataGridViewTrnspsr.Columns[4].Visible = false;

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
            foreach (DataRow row in _transposerTable.Rows)
            {
                var security = new BloombergSecurity(row);
                _securities.Add(security);
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
                return width += 20;
            }
        }

        #endregion
    }
}
