using System.Collections.Concurrent;
using System.Text;

namespace UniversityApi.Common.Logging;

/// <summary>
/// Простой провайдер логирования в файл: logs/app-ГГГГММДД.log.
/// Работает через стандартный ILogger, поэтому код сервисов о файле ничего не знает.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _directory;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

    public FileLoggerProvider(string directory, LogLevel minLevel)
    {
        _directory = directory;
        _minLevel = minLevel;
        Directory.CreateDirectory(_directory);
    }

    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, name => new FileLogger(name, this));

    public void Dispose() => _loggers.Clear();

    internal bool IsEnabled(LogLevel level) => level != LogLevel.None && level >= _minLevel;

    internal void Write(string line)
    {
        var path = Path.Combine(_directory, $"app-{DateTime.Now:yyyyMMdd}.log");
        lock (_lock)
        {
            File.AppendAllText(path, line, Encoding.UTF8);
        }
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _category;
        private readonly FileLoggerProvider _provider;

        public FileLogger(string category, FileLoggerProvider provider)
        {
            _category = category;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var sb = new StringBuilder()
                .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .Append(" [").Append(ShortLevel(logLevel)).Append("] ")
                .Append(_category).Append(": ")
                .Append(formatter(state, exception))
                .AppendLine();

            if (exception is not null)
                sb.AppendLine(exception.ToString());

            _provider.Write(sb.ToString());
        }

        private static string ShortLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => level.ToString()
        };
    }
}

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string directory, LogLevel minLevel)
    {
        builder.AddProvider(new FileLoggerProvider(directory, minLevel));
        return builder;
    }
}
