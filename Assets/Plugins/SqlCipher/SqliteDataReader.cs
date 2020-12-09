using SqlCipher;
using System;
using System.Globalization;
using Sqlite3DatabaseHandle = System.IntPtr;

namespace SqlCipher
{
    public sealed class SqliteDataReader
    {
        Sqlite3DatabaseHandle stmt;

        internal SqliteDataReader(SqliteCommand cmd)
        {
            if (cmd != null)
            {
                stmt = cmd.Prepare();
            }
        }

        public void SetCommand(SqliteCommand cmd)
        {
            if (cmd != null)
            {
                stmt = cmd.Prepare();
            }
        }

        public int FieldCount
        {
            get
            {
                return SQLite3.ColumnCount(stmt);
            }
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        public bool GetBoolean(int i)
        {
            return SQLite3.ColumnInt(stmt, i) == 1;
        }

        public byte GetByte(int i)
        {
            return (byte)SQLite3.ColumnInt(stmt, i);
        }

        public char GetChar(int i)
        {
            return (char)SQLite3.ColumnInt(stmt, i);
        }

        public void Close()
        {
            SQLite3.Finalize(stmt);
            stmt = Sqlite3DatabaseHandle.Zero;
        }

        public DateTime GetDateTime(int i)
        {
            return new DateTime(SQLite3.ColumnInt64(stmt, i));
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)SQLite3.ColumnDouble(stmt, i);
        }

        public double GetDouble(int i)
        {
            return SQLite3.ColumnDouble(stmt, i);
        }

        public float GetFloat(int i)
        {
            return (float)SQLite3.ColumnDouble(stmt, i);
        }

        public Guid GetGuid(int i)
        {
            string text = SQLite3.ColumnString(stmt, i);
            return new Guid(text);
        }

        public short GetInt16(int i)
        {
            return (short)SQLite3.ColumnInt(stmt, i);
        }

        public int GetInt32(int i)
        {
            return SQLite3.ColumnInt(stmt, i);
        }

        public long GetInt64(int i)
        {
            return SQLite3.ColumnInt64(stmt, i);
        }

        public string GetName(int i)
        {
            return SQLite3.ColumnName16(stmt, i);
        }

        public int GetOrdinal(string name)
        {
            int num = FieldCount;
            for (int i = 0; i < num; i++)
            {
                if (string.Compare(name, GetName(i), true, CultureInfo.InvariantCulture) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetString(int i)
        {
            return SQLite3.ColumnString(stmt, i);
        }

        public object GetValue(int i)
        {
            SQLite3.ColType sQLiteType = GetSQLiteType(i);
            return ReadCol(stmt, i, sQLiteType);
        }

        private object ReadCol(Sqlite3DatabaseHandle stmt, int index, SQLite3.ColType type)
        {
            if (type == SQLite3.ColType.Null)
                return null;

            if (type == SQLite3.ColType.Integer)
                return SQLite3.ColumnDouble(stmt, index);

            if (type == SQLite3.ColType.Float)
                return SQLite3.ColumnDouble(stmt, index);

            if (type == SQLite3.ColType.Text)
                return SQLite3.ColumnString(stmt, index);

            if (type == SQLite3.ColType.Blob)
                return SQLite3.ColumnBlob(stmt, index);

            return null;
        }

        public int GetValues(object[] values)
        {
            int num = FieldCount;
            if (values.Length < num)
            {
                num = values.Length;
            }
            for (int i = 0; i < num; i++)
            {
                values[i] = GetValue(i);
            }
            return num;
        }

        public bool IsDBNull(int i)
        {
            return GetSQLiteType(i) == SQLite3.ColType.Null;
        }

        private SQLite3.ColType GetSQLiteType(int i)
        {
            SQLite3.ColType colType = SQLite3.ColumnType(stmt, i);
            return colType;
        }

        public bool Read()
        {
            return SQLite3.Step(stmt) == SQLite3.Result.Row;
        }

        public void Dispose()
        {

        }

    }
}