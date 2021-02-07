using System.Drawing;
using System.Windows.Forms;

namespace JP.SQLite
{
	public partial class SQLiteTableGridView : Form
	{
		public bool AllowUserToAddRows
		{
			get => table.AllowUserToAddRows;
			set => table.AllowUserToAddRows = value;
		}
		public bool AllowUserToDeleteRows
		{
			get => table.AllowUserToDeleteRows;
			set => table.AllowUserToDeleteRows = value;
		}

		public SQLiteTableGridView(object dataSource,
			int numOfInvisibleCols = 0, int numOfFrozenCols = 1)
		{
			InitializeComponent();
			
			table = new DataGridView();
			table.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
			table.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			table.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			table.AutoSize = true;
			table.MaximumSize = new Size(1920, 800);
			table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			table.Parent = this;
			table.BringToFront();
			table.Dock = DockStyle.Fill;
			
			table.DataBindingComplete += (sender, eventArgs) =>
			{
				table.DataError += (s, ea) => ea.Cancel = true ;
				for(int i = 0; i < numOfFrozenCols; ++i)
					table.Columns[i].Frozen = true;
				for(int i = 0; i < numOfInvisibleCols; ++i)
					table.Columns[i].Visible = false;
			};
			table.DataSource = dataSource;
		}

		readonly DataGridView table;
	}
}
