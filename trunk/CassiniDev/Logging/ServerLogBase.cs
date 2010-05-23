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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;

#endregion

namespace CassiniDev.ServerLog
{
    public abstract class ServerLogBase : IDisposable
    {
        private static readonly string SqliteConnectionString;

        private readonly string _connectionString;

        private readonly DbCommand _insertCommand;

        private readonly object _lockObj = new object();

        private readonly string _physicalPath;

        private readonly DbCommand _selectCommand;

        private readonly DbCommand _selectCommandSingle;

        private readonly DbCommand _selectConversationCommand;

        private readonly DbCommand _truncateCommand;

        static ServerLogBase()
        {
            try
            {
                // check for sqlite

                // When we consider using other dbms then move connection string to configuration.
                SqliteConnectionString = "data source=" +
                                         Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                                      "ServerLog.db3");

                new SQLiteServerLog(Environment.CurrentDirectory, SqliteConnectionString);
            }
            catch
            {
                CanPersistLog = false;
            }
        }

        protected ServerLogBase(string physicalPath, string connectionString)
        {
            _physicalPath = physicalPath;
            _connectionString = connectionString;
            _insertCommand = CreateInserCommand();
            _selectCommandSingle = CreateSelectCommandSingle();
            _selectCommand = CreateSelectCommandAll(_physicalPath);
            _truncateCommand = CreateTruncateCommand();
            _selectConversationCommand = CreateSelectCommandConversation();
        }

        /// <summary>
        /// This constructor is intended for the NullDal. Don't use it otherwise.
        /// </summary>
        protected ServerLogBase()
        {
        }

        public static bool CanPersistLog { get; internal set; }

        protected abstract DbProviderFactory Factory { get; }

        public static bool LoggingEnabled { get; set; }

        public abstract string ParameterPrefix { get; }

        public abstract string TruncateStatement { get; }

        #region IDisposable Members

        public virtual void Dispose()
        {
            _insertCommand.Dispose();
            _selectCommand.Dispose();
            _selectCommandSingle.Dispose();
            _truncateCommand.Dispose();
        }

        #endregion

        public virtual void Clear()
        {
            lock (_lockObj)
            {
                using (DbConnection connection = CreateConnection())
                {
                    _truncateCommand.Connection = connection;
                    connection.Open();
                    _truncateCommand.ExecuteNonQuery();
                }
            }
        }

        public static ServerLogBase Create(string physicalPath)
        {
            if (!LoggingEnabled)
            {
                return new NullServerLogDal("Logging is disabled");
            }

            if (CanPersistLog)
            {
                try
                {
                    return new SQLiteServerLog(physicalPath, SqliteConnectionString);
                }
                catch (Exception ex)
                {
                    return new NullServerLogDal(string.Format("Error creating DAL: {0}", ex.Message));
                }
            }

            return new NullServerLogDal(SR.GetString(SR.WebdevInMemoryLogging));
        }

        public virtual LogInfo GetLog(long rowId)
        {
            LogInfo returnValue = null;
            lock (_lockObj)
            {
                _selectCommandSingle.Parameters[ParameterPrefix + "RowId"].Value = rowId;

                using (DbConnection connection = CreateConnection())
                {
                    _selectCommandSingle.Connection = connection;
                    connection.Open();

                    using (DbDataReader reader = _selectCommandSingle.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            returnValue = BuildLogInfo(reader);
                        }
                    }
                }
            }

            return returnValue;
        }

        public virtual List<LogInfo> GetLogs(Guid conversationId)
        {
            List<LogInfo> returnValue = new List<LogInfo>();

            lock (_lockObj)
            {
                using (DbConnection connection = CreateConnection())
                {
                    _selectConversationCommand.Parameters[ParameterPrefix + "ConversationId"].Value = conversationId;
                    _selectConversationCommand.Connection = connection;
                    connection.Open();

                    using (DbDataReader reader = _selectConversationCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            returnValue.Add(BuildLogInfo(reader));
                        }
                    }
                }
            }

            return returnValue;
        }

        public virtual List<LogInfo> GetLogs()
        {
            List<LogInfo> returnValue = new List<LogInfo>();

            lock (_lockObj)
            {
                using (DbConnection connection = CreateConnection())
                {
                    _selectCommand.Connection = connection;
                    connection.Open();

                    using (DbDataReader reader = _selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            returnValue.Add(BuildLogInfo(reader));
                        }
                    }
                }
            }

            return returnValue;
        }

        public virtual void SaveLog(LogInfo info)
        {
            lock (_lockObj)
            {
                SetParameters(info, _insertCommand);
                using (DbConnection connection = CreateConnection())
                {
                    _insertCommand.Connection = connection;
                    connection.Open();
                    info.RowId = (long) _insertCommand.ExecuteScalar();
                }
            }
        }

        protected LogInfo BuildLogInfo(IDataRecord reader)
        {
            return new LogInfo
                {
                    RowId = reader.GetValueOrDefault<long>("RowId"),
                    ConversationId = reader.GetValueOrDefault<Guid>("ConversationId"),
                    RowType = reader.GetValueOrDefault<long>("RowType"),
                    Created = reader.GetValueOrDefault<DateTime>("Created"),
                    PathTranslated = reader.GetValueOrDefault<string>("PathTranslated"),
                    Url = reader.GetValueOrDefault<string>("Url"),
                    Headers = reader.GetValueOrDefault<string>("Headers"),
                    StatusCode = reader.GetValueOrDefault<long?>("StatusCode"),
                    Body = reader.GetValueOrDefault<byte[]>("Body"),
                    Exception = reader.GetValueOrDefault<string>("Exception"),
                    Identity = reader.GetValueOrDefault<string>("Identity"),
                    PhysicalPath = reader.GetValueOrDefault<string>("PhysicalPath")
                };
        }

        protected DbParameter BuildParameter(string paramName, DbType paramType)
        {
            DbParameter param = Factory.CreateParameter();
            param.ParameterName = ParameterPrefix + paramName;
            param.DbType = paramType;
            return param;
        }

        protected DbConnection CreateConnection()
        {
            DbConnection connection = Factory.CreateConnection();
            connection.ConnectionString = _connectionString;
            return connection;
        }

        protected DbCommand CreateInserCommand()
        {
            DbCommand command = Factory.CreateCommand();
            command.Parameters.Add(BuildParameter("RowId", DbType.Int64));
            command.Parameters.Add(BuildParameter("ConversationId", DbType.Guid));
            command.Parameters.Add(BuildParameter("RowType", DbType.Int64));
            command.Parameters.Add(BuildParameter("Created", DbType.DateTime));
            command.Parameters.Add(BuildParameter("PathTranslated", DbType.String));
            command.Parameters.Add(BuildParameter("Url", DbType.String));
            command.Parameters.Add(BuildParameter("Headers", DbType.String));
            command.Parameters.Add(BuildParameter("StatusCode", DbType.Int64));
            command.Parameters.Add(BuildParameter("Body", DbType.Binary));
            command.Parameters.Add(BuildParameter("Exception", DbType.String));
            command.Parameters.Add(BuildParameter("Identity", DbType.String));
            command.Parameters.Add(BuildParameter("PhysicalPath", DbType.String));
            command.CommandText =
                @"
		                INSERT INTO Log
		                                      (ConversationId, RowType, Created, PathTranslated, Url, Headers, StatusCode, Body, Exception, Identity, PhysicalPath)
		                VALUES     (" +
                ParameterPrefix + "ConversationId,"
                + ParameterPrefix + "RowType, "
                + ParameterPrefix + "Created, "
                + ParameterPrefix + "PathTranslated, "
                + ParameterPrefix + "Url, "
                + ParameterPrefix + "Headers, "
                + ParameterPrefix + "StatusCode, "
                + ParameterPrefix + "Body, "
                + ParameterPrefix + "Exception, "
                + ParameterPrefix + "Identity, "
                + ParameterPrefix + "PhysicalPath); SELECT last_insert_rowid() as RowId;";
            return command;
        }

        protected DbCommand CreateSelectCommand()
        {
            DbCommand command = Factory.CreateCommand();
            command.CommandText =
                @"SELECT RowId, ConversationId, RowType, Created, PathTranslated, Url, Headers, StatusCode, Body, Exception, Identity,  PhysicalPath
                  FROM         Log";
            return command;
        }

        protected DbCommand CreateSelectCommandAll(string physicalPath)
        {
            DbCommand command = CreateSelectCommand();
            command.CommandText += " WHERE PhysicalPath = " + ParameterPrefix + "PhysicalPath;";
            DbParameter param = BuildParameter("PhysicalPath", DbType.String);
            param.Value = physicalPath;
            command.Parameters.Add(param);
            return command;
        }

        protected DbCommand CreateSelectCommandConversation()
        {
            DbCommand command = CreateSelectCommand();
            command.CommandText += " WHERE ConversationId = " + ParameterPrefix + "ConversationId ORDER BY RowId;";
            command.Parameters.Add(BuildParameter("ConversationId", DbType.Guid));
            return command;
        }

        protected DbCommand CreateSelectCommandSingle()
        {
            DbCommand command = CreateSelectCommand();
            command.CommandText += " WHERE RowId = " + ParameterPrefix + "RowId;";
            command.Parameters.Add(BuildParameter("RowId", DbType.Int64));
            return command;
        }

        protected DbCommand CreateTruncateCommand()
        {
            DbCommand command = Factory.CreateCommand();
            command.CommandText = TruncateStatement;
            return command;
        }

        protected void SetParameters(LogInfo info, DbCommand command)
        {
            command.Parameters[ParameterPrefix + "RowId"].Value = info.RowId;
            command.Parameters[ParameterPrefix + "ConversationId"].Value = info.ConversationId;
            command.Parameters[ParameterPrefix + "RowType"].Value = info.RowType;
            command.Parameters[ParameterPrefix + "Created"].Value = info.Created;
            command.Parameters[ParameterPrefix + "PathTranslated"].Value = info.PathTranslated;
            command.Parameters[ParameterPrefix + "Url"].Value = info.Url;
            command.Parameters[ParameterPrefix + "Headers"].Value = info.Headers;
            command.Parameters[ParameterPrefix + "StatusCode"].Value = info.StatusCode;
            command.Parameters[ParameterPrefix + "Body"].Value = info.Body;
            command.Parameters[ParameterPrefix + "Exception"].Value = info.Exception;
            command.Parameters[ParameterPrefix + "Identity"].Value = info.Identity;
            command.Parameters[ParameterPrefix + "PhysicalPath"].Value = info.PhysicalPath;
        }
    }
}