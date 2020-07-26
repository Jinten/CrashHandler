using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CrashHandler
{
    public static class StartupInformation
    {
        public static string[] Args { get; private set; }

        public static void Initialize(StartupEventArgs e)
        {
            Args = e.Args;
        }
    }
}
