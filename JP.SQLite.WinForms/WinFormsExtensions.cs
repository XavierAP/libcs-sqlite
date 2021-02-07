using System;
using System.Windows.Forms;

namespace JP.SQLite
{
	public static class WinFormsExtensions
	{
		public static void Display(this Exception err) => MessageBox.Show(err.Message,
			"SQL error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}
}
