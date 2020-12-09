using SqlCipher;
using System;
using Sqlite3Statement = System.IntPtr;

namespace SqlCipher
{
    public sealed class SqliteCommand
    {
        public string CommandText { get; set; }

        public SqliteConnection Connection { get; set; }

        public bool UTF8 { get; set; }

        public SqliteCommand(SqliteConnection connection)
        {
            Connection = connection;
            this.CommandText = "";
            this.UTF8 = false;
        }

        public SqliteDataReader ExecuteReader()
        {
            return new SqliteDataReader(this);
        }

        public int ExecuteNonQuery()
        {
            SQLite3.Result r = SQLite3.Result.OK;
            Sqlite3Statement stmt = Prepare();
            r = SQLite3.Step(stmt);
            Finalize(stmt);

            if (r == SQLite3.Result.Row)
            {
                return 0;
            }

            if (r == SQLite3.Result.Done)
            {
                int rowsAffected = SQLite3.Changes(this.Connection.Handle);
                return rowsAffected;
            }

            if (r == SQLite3.Result.Error)
            {
                string msg = SQLite3.GetErrmsg(this.Connection.Handle);
                throw SQLiteException.New(r, msg);
            }

            if (r == SQLite3.Result.Constraint)
                if (SQLite3.ExtendedErrCode(this.Connection.Handle) == SQLite3.ExtendedResult.ConstraintNotNull)
                    throw SQLiteException.New(r, SQLite3.GetErrmsg(this.Connection.Handle));

            throw SQLiteException.New(r, r.ToString());
        }

        public Sqlite3Statement Prepare()
        {
            Sqlite3Statement stmt = SQLite3.Prepare2(this.Connection.Handle, this.CommandText, this.UTF8);
            return stmt;
        }

        private void Finalize(Sqlite3Statement stmt)
        {
            SQLite3.Finalize(stmt);
        }

        public void Dispose()
        {

        }

    }
}