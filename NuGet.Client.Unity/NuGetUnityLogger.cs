using NuGet.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Client.Unity
{
    public class NuGetUnityLogger : ILogger
    {
        public void LogDebug(string data) => $"DEBUG: {data}".Dump();
        public void LogVerbose(string data) => $"VERBOSE: {data}".Dump();
        public void LogInformation(string data) => $"INFORMATION: {data}".Dump();
        public void LogMinimal(string data) => $"MINIMAL: {data}".Dump();
        public void LogWarning(string data) => $"WARNING: {data}".Dump();
        public void LogError(string data) => $"ERROR: {data}".Dump();
        public void LogSummary(string data) => $"SUMMARY: {data}".Dump();

        public Task LogAsync(LogLevel level, string data)
        {
            return Task.Run(() =>
            {
                Log(level, data);
            });
        }

        public async Task LogAsync(ILogMessage message)
        {
            await LogAsync(message.Level, message.Message);
        }

        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    LogDebug(data);
                    break;
                case LogLevel.Error:
                    LogError(data);
                    break;
                case LogLevel.Information:
                    LogInformation(data);
                    break;
                case LogLevel.Verbose:
                    LogVerbose(data);
                    break;
                case LogLevel.Warning:
                    LogWarning(data);
                    break;
                case LogLevel.Minimal:
                default:
                    LogMinimal(data);
                    break;
            }
        }

        public void Log(ILogMessage message)
        {
            Log(message.Level, message.Message);
        }

        public void LogInformationSummary(string data)
        {
            LogInformation(data);
        }

    }
}
