using SqlCipher;
using System;
using System.Threading;

namespace SqlCipher
{
    public sealed class SqliteTransaction
    {
        internal SqliteConnection _cnn;
        private int _transactionDepth;

        internal SqliteTransaction(SqliteConnection connection)
        {
            _cnn = connection;
            if (Interlocked.CompareExchange(ref this._transactionDepth, 1, 0) == 0)
            {
                try
                {
                    _cnn.Execute("begin transaction");
                }
                catch (Exception ex)
                {
                    SQLiteException sqlExp = ex as SQLiteException;
                    if (sqlExp != null)
                    {
                        switch (sqlExp.Result)
                        {
                            case SQLite3.Result.IOError:
                            case SQLite3.Result.Full:
                            case SQLite3.Result.Busy:
                            case SQLite3.Result.NoMem:
                            case SQLite3.Result.Interrupt:
                                RollbackTo(null, true);
                                break;
                        }
                    }
                    else
                    {
                        Interlocked.Decrement(ref this._transactionDepth);
                    }
                    _cnn = null;
                    throw;
                }
            }
            else
            {
                _cnn = null;
                throw new InvalidOperationException("Cannot begin a transaction while already in a transaction.");
            }
        }

        private void DoSavePointExecute(string savepoint, string cmd)
        {
            // Validate the savepoint
            int firstLen = savepoint.IndexOf('D');
            if (firstLen >= 2 && savepoint.Length > firstLen + 1)
            {
                int depth;
                if (int.TryParse(savepoint.Substring(firstLen + 1), out depth))
                    if (0 <= depth && depth < this._transactionDepth)
                    {
#if NETFX_CORE
						Volatile.Write (ref _transactionDepth, depth);
#elif SILVERLIGHT
						_transactionDepth = depth;
#else
                        Thread.VolatileWrite(ref this._transactionDepth, depth);
#endif
                        _cnn.Execute(cmd + savepoint);
                        return;
                    }
            }

            throw new ArgumentException(
                "savePoint is not valid, and should be the result of a call to SaveTransactionPoint.", "savePoint");
        }

        public void Commit()
        {
            if (Interlocked.Exchange(ref this._transactionDepth, 0) != 0) _cnn.Execute("commit");
            // Do nothing on a commit with no open transaction
            _cnn = null;
        }

        public void Rollback()
        {
            RollbackTo(null, false);
        }

        public void RollbackTo(string savepoint)
        {
            RollbackTo(savepoint, false);
        }

        private void RollbackTo(string savepoint, bool noThrow)
        {
            try
            {
                if (string.IsNullOrEmpty(savepoint))
                {
                    if (Interlocked.Exchange(ref this._transactionDepth, 0) > 0) _cnn.Execute("rollback");
                }
                else
                {
                    DoSavePointExecute(savepoint, "rollback to ");
                }
            }
            catch (SQLiteException)
            {
                _cnn = null;
                if (!noThrow)
                    throw;
            }

            _cnn = null;
        }
    }
}