using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cassini
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new MainForm2(args));
        }

    }
}
