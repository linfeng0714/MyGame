using SqlCipher;
using System;
using System.Text;
using System.Threading;
using Sqlite3DatabaseHandle = System.IntPtr;

namespace SqlCipher
{
    public sealed class SqliteConnection : IDisposable
    {
        internal static readonly Sqlite3DatabaseHandle NullHandle = default(Sqlite3DatabaseHandle);

        public Sqlite3DatabaseHandle Handle { get; private set; }

        public string DatabasePath { get; private set; }

        private bool _open;

        public SqliteConnection(string databasePath, string password = null)
            : this(databasePath, password, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create)
        {

        }

        public SqliteConnection(string databasePath, string password, SQLiteOpenFlags openFlags)
        {
            if (string.IsNullOrEmpty(databasePath))
                throw new ArgumentException("Must be specified", "databasePath");

            this.DatabasePath = databasePath;

            Sqlite3DatabaseHandle handle;

            byte[] databasePathAsBytes = GetNullTerminatedUtf8(this.DatabasePath);
            SQLite3.Result r = SQLite3.Open_V2(databasePathAsBytes, out handle, (int)openFlags, IntPtr.Zero);
            this.Handle = handle;
            if (r != SQLite3.Result.OK)
            {
                string msg = SQLite3.GetErrmsg(handle);
                throw SQLiteException.New(r, string.Format("Open database file Error!\n file:{0}\n result:{1}\n msg:{2}\n", this.DatabasePath, r, msg));
            }

            if (!string.IsNullOrEmpty(password))
            {
                byte[] passwordAsBytes = GetNullTerminatedUtf8(password);
                SQLite3.Result result = SQLite3.Key(handle, passwordAsBytes, passwordAsBytes.Length);
                if (result != SQLite3.Result.OK)
                {
                    string msg = SQLite3.GetErrmsg(handle);
                    throw SQLiteException.New(r, string.Format("Set Password Error!\n file:{0}\n result:{1}\n msg:{2}\n", this.DatabasePath, result, msg));
                }
            }

            this._open = true;
        }

        private static byte[] GetNullTerminatedUtf8(string sourceText)
        {
            int num = Encoding.UTF8.GetByteCount(sourceText) + 1;
            byte[] array = new byte[num];
            num = Encoding.UTF8.GetBytes(sourceText, 0, sourceText.Length, array, 0);
            array[num] = 0;
            return array;
            //return Encoding.UTF8.GetBytes(sourceText);
        }

        public SqliteTransaction BeginTransaction()
        {
            return new SqliteTransaction(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SqliteConnection()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            Close();
        }

        public void Close()
        {
            if (this._open && this.Handle != NullHandle)
            {
                try
                {
                    SQLite3.Result r = SQLite3.Close(this.Handle);
                    if (r != SQLite3.Result.OK)
                    {
                        string msg = SQLite3.GetErrmsg(this.Handle);
                        throw SQLiteException.New(r, msg);
                    }
                }
                finally
                {
                    this.Handle = NullHandle;
                    this._open = false;
                }
            }
        }

        public SqliteCommand CreateCommand()
        {
            if (!this._open)
                throw SQLiteException.New(SQLite3.Result.Error, "Cannot create commands from unopened database");

            return new SqliteCommand(this);
        }

        public void ChangePassword(string newPassword)
        {
            byte[] passwordAsBytes = GetNullTerminatedUtf8(newPassword);
            SQLite3.Result result = SQLite3.ReKey(this.Handle, passwordAsBytes, passwordAsBytes.Length);
            if (result != SQLite3.Result.OK)
            {
                string msg = SQLite3.GetErrmsg(this.Handle);
                throw SQLiteException.New(result, msg);
            }
        }

        public int Execute(string query)
        {
            SqliteCommand cmd = CreateCommand();
            cmd.CommandText = query;

            int r = cmd.ExecuteNonQuery();
            return r;
        }


    }
}