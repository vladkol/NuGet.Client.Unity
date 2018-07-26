using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityNuGetSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            //var t = NuGet.Client.Unity.NuGetUnityHelper.InstallPackage("NuGet.Client");
            //var t = NuGet.Client.Unity.NuGetUnityHelper.InstallDotnetPackage("Microsoft.Identity.Client", null, "1.1.4-preview0002");
            // CNTK.UWP.CPUOnly
            var t = NuGet.Client.Unity.NuGetUnityHelper.GenerateUnityPluginsForNuGet("Microsoft.Identity.Client", null, "1.1.4-preview0002");
            t.Wait();

            string folder = t.Result;
        }
    }
}
