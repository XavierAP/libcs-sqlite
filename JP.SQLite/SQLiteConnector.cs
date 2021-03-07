using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace JP.SQLite
{
	public interface ISQLiteConnector
	{
		void Attach(string pathName, bool fileExists, string dbName = null);
		void Detach(string dbName);
		ISimpleDataReader Select(string sqlStatement);
		void Write(IEnumerable<string> sqlStatements);
		void Write(params string[] sqlStatements);
	}

	/// <summary>SQLite connector with simplified interface, and thread-safe.</summary>
	/// <remarks>Wraps <see cref="System.Data.SQLite.SQLiteConnection"/>.
	/// Every <see cref="Select"/>() query creates a transaction, and
	/// closes it and the connection before returning (or throwing).
	/// So does every <see cref="Write"/>() call, although this can process
	/// any number of SQL DML or DDL commands at once; in one transaction,
	/// and if any of it throws an error, the whole set is rolled back.
	/// Therefore
	/// prefer infrequent queries and manipulations, and cache data
	/// in your own program's memory, and handle exceptions.</remarks>
	/// <exception>Up to you buddy.</exception>
	public class SQLiteConnector : ISQLiteConnector, IDisposable
	{
		private readonly SqliteConnection connection;
		private readonly object Locker = new object();

		/// <param name="fileExists">If false, tries to create a blank file;
		/// if true and file doesn't exist actually, throws.</param>
		public SQLiteConnector(string pathName, bool fileExists)
		{
			CheckFile(pathName, fileExists);

			var cs = new SqliteConnectionStringBuilder
			{
				ForeignKeys = true,
				DataSource = pathName
			};
			connection = new SqliteConnection(cs.ToString());
			/*
			if(attachedPathNames != null && attachedPathNames.Length > 0)
			{
				var sb = new StringBuilder(attachedPathNames[0]);
				for(int i = 1; i < attachedPathNames.Length; ++i)
					 sb.Append(';').Append(attachedPathNames[i]);
				cs.Attach = sb.ToString();
			}*/
		}

		/// <param name="fileExists">If false, tries to create a blank file;
		/// if true and file doesn't exist actually, throws.</param>
		/// <param name="dbName">If not specified, equal to the file name (without path or extension).</param>
		public void Attach(string pathName, bool fileExists, string dbName = null)
		{
			CheckFile(pathName, fileExists);

			if(string.IsNullOrWhiteSpace(dbName))
				dbName = Path.GetFileNameWithoutExtension(pathName);

			Write(string.Format("ATTACH '{0}' as '{1}'", pathName, dbName));
		}

		/// <param name="dbName">Name of the database (not necessarily of the file and certainly no path or extensions).</param>
		public void Detach(string dbName)
		{
			if(string.IsNullOrWhiteSpace(dbName))
				throw new ArgumentNullException("dbName");

			Write(string.Format("DETACH '{0}'", dbName));
		}

		public ISimpleDataReader Select(string sqlStatement)
		{
			if(string.IsNullOrWhiteSpace(sqlStatement))
				throw new ArgumentNullException(nameof(sqlStatement));

			lock(Locker)
			{
				try
				{
					connection.Open();
					var command = new SqliteCommand(sqlStatement, connection);
					return new SimpleDataReader(command.ExecuteReader(), command, connection);
				}
				catch
				{
					connection.Close();
					throw;
				}
			}
		}

		public void Write(params string[] sqlStatements)
		{
			Write((IEnumerable<string>)sqlStatements);
		}

		/// <summary>Executes any non-query commands (data manipulation or data definition) in one transaction.</summary>
		/// <exception>On error, rolls back the whole transaction and re-throws.</exception>
		/// <param name="sqlStatements">Commands to be executed in sequence.</param>
		public void Write(IEnumerable<string> sqlStatements)
		{
			if(sqlStatements == null)
				throw new ArgumentNullException("sqlStatements");

			SqliteTransaction transaction = null;
			lock(Locker)
			{
				try
				{
					connection.Open();
					transaction = connection.BeginTransaction();

					using(var command = new SqliteCommand())
					{
						command.Connection = connection;
						command.Transaction = transaction;

						foreach(var statement in sqlStatements)
						{
							if(string.IsNullOrWhiteSpace(statement)) throw new ArgumentNullException(
								"sqlStatements", "One of the statements was blank or null.");
							command.CommandText = statement;
							command.ExecuteNonQuery();
						}
					}
					transaction.Commit();
				}
				catch
				{
					if(transaction != null)
						transaction.Rollback();

					throw;
				}
				finally
				{
					connection.Close();
					if(transaction != null)
						transaction.Dispose();
				}
			}
		}

		private void CheckFile(string pathName, bool fileExists)
		{
			if(fileExists)
			{
				if(!File.Exists(pathName))
					throw new FileNotFoundException("File not found.", pathName);
			}
			else File.Create(pathName).Dispose();
		}

		public void Dispose() => connection.Dispose();
	}
}
