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
using System.Data.Common;

#endregion

namespace CassiniDev.ServerLog
{
    /// <summary>
    /// For use when data provider is not available
    /// </summary>
    public class NullServerLogDal : ServerLogBase
    {
        private readonly string _reason;

        public NullServerLogDal(string reason)
        {
            _reason = reason;
        }

        protected override DbProviderFactory Factory
        {
            get { return null; }
        }

        public override string ParameterPrefix
        {
            get { return null; }
        }

        public override string TruncateStatement
        {
            get { return null; }
        }

        public override void Clear()
        {
        }

        public override LogInfo GetLog(long rowId)
        {
            return CreateLog(0, Guid.Empty);
        }

        public override List<LogInfo> GetLogs(Guid conversationId)
        {
            return new List<LogInfo> {CreateLog(0, Guid.Empty, 0)};
        }

        public override List<LogInfo> GetLogs()
        {
            return new List<LogInfo> {CreateLog(0, Guid.Empty, 0)};
        }

        public override void SaveLog(LogInfo info)
        {
        }

        private LogInfo CreateLog(long rowId, Guid id, int rowType)
        {
            return new LogInfo
                {
                    RowId = rowId,
                    Body = null,
                    ConversationId = id,
                    Created = DateTime.Now,
                    Exception = null,
                    Headers = _reason,
                    Identity = null,
                    PathTranslated = null,
                    PhysicalPath = null,
                    RowType = rowType,
                    StatusCode = 0,
                    Url = _reason
                };
        }

        private LogInfo CreateLog(long rowId, Guid id)
        {
            return CreateLog(rowId, id, 1);
        }
    }
}