// /*!
//  * Project: Salient.Web.HttpLib
//  * http://salient.codeplex.com
//  *
//  * Copyright 2010, Sky Sanders
//  * Dual licensed under the MIT or GPL Version 2 licenses.
//  * http://salient.codeplex.com/license
//  *
//  * Date: April 11 2010 
//  */

#region

using System;
using System.Runtime.Serialization;

#endregion

namespace Salient.Web.HttpLib.TestSite
{
    [Serializable]
    [DataContract]
    public class TestClass
    {

        private int _intVal;
        private DateTime _date;
        private string _header;
        private string _name;

        [DataMember]
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        [DataMember]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [DataMember]
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }

        [DataMember]
        public int IntVal
        {
            get { return _intVal; }
            set { _intVal = value; }
        }
    }
}