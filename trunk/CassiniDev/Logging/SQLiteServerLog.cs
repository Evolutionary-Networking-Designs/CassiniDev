//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System.Data.Common;
using System.Data.SQLite;

#endregion

namespace CassiniDev.ServerLog
{
    public class SQLiteServerLog : ServerLogBase
    {
        internal SQLiteServerLog(string physicalPath, string connectionString)
            : base(physicalPath, connectionString)
        {
            EnsureDatabase();
        }

        protected override DbProviderFactory Factory
        {
            get { return SQLiteFactory.Instance; }
        }

        public override string ParameterPrefix
        {
            get { return "@"; }
        }

        public override string TruncateStatement
        {
            get { return "DELETE FROM LOG;"; }
        }

        internal static void EnsureSQLite()
        {
            try
            {
                new SQLiteConnection();
                CanPersistLog = true;
            }
            catch
            {
                CanPersistLog = false;
            }
        }

        private void EnsureDatabase()
        {
            using (DbConnection connection = CreateConnection())
            {
                using (DbCommand command = Factory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText =
                        @"
                            CREATE TABLE IF NOT EXISTS [Log] (
                                [RowId] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                                [ConversationId] guid NOT NULL,
                                [RowType] integer NOT NULL,
                                [Created] datetime NOT NULL,
                                [PathTranslated] nvarchar,
                                [Url] nvarchar,
                                [Headers] nvarchar,
                                [StatusCode] integer,
                                [Body] image,
                                [Exception] nvarchar,
                                [Identity] nvarchar,
                                [Host] nvarchar,
                                [Port] integer,
                                [IPAddress] nvarchar,
                                [PhysicalPath] nvarchar,
                                [VirtualPath] nvarchar
                            );";

                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}