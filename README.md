# Serilog Rolling File Sink (roll-on-file-size alternative) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.RollingFileAlternate.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.RollingFileAlternate/)

This is a rolling file sink that allows you to specify roll over behaviour based on file size.

### Getting started

Install the [Serilog.Sinks.RollingFileAlternate](https://nuget.org/packages/serilog.sinks.rollingfilealternate) package from NuGet:

```powershell
Install-Package Serilog.Sinks.RollingFileAlternate
```

To configure the sink in C# code, call `WriteTo.RollingFileAlternate()` during logger configuration:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.RollingFileAlternate(".\\logs")
    .CreateLogger();
    
log.Information("This will be written to the rolling file set");
```

The sink is configured with the path of a folder for the log file set:

```
logs\20160631-00001.txt
logs\20160701-00001.txt
logs\20160701-00002.txt
```

> **Important:** Only one process may write to a log file at a given time. For multi-process scenarios, either use separate files or [one of the non-file-based sinks](https://github.com/serilog/serilog/wiki/Provided-Sinks).

### File size limit

The file size limit, beyond which a new log file will be created, is specified with the `fileSizeLimitBytes` parameter.

```csharp
    .WriteTo.RollingFileAlternate(".\\logs", fileSizeLimitBytes: 1024 * 1024)
```

The default if no limit is specified is to roll every two megabytes.

### XML `<appSettings>` configuration

To use the alternate rolling file sink with the [Serilog.Settings.AppSettings](https://github.com/serilog/serilog-settings-appsettings) package, first install that package if you haven't already done so:

```powershell
Install-Package Serilog.Settings.AppSettings
```

Instead of configuring the logger in code, call `ReadFrom.AppSettings()`:

```csharp
var log = new LoggerConfiguration()
    .ReadFrom.AppSettings()
    .CreateLogger();
```

In your application's `App.config` or `Web.config` file, specify the rolling file sink assembly and sink configuration under the `<appSettings>` node:

```xml
<configuration>
  <appSettings>
    <add key="serilog:using:RollingFileAlternate" value="Serilog.Sinks.RollingFileAlternate" />
    <add key="serilog:write-to:RollingFileAlternate.logsDirectory" value=".\logs" />
    <add key="serilog:write-to:RollingFileAlternate.fileSizeLimitBytes" value="1048576" />
```

The parameters that can be set through the `serilog:write-to:RollingFileAlternate` keys are the method parameters accepted by the `WriteTo.RollingFileAlternate()` configuration method.

In XML and JSON configuration formats, environment variables can be used in setting values. This means, for instance, that the log folder path can be based on `TMP` or `APPDATA`:

```xml
    <add key="serilog:write-to:RollingFileALternate.logsFolder" value="%APPDATA%\MyApp\logs" />
```

### JSON `appsettings.json` configuration

To use the alternate rolling file sink with _Microsoft.Extensions.Configuration_, for example with ASP.NET Core or .NET Core, use the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) package. First install that package if you have not already done so:

```powershell
Install-Package Serilog.Settings.Configuration
```

Instead of configuring the alternate rolling file directly in code, call `ReadFrom.Configuration()`:

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

In your `appsettings.json` file, under the `Serilog` node, :

```json
{
  "Serilog": {
    "WriteTo": [{
      "Name": "RollingFileAlternate",
      "Args": {
        "logDirectory": ".\\logs",
        "fileSizeLimitBytes": 1048576
      }
    }]
  }
}
```

### Controlling event formatting

The alternate rolling file sink creates events in a fixed text format by default:

```
2016-07-06 09:02:17.148 +10:00 [Information] HTTP "GET" "/" responded 200 in 1994 ms
```

The format is controlled using an _output template_, which the alternate rolling file configuration method accepts as an `outputTemplate` parameter.

The default format above corresponds to an output template like:

```csharp
    .WriteTo.RollingFileAlternate(".\\logs",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
```

