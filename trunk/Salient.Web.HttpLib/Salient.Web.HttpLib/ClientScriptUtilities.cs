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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

#endregion

namespace Salient.Web.HttpLib
{
    public static class ClientScriptUtilities
    {
        private static readonly Regex RxMsAjaxJsonInner = new Regex("^{\\s*\"d\"\\s*:(.*)}$", RegexOptions.Compiled);

        private static readonly Regex RxMsAjaxJsonInnerType = new Regex("\\s*\"__type\"\\s*:\\s*\"[^\"]*\"\\s*,\\s*", RegexOptions.Compiled);


 

        /// <summary>
        /// Pre-processes <paramref name="json"/>, if necessary, to extract the inner object from a "d:" 
        /// wrapped MsAjax response and removing "__type" properties to allow deserialization with JavaScriptSerializer
        /// into an instance of <typeparamref name="T"/>.
        /// 
        /// Note: this method is not limited to MsAjax responses, it will capably deserialize any valid JSON.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T CleanAndDeserialize<T>(this JavaScriptSerializer serializer, string json)
        {
            string innerJson = CleanWebScriptJson(json);
            return serializer.Deserialize<T>(innerJson);
        }


        /// <summary>
        /// Pre-processes <paramref name="json"/>, if necessary, to extract the inner object from a "d:" 
        /// wrapped MsAjax response and removing "__type" properties to allow deserialization with JavaScriptSerializer
        /// into an instance of anonymous type <typeparamref name="T"/>.
        /// 
        /// Note: this method is not limited to MsAjax responses, it will capably deserialize any valid JSON.
        /// </summary>
        /// <typeparam name="T">The anonymous type defined by <paramref name="anonymousPrototype"/> </typeparam>
        /// <param name="serializer"></param>
        /// <param name="json"></param>
        /// <param name="anonymousPrototype">
        /// An instance of the anonymous type into which you would like to stuff this JSON. It simply needs to be
        /// shaped like the JSON object. 
        /// <example>
        /// string json = "{ \"name\": \"Joe\" }";
        /// var jsob = new JavaScriptSerializer().CleanAndDeserialize(json, new { name=default(string) });
        /// Debug.Assert(jsob.name=="Joe");
        /// </example>
        /// </param>
        /// <returns></returns>
        public static T CleanAndDeserialize<T>(this JavaScriptSerializer serializer, string json, T anonymousPrototype)
        {
            json = CleanWebScriptJson(json);
            Dictionary<string, object> dict = (Dictionary<string, object>)serializer.DeserializeObject(json);
            return dict.ToAnonymousType(anonymousPrototype);
        }

        /// <summary>
        /// Extracts the inner JSON of an MS Ajax 'd' result and removes embedded '__type' properties.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>The inner JSON</returns>
        private static string CleanWebScriptJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json");
            }

            Match match = RxMsAjaxJsonInner.Match(json);
            string innerJson = match.Success ? match.Groups[1].Value : json;
            return RxMsAjaxJsonInnerType.Replace(innerJson, string.Empty);
        }


        #region Dictionary to Anonymous Type

        /* An entry on Jacob Carpenter saved me from having to work this out for myself.
         * Thanks Jacob.
         * http://jacobcarpenter.wordpress.com/2008/03/13/dictionary-to-anonymous-type/
         */

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue result;
            dict.TryGetValue(key, out result);
            return result;
        }

        private static T ToAnonymousType<T, TValue>(this IDictionary<string, TValue> dict, T anonymousPrototype)
        {

            // get the sole constructor
            var ctor = anonymousPrototype.GetType().GetConstructors().Single();

            // conveniently named constructor parameters make this all possible...
            // TODO: sky: i think the conditional assignment could be improved
            // ReSharper disable CompareNonConstrainedGenericWithNull
            // In our typical use of this method, we are deserializing valid JSON, which should not contain
            // nulls for value types. So this is not a problem.
            var args = from p in ctor.GetParameters()
                       let val = dict.GetValueOrDefault(p.Name)
                       select val != null && p.ParameterType.IsAssignableFrom(val.GetType()) ?
                       (object)val : null;
            // ReSharper restore CompareNonConstrainedGenericWithNull
            return (T)ctor.Invoke(args.ToArray());
        }
        #endregion
    }
}
