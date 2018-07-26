using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace NuGet.Client.Unity
{
    public static class NuGetUnityHelper
    {
        private const string packagesFolderName = "restoredpackages.ng";
        private const string pluginsFolderName = "NuGetUnity.plugins";
        private const string targetFramework = "netstandard2.0";


        static NuGetUnityHelper()
        {

        }

        public static async Task<string> GenerateUnityPluginsForNuGet(string PackageId, string source = null, string version = null, string[] runtimes = null)
        {
            string baseFolder = await InstallDotnetPackage(PackageId, source, version);
            string pluginsFolder = string.Empty;
            if(!string.IsNullOrEmpty(baseFolder))
            {
                List<string> runtimeList = null;
                ProcessStartInfo pi = null;
                if (runtimes == null || runtimes.Length == 0)
                {
                    runtimeList = new List<string>();
                    runtimeList.Add("win10-x64");
                    runtimeList.Add("win10-x86");
                    runtimeList.Add("osx-x64");
                    runtimeList.Add("android");
                }
                else
                {
                    runtimeList = runtimes.ToList();
                }

                foreach(var runtime in runtimeList)
                {
                    pi = PrepareDotNetPublishCommandLine(runtime);

                    pi.WorkingDirectory = baseFolder;
                    bool ok = await Task<bool>.Run<bool>(() =>
                    {
                        Process donNet = Process.Start(pi);
                        donNet.WaitForExit();
                        return donNet.ExitCode == 0;
                    });
                }
                
                string pluginsPre = Path.Combine(baseFolder, pluginsFolderName);
                Directory.CreateDirectory(pluginsPre);

                string buildFolder = Path.Combine(baseFolder, Path.Combine("bin", "release"));
                if (Directory.Exists(buildFolder))
                {
                    string allbuildsFolder = Path.Combine(buildFolder, "netstandard2.0");
                    foreach(var runtime in runtimeList)
                    {
                        string folder = Path.Combine(allbuildsFolder, "netstandard2.0"); 

                    }

                    pluginsFolder = pluginsPre;
                }
            }

            return pluginsFolder;
        }

        public static async Task<string> InstallDotnetPackage(string PackageId, string source, string version)
        {
            string tmpDir = Path.Combine(Path.GetTempPath(), "NuGetUnity" + DateTime.UtcNow.ToFileTime().ToString());
            Directory.CreateDirectory(tmpDir);

            string restoredPackagesPath = Path.Combine(tmpDir, packagesFolderName);
            Directory.CreateDirectory(restoredPackagesPath);

            ProcessStartInfo pi = PrepareDotNetNewCommandLine(PackageId);
            pi.WorkingDirectory = tmpDir;
            bool ok = await Task<bool>.Run<bool>(() =>
            {
                Process donNet = Process.Start(pi);
                donNet.WaitForExit();
                return donNet.ExitCode == 0;
            });

            if (ok)
            {
                pi = PrepareDotNetAddCommandLine(PackageId, source, version);
                pi.WorkingDirectory = tmpDir;
                ok = await Task<bool>.Run<bool>(() =>
                {
                    Process donNet = Process.Start(pi);
                    donNet.WaitForExit();
                    return donNet.ExitCode == 0;
                });
            }

            return ok ? tmpDir : string.Empty;
        }

        private static ProcessStartInfo PrepareDotNetAddCommandLine(string PackageId, string source, string version)
        {
            string srcUri = string.IsNullOrEmpty(source) ? "https://api.nuget.org/v3/index.json" : source;

            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "dotnet";

            pi.Arguments = $"add package {PackageId} --framework {targetFramework} --package-directory \"{packagesFolderName}\" --source \"{srcUri}\"";
            if (!string.IsNullOrEmpty(version))
                pi.Arguments += $" --version {version}";


            return pi;
        }

        private static ProcessStartInfo PrepareDotNetPublishCommandLine(string runtime)
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "dotnet";

            pi.Arguments = $"publish --framework {targetFramework} --configuration Release";
            if (!string.IsNullOrEmpty(runtime))
                pi.Arguments += $" --runtime {runtime}";
            return pi;
        }

        private static ProcessStartInfo PrepareDotNetNewCommandLine(string PackageId)
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "dotnet";

            pi.Arguments = $"new classlib --framework {targetFramework}";



            return pi;
        }
    }
}
