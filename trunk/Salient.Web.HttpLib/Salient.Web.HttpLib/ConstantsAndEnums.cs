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
namespace Salient.Web.HttpLib
{
    /// <summary>
    /// Http methdods supported by HttpRequestUtil
    /// </summary>
    public enum HttpMethod
    {
        Get,
        Post
    }

    /// <summary>
    /// Request content-types supported by HttpRequestUtil
    /// </summary>
    public enum ContentType
    {
        None,
        ApplicationJson,
        TextJson,
        ApplicationForm
    }

    /// <summary>
    /// Utility class to facilitate mapping ContentType enum to corresponding constants
    /// </summary>
    internal static class ContentTypes
    {
        public const string ApplicationForm = "application/x-www-form-urlencoded; charset=UTF-8";
        public const string ApplicationJson = "application/json; charset=UTF-8";
        public const string None = "";
        public const string TextJson = "text/json; charset=UTF-8";

        public static string AsString(this ContentType value)
        {
            switch (value)
            {
                case ContentType.None:
                    return None;
                case ContentType.ApplicationJson:
                    return ApplicationJson;
                case ContentType.TextJson:
                    return TextJson;
                case ContentType.ApplicationForm:
                    return ApplicationForm;
                default:
                    return None;
            }
        }
    }
}