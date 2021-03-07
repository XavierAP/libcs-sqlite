using System;
using System.Data.SQLite;

namespace JP.SQLite
{
	/// <summary>A simpliflying wrapper of <see cref="System.Data.IDataReader"/>
	/// that also encapsulates and takes care of disposing other necessary objects.</summary>
	public interface ISimpleDataReader : IDisposable
	{
		bool Read();

		double GetDouble();
		long GetInt64();
		string GetString();

		bool IsNullNext();
		void Skip(ushort nColumns = 1);
	}

	class SimpleDataReader : ISimpleDataReader
	{
		readonly SQLiteDataReader reader;
		readonly SQLiteConnection connection;
		readonly SQLiteCommand command;

		ushort columnCursor = 0;

		internal SimpleDataReader(SQLiteDataReader reader, SQLiteCommand command, SQLiteConnection connection)
		{
			this.reader = reader;
			this.command = command;
			this.connection = connection;
		}

		public bool Read()
		{
			columnCursor = 0;
			return reader.Read();
		}

		public double GetDouble() => reader.GetDouble(columnCursor++);
		public long GetInt64() => reader.GetInt64(columnCursor++);
		public string GetString() => reader.GetString(columnCursor++);
		
		public bool IsNullNext() => reader.IsDBNull(columnCursor);
		public void Skip(ushort nColumns = 1) => columnCursor += nColumns;

		public void Dispose()
		{
			reader.Dispose();
			command.Dispose();
			connection.Close();
		}
	}
}
