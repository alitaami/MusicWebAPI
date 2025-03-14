// Initialize Serilog with basic configuration
using Serilog;
 
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
