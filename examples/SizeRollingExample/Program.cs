using Serilog;

namespace SizeRollingExample
{
    using System;

    using Serilog.Sinks.RollingFileAlternate;

    public class Program
    {
        static void Main(string[] args)
        {
            var logger =
                new LoggerConfiguration().MinimumLevel.Verbose()
                    .WriteTo.RollingFileAlternate(@"C:\logs\serilogtest\", fileSizeLimitBytes: 4096)
                    .WriteTo.Console()
                    .CreateLogger();

            int messageCount = 0;
            while (true)
            {
                for (int i = 0; i < 100; i++)
                {
                    logger.Information("Message: {messageCount}", messageCount);
                    messageCount++;
                }
                Console.WriteLine("Enter to log 100 logs...");
                Console.ReadLine();
            }
        }
    }
}
