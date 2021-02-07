using System;
using System.Data;
using System.Data.SQLite;

namespace JP.SQLite
{
	public class SQLiteBinder : IDisposable
	{
		public DataTable DataTable { get; }
		
		readonly SQLiteDataAdapter DataAdapter;
		readonly SQLiteCommandBuilder CommandBuilder;

		public SQLiteBinder(string dbFilePathName, string selectCommand, byte numOfReadOnlyCols = 1)
		{
			Validate(selectCommand);

			DataAdapter = new SQLiteDataAdapter(
				$"PRAGMA foreign_keys = ON; {selectCommand};",
				$"Data Source=\"{dbFilePathName}\"");
			CommandBuilder = new SQLiteCommandBuilder(DataAdapter);
			DataTable = new DataTable();
			DataAdapter.Fill(DataTable);

			for(byte i = 0; i < numOfReadOnlyCols; ++i)
				DataTable.Columns[i].ReadOnly = true;
		}

		public void Update()
		{
			if(IsDisposed) throw new InvalidOperationException($"May not call {nameof(SQLiteBinder)}.{nameof(Update)} after .{nameof(Dispose)}.");

			DataAdapter.Update(DataTable);
			DataTable.AcceptChanges();
		}

		public void Dispose()
		{
			IsDisposed = true;
			DataTable.Dispose();
			CommandBuilder.Dispose();
			DataAdapter.Dispose();
		}
		bool IsDisposed = false;

		static void Validate(string selectCommand)
		{
			if(string.IsNullOrWhiteSpace(selectCommand))
				throw new ArgumentException("SQL select command is empty.");

			var words = selectCommand.Split(whitespace, 3, StringSplitOptions.RemoveEmptyEntries);

			if("SELECT" != words[0].ToUpper() || "*" != words[1])
				throw new ArgumentException($"Only «SELECT *» SQL command supported by {nameof(SQLiteBinder)}. Received: {selectCommand}");
		}
		readonly static char[] whitespace = " \n\r\t".ToCharArray();
	}
}
