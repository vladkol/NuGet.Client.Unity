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
        private const string packagesFolderName = "RestoredNuGetPackages";
        private const string targetFramework = "netstandard2.0";


        static NuGetUnityHelper()
        {

        }

        public static async Task<bool> AddNuGetPackage(string projectFolder, string PackageId, string source = null, string version = null, string[] runtimes = null)
        {
            bool bRes = false;

            Console.WriteLine("Adding NuGet Package {0}", PackageId);

            projectFolder = Path.GetFullPath(projectFolder);
            var projectName = Path.GetFileName(projectFolder);

            if (!Directory.Exists(projectFolder))
            {
                Console.WriteLine("Creating project {0}", projectName);
                Directory.CreateDirectory(projectFolder);
            }

            if (Directory.Exists(projectFolder))
            {
                if(!File.Exists(Path.Combine(projectFolder, projectName) + ".csproj"))
                {
                    ProcessStartInfo pi = PrepareDotNetNewCommandLine();
                    pi.WorkingDirectory = projectFolder;
                    bool ok = await Task<bool>.Run<bool>(() =>
                    {
                        Process donNet = Process.Start(pi);
                        donNet.WaitForExit();
                        return donNet.ExitCode == 0;
                    });
                }

                bRes = await InstallDotnetPackage(projectFolder, PackageId, source, version);
                if(!bRes)
                {
                    Console.WriteLine("Couldn't add package {0}.", projectName);
                }
            }
            else
            {
                Console.WriteLine("Cannot create or access project {0}.", projectName);
            }

            if (bRes)
            {
                Console.WriteLine("Package {0} was successfully added.", PackageId);
            }

            return bRes;
        }

        public static async Task<bool> RemoveNuGetPackage(string projectFolder, string PackageId)
        {
            projectFolder = Path.GetFullPath(projectFolder);
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "dotnet";

            pi.Arguments = $"remove package {PackageId} --package-directory \"{packagesFolderName}\"";

            pi.WorkingDirectory = projectFolder;

            return await Task<bool>.Run<bool>(() =>
            {
                Process donNet = Process.Start(pi);
                donNet.WaitForExit();
                return donNet.ExitCode == 0;
            });
        }

        public static async Task<bool> Rebuild(string projectFolder, string targetFolder, string[] runtimes = null)
        {
            projectFolder = Path.GetFullPath(projectFolder);
            targetFolder = Path.GetFullPath(targetFolder);

            var targetResult = await GenerateUnityPluginsForNuGet(projectFolder, targetFolder, runtimes);
            bool ok = !string.IsNullOrEmpty(targetResult);

            if (ok)
            {
                Console.WriteLine("NuGet project was successfully generated.");
            }
            else
            {
                Console.WriteLine("ERROR: Couldn't build NuGet project.");
            }

            return ok;
        }

        private static async Task<string> GenerateUnityPluginsForNuGet(string projectFolder, string targetFolder, string[] runtimes = null)
        {
            string baseFolder = projectFolder;

            Directory.Delete(Path.Combine(projectFolder, "bin"), true);

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
                
                string pluginsPre = targetFolder;

                string buildFolder = Path.Combine(baseFolder, Path.Combine("bin", "release"));
                if (Directory.Exists(buildFolder))
                {
                    string baseFolderName = Path.GetFileName(baseFolder);
                    string allbuildsFolder = Path.Combine(buildFolder, "netstandard2.0");

                    if (!Directory.Exists(pluginsPre))
                        Directory.CreateDirectory(pluginsPre);

                    bool bOK = true;
                    foreach(var runtime in runtimeList)
                    {
                        var runtimeFolder = Path.Combine(Path.Combine(allbuildsFolder, runtime), "publish");
                        if(Directory.Exists(runtimeFolder))
                        {
                            bOK &= CopyDirectory(runtimeFolder, Path.Combine(pluginsPre, runtime), baseFolderName, null);
                        }
                    }

                    if(bOK)
                    {
                        pluginsFolder = pluginsPre;
                    }
                }
            }

            return pluginsFolder;
        }


        private static bool CopyDirectory(string directory, string targetPathName, string excludeStartWidth, string excludeEndWidth)
        {
            if(!Directory.Exists(targetPathName))
            {
                var di = Directory.CreateDirectory(targetPathName);
                if (di == null || !di.Exists)
                    return false;
            }

            var dirs = Directory.GetDirectories(directory);
            foreach (var d in dirs)
            {
                string dirName = Path.GetFileName(d);

                if( (!String.IsNullOrEmpty(excludeStartWidth) && dirName.StartsWith(excludeStartWidth, StringComparison.CurrentCultureIgnoreCase)) 
                    || (!String.IsNullOrEmpty(excludeEndWidth) && dirName.EndsWith(excludeEndWidth, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                if (!CopyDirectory(d, Path.Combine(targetPathName, dirName), excludeStartWidth, excludeEndWidth))
                    return false;
            }

            var files = Directory.GetFiles(directory);
            foreach(var f in files)
            {
                string fileName = Path.GetFileName(f);

                if ((!String.IsNullOrEmpty(excludeStartWidth) && fileName.StartsWith(excludeStartWidth, StringComparison.CurrentCultureIgnoreCase))
                    || (!String.IsNullOrEmpty(excludeEndWidth) && fileName.EndsWith(excludeEndWidth, StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                var targetFile = Path.Combine(targetPathName, fileName);
                File.Copy(f, targetFile, true);
                if(!File.Exists(targetFile))
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task<bool> InstallDotnetPackage(string projectFolder, string PackageId, string source, string version)
        {
            string restoredPackagesPath = Path.Combine(projectFolder, packagesFolderName);
            Directory.CreateDirectory(restoredPackagesPath);

            ProcessStartInfo pi = PrepareDotNetAddCommandLine(PackageId, source, version);
            pi.WorkingDirectory = projectFolder;
            bool ok = await Task<bool>.Run<bool>(() =>
            {
                Process donNet = Process.Start(pi);
                donNet.WaitForExit();
                return donNet.ExitCode == 0;
            });

            return ok;
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

        private static ProcessStartInfo PrepareDotNetNewCommandLine()
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = "dotnet";

            pi.Arguments = $"new classlib --framework {targetFramework}";



            return pi;
        }
    }
}
