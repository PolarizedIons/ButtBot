using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace ButtBot.Library
{
    public static class LogHelper
    {
        public static void SetupLogging()
        {
            var logDir = Environment.GetEnvironmentVariable("LOG_DIR") ?? "./logs/";
            var loggerBuilder = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(new JsonFormatter(), Path.Join(logDir, "log.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30);

            if (Debugger.IsAttached)
            {
                loggerBuilder.WriteTo.Console();
            }
            else
            {
                loggerBuilder.WriteTo.Console(new JsonFormatter());
            }

            var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
            var seqApiKey = Environment.GetEnvironmentVariable("SEQ_API");
            if (!string.IsNullOrEmpty(seqUrl))
            {
                loggerBuilder.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
            }

            Log.Logger = loggerBuilder.CreateLogger();
        }
    }
}
