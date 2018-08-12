using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NuGet.Client.Unity.Tests
{
    internal class NuGetTestContext
    {
        public string projectFolder;
        public string targetFolder;

        public bool addPackageDone = false;

        public NuGetTestContext()
        {
            var tmpPath = Path.Combine(Path.GetTempPath() + Path.GetRandomFileName());
            Directory.CreateDirectory(tmpPath);

            projectFolder = Path.Combine(tmpPath, "NuGetTmp");
            targetFolder = Path.Combine(tmpPath, "NuGetPlugins");
        }
    }
}
