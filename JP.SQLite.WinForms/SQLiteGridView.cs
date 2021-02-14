using System.Drawing;
using System.Windows.Forms;

namespace JP.SQLite
{
	public class SQLiteGridView : DataGridView
	{
		/// <summary>First N columns may not be modified.
		/// Do not set before the DataBindingComplete event.
		/// Constructor parameter may be used instead.</summary>
		public byte NumberOfReadOnlyColumns
		{
			set
			{
				for(byte i = 0; i < value; ++i)
					Columns[i].ReadOnly = false;
			}
		}
		/// <summary>First N columns won't be visible to the user.
		/// Do not set before the DataBindingComplete event.
		/// Constructor parameter may be used instead.</summary>
		public byte NumberOfHiddenColumns
		{
			set
			{
				for(byte i = 0; i < value; ++i)
					Columns[i].Visible = false;
			}
		}
		/// <summary>First N columns will stay in place when scrolling horizontally.
		/// Do not set before the DataBindingComplete event.
		/// Constructor parameter may be used instead.</summary>
		public byte NumberOfFrozenColumns
		{
			set
			{
				for(byte i = 0; i < value; ++i)
					Columns[i].Frozen = false;
			}
		}

		public SQLiteGridView(object dataSource,
			byte numberOfHiddenColumns = 0,
			byte numberOfReadOnlyColumns = 0,
			byte numberOfFrozenColumns = 0)
		{
			AlternatingRowsDefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
			
			DataBindingComplete += (sender, eventArgs) =>
			{
				DataError += (s, ea) => ea.Cancel = true;
				NumberOfHiddenColumns = numberOfHiddenColumns;
				NumberOfReadOnlyColumns = numberOfReadOnlyColumns;
				NumberOfFrozenColumns = numberOfFrozenColumns;
			};

			DataSource = dataSource;
		}
	}
}
