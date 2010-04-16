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

using System.IO;
using System.Text;

#endregion

namespace Salient.Web.HttpLib
{
    /// <summary>
    /// A group of extension methods to faciliate retrieving data from streams.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Reads stream into a byte array, which is returned.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] Bytes(this Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        return ms.ToArray();
                    }
                    ms.Write(buffer, 0, read);
                }
            }
        }


        /// <summary>
        /// Reads stream into a string, which is returned.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string Text(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}