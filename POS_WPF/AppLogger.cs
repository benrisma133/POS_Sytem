using Microsoft.Extensions.Logging;

public static class AppLogger
{
    public static ILoggerFactory Factory { get; } =
    LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.AddProvider(new DebugLoggerProvider());
        builder.SetMinimumLevel(LogLevel.Information);
    });


    public static ILogger CreateLogger<T>() =>
        Factory.CreateLogger<T>();
}
