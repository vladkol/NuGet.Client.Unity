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
            var t = RunTest();
            t.Wait();
        }

        static async Task RunTest()
        {
            string projectFolder = "NuGetTmp";
            string targetFolder = "NuGetPlugins";

            await NuGet.Client.Unity.NuGetUnityHelper.AddNuGetPackage(projectFolder, "Microsoft.Identity.Client", null, "1.1.4-preview0002");
            await NuGet.Client.Unity.NuGetUnityHelper.AddNuGetPackage(projectFolder, "MySql.Data");

            await NuGet.Client.Unity.NuGetUnityHelper.Rebuild(projectFolder, targetFolder);
        }
    }
}
