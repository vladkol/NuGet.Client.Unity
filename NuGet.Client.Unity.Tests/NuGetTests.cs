using System;
using System.IO;
using Xunit;

namespace NuGet.Client.Unity.Tests
{
    [TestPriority(1)]
    public class NuGetTests : IDisposable
    {
        NuGetTestContext context;

        public NuGetTests()
        {
            context = new NuGetTestContext();
        }

        public void Dispose()
        {
            if(Directory.Exists(context.projectFolder))
                Directory.Delete(context.projectFolder, true);
            if(Directory.Exists(context.targetFolder))
                Directory.Delete(context.targetFolder, true);
            context = null;
        }

        [Fact, TestPriority(1)]
        public void AddPackage()
        {
            if (context.addPackageDone)
                return;

            string projectFolder = context.projectFolder;
            string targetFolder = context.targetFolder;

            var addPackageTask1 = NuGet.Client.Unity.NuGetUnity.AddNuGetPackage(projectFolder, "Microsoft.Identity.Client", null, "1.1.4-preview0002");
            addPackageTask1.Wait();
            Assert.True(addPackageTask1.Result);

            var addPackageTask2 = NuGet.Client.Unity.NuGetUnity.AddNuGetPackage(projectFolder, "MySql.Data");
            addPackageTask2.Wait();
            Assert.True(addPackageTask2.Result);

            var buildTask = NuGet.Client.Unity.NuGetUnity.Rebuild(projectFolder, targetFolder);
            buildTask.Wait();
            Assert.True(buildTask.Result);

            
            bool fileExistsIdentityClient = File.Exists(Path.Combine(targetFolder,  "Microsoft.Identity.Client.dll"));
            bool fileExistsSecCrypt = File.Exists(Path.Combine(Path.Combine(targetFolder, "win10-x64"), "System.Security.Cryptography.Algorithms.dll"));
            bool fileExistsMySqlData = File.Exists(Path.Combine(targetFolder, "MySql.Data.dll"));

            Assert.True(fileExistsIdentityClient);
            Assert.True(fileExistsSecCrypt);
            Assert.True(fileExistsMySqlData);

            context.addPackageDone = true;
        }

        [Fact, TestPriority(2)]
        public void RemovePackage()
        {
            if(!context.addPackageDone)
            {
                AddPackage();
            }
            Assert.True(context.addPackageDone);

            string projectFolder = context.projectFolder;
            string targetFolder = context.targetFolder;

            var removePackageTask = NuGet.Client.Unity.NuGetUnity.RemoveNuGetPackage(projectFolder, "MySql.Data");
            removePackageTask.Wait();
            Assert.True(removePackageTask.Result);
            var buildTask = NuGet.Client.Unity.NuGetUnity.Rebuild(projectFolder, targetFolder);
            buildTask.Wait();
            Assert.True(buildTask.Result);

            bool fileExistsIdentityClient = File.Exists(Path.Combine(targetFolder, "Microsoft.Identity.Client.dll"));
            bool fileExistsMySqlData = File.Exists(Path.Combine(targetFolder, "MySql.Data.dll"));

            Assert.True(fileExistsIdentityClient);
            Assert.True(!fileExistsMySqlData);
        }
    }
}
