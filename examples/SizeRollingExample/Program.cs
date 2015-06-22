using Serilog;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;

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
                    .WriteTo.RollingFileAlternate(@"C:\logs\serilogtest\today{Date}AB.txt", fileSizeLimitBytes: 512)
                    .WriteTo.Console()
                    .CreateLogger();
            Console.WriteLine("Hit q to exit, any other key to log a message");
            int messageCount = 0;
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                logger.Information("Message: {messageCount}", messageCount);
                messageCount++;
            }
        }
    }
}
