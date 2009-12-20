// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/

#region

using System;
using System.Reflection;

#endregion

namespace Cassini
{
    internal class Program
    {
        private static Server _server;

        private static void Main(string[] args)
        {
            string _portString = "80";
            string _virtRoot = "/";
            string _appPath = string.Empty;

            try
            {
                if (args.Length >= 1) _appPath = args[0];
                if (args.Length >= 2) _portString = args[1];
                if (args.Length >= 3) _virtRoot = args[2];
            }
            catch
            {
            }


            if (args != null)
            {
                try
                {
                    _server = new Server(Convert.ToInt32(_portString), _virtRoot, _appPath);
                    _server.Start();
                }
                catch
                {
                    throw new Exception("Cassini Managed Web Server failed to start listening on port " +
                                        _portString + ".\r\n"
                                        + "Possible conflict with another Web Server on the same port.");
                }

                Console.WriteLine("\nstarted:{0}", _server.RootUrl);      
                Console.WriteLine("{0}\n", Assembly.GetAssembly(typeof (Server)).FullName);
                Console.WriteLine("\nPress enter to shutdown.\n");
                Console.ReadLine();

                try
                {
                    _server.Stop();
                }
                catch
                {
                }
                finally
                {
                    _server = null;
                }
            }


     
        }
    }
}