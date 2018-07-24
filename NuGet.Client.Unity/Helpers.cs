using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace NuGet.Client.Unity
{
    internal static class Helpers
    {
        internal static void Dump(this string str)
        {
            Debug.WriteLine(str);
            Console.Error.WriteLine(str);
        }
    }
}
