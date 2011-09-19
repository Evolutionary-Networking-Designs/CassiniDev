using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassiniDev.Logging
{
    public class ErrorLogger
    {
        static object errorLock = new object(); 
        public static void Log(Exception e)
        {
            lock (errorLock)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------");
                Console.WriteLine(e.Message);
                Console.WriteLine("---");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("--------------------------");
                Console.WriteLine();
            }
        }
    }
}
