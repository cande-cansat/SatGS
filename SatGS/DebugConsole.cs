using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SatGS
{
    internal static class DebugConsole
    {
        static bool initialized;

        static void Initialize()
        {
            initialized = true;

            AllocConsole();
        }

        public static void Write(string text)
        {
            if (!initialized)
                Initialize();
            Console.Write(text);
        }

        public static void WriteLine(string text)
        {
            if (!initialized)
                Initialize();
            Console.WriteLine(text);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();
    }
}
