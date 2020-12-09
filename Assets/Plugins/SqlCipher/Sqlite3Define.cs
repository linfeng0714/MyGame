﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlCipher
{
    [Flags]
    public enum SQLiteOpenFlags
    {
        ReadOnly = 1,
        ReadWrite = 2,
        Create = 4,
        NoMutex = 0x8000,
        FullMutex = 0x10000,
        SharedCache = 0x20000,
        PrivateCache = 0x40000,
        ProtectionComplete = 0x00100000,
        ProtectionCompleteUnlessOpen = 0x00200000,
        ProtectionCompleteUntilFirstUserAuthentication = 0x00300000,
        ProtectionNone = 0x00400000
    }

    [Flags]
    public enum CreateFlags
    {
        None = 0,
        ImplicitPK = 1, // create a primary key for field called 'Id' (Orm.ImplicitPkName)
        ImplicitIndex = 2, // create an index for fields ending in 'Id' (Orm.ImplicitIndexSuffix)
        AllImplicit = 3, // do both above

        AutoIncPK = 4 // force PK field to be auto inc
    }

    public class SQLiteException : Exception
    {
        protected SQLiteException(SQLite3.Result r, string message) : base(message)
        {
            this.Result = r;
        }

        public SQLite3.Result Result { get; private set; }

        public static SQLiteException New(SQLite3.Result r, string message)
        {
            return new SQLiteException(r, message);
        }
    }
}