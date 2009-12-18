#region

using System;
using System.IO;
using System.Reflection;
using Cassini.CommandLine;

#endregion

namespace Cassini
{
    internal class Program
    {
        private static Server _server;


        private static void Main(string[] args)
        {
            Console.WriteLine("{0}\n", Assembly.GetAssembly(typeof (Server)).FullName);

            CassiniArgs parsedArgs = new CassiniArgs();
            if (Parser.ParseArgumentsWithUsage(args, parsedArgs))
            {
                if (string.Compare("Current path", parsedArgs.path, true) == 0)
                {
                    parsedArgs.path = Environment.CurrentDirectory;
                }


                try
                {
                    if (string.IsNullOrEmpty(parsedArgs.path) || !Directory.Exists(parsedArgs.path))
                    {
                        throw new Exception("Invalid path.");
                    }

                    if (parsedArgs.port <= 0)
                    {
                        throw new Exception("Invalid port.");
                    }

                    if (string.IsNullOrEmpty(parsedArgs.vroot) || !parsedArgs.vroot.StartsWith("/"))
                    {
                        throw new Exception("Invalid vroot.");
                    }

                    try
                    {
                        _server = new Server(parsedArgs.port, parsedArgs.vroot, parsedArgs.path);
                        _server.Start();
                    }
                    catch
                    {
                        throw new Exception("Cassini Managed Web Server failed to start listening on port " +
                                            parsedArgs.port + ".\r\n"
                                            + "Possible conflict with another Web Server on the same port.");
                    }

                    Console.WriteLine("started  {0}", _server.RootUrl);
                    Console.WriteLine("\nPress enter to shutdown.\n");
                    Console.ReadLine();

                    try
                    {
                        _server.Stop();
                    }
// ReSharper disable EmptyGeneralCatchClause
                    catch
// ReSharper restore EmptyGeneralCatchClause
                    {
                    }
                    finally
                    {
                        _server = null;
                    }
                }
                catch (Exception ex)
                {
                    // log it
                    Console.WriteLine(ex.Message + "\n");
                    Console.WriteLine(Parser.ArgumentsUsage(typeof (CassiniArgs)));
                    _server = null;
                }

                Console.ReadLine();
            }
        }
    }
}