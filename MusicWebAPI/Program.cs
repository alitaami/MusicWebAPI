// Initialize Serilog with basic configuration
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/musicwebapi_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    WebApplication
        .CreateBuilder(args)
        .ConfigureServices()
        .Build()
        .Configure()
        .Run();
}
catch (Exception ex)
{
    Log.Logger.Error(ex.Message);
    throw;
}
finally
{
    Log.CloseAndFlush(); // Ensure all logs are flushed before exit
}
