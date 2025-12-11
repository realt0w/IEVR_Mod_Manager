using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IEVRModManager.Helpers
{
    /// <summary>
    /// Specifies the severity level of a log entry.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug-level messages for detailed diagnostic information.
        /// </summary>
        Debug = 0,
        
        /// <summary>
        /// Informational messages for general application flow.
        /// </summary>
        Info = 1,
        
        /// <summary>
        /// Warning messages for potentially problematic situations.
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// Error messages for error events that might still allow the application to continue.
        /// </summary>
        Error = 3
    }

    /// <summary>
    /// Provides a thread-safe logging system with multiple output targets and log rotation.
    /// </summary>
    public class Logger
    {
        private static Logger? _instance;
        private static readonly object _lockObject = new object();
        
        private readonly ConcurrentQueue<LogEntry> _logQueue;
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private readonly int _maxLogFileSizeBytes;
        private readonly int _maxLogFiles;
        private LogLevel _minimumLevel;
        private bool _writeToFile;
        private bool _writeToDebug;
        private Action<string, LogLevel>? _uiCallback;
        private bool _isDisposed;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _writerTask;

        /// <summary>
        /// Gets the singleton instance of the Logger.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new Logger();
                        }
                    }
                }
                return _instance;
            }
        }

        private Logger()
        {
            _logQueue = new ConcurrentQueue<LogEntry>();
            _logDirectory = Config.BaseDir;
            _logFilePath = Path.Combine(_logDirectory, "app.log");
            _maxLogFileSizeBytes = 5 * 1024 * 1024; // 5 MB
            _maxLogFiles = 5;
            _minimumLevel = LogLevel.Debug;
            _writeToFile = true;
            _writeToDebug = false; // Only in debug builds
            _cancellationTokenSource = new CancellationTokenSource();
            
            FileSystemHelper.EnsureDirectoryExists(_logDirectory);
            
            _writerTask = Task.Run(() => ProcessLogQueue(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// Sets the minimum log level. Messages below this level will be ignored.
        /// </summary>
        /// <param name="level">The minimum log level.</param>
        public void SetMinimumLevel(LogLevel level)
        {
            _minimumLevel = level;
        }

        /// <summary>
        /// Enables or disables writing logs to file.
        /// </summary>
        /// <param name="enabled">Whether to write to file.</param>
        public void SetFileLogging(bool enabled)
        {
            _writeToFile = enabled;
        }

        /// <summary>
        /// Enables or disables writing logs to debug output.
        /// </summary>
        /// <param name="enabled">Whether to write to debug output.</param>
        public void SetDebugLogging(bool enabled)
        {
            _writeToDebug = enabled;
        }

        /// <summary>
        /// Sets a callback for UI logging. The callback receives the formatted message and log level.
        /// </summary>
        /// <param name="callback">The callback function, or null to disable UI logging.</param>
        public void SetUICallback(Action<string, LogLevel>? callback)
        {
            _uiCallback = callback;
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>
        /// Logs a message with an exception.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        public void Log(LogLevel level, string message, Exception? exception)
        {
            if (level < _minimumLevel)
            {
                return;
            }

            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Exception = exception
            };

            _logQueue.Enqueue(entry);

            // Immediate UI callback (synchronous for UI thread)
            _uiCallback?.Invoke(FormatMessage(entry), level);
        }

        /// <summary>
        /// Logs a message at the specified level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        public void Log(LogLevel level, string message)
        {
            Log(level, message, null);
        }

        private void ProcessLogQueue(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var entries = new System.Collections.Generic.List<LogEntry>();
                    
                    // Batch process entries
                    while (_logQueue.TryDequeue(out var entry) && entries.Count < 100)
                    {
                        entries.Add(entry);
                    }

                    if (entries.Count > 0)
                    {
                        WriteEntries(entries);
                    }

                    Thread.Sleep(100); // Small delay to batch writes
                }
                catch (Exception ex)
                {
                    // Last resort: write to debug output if available
                    System.Diagnostics.Debug.WriteLine($"[Logger] Error processing log queue: {ex.Message}");
                }
            }
        }

        private void WriteEntries(System.Collections.Generic.List<LogEntry> entries)
        {
            if (!_writeToFile && !_writeToDebug)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (var entry in entries)
            {
                var formatted = FormatLogEntry(entry);
                sb.AppendLine(formatted);

                if (_writeToDebug)
                {
                    System.Diagnostics.Debug.WriteLine(formatted);
                }
            }

            if (_writeToFile && sb.Length > 0)
            {
                WriteToFile(sb.ToString());
            }
        }

        private string FormatLogEntry(LogEntry entry)
        {
            var timestamp = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var level = entry.Level.ToString().ToUpper().PadRight(7);
            var message = entry.Message;

            var logLine = $"{timestamp} [{level}] {message}";

            if (entry.Exception != null)
            {
                logLine += $"{Environment.NewLine}{entry.Exception}";
            }

            return logLine;
        }

        private string FormatMessage(LogEntry entry)
        {
            var timestamp = entry.Timestamp.ToString("HH:mm:ss");
            return $"[{timestamp}] {entry.Message}";
        }

        private string FormatMessage(LogEntry entry, LogLevel level)
        {
            var timestamp = entry.Timestamp.ToString("HH:mm:ss");
            return $"[{timestamp}] {entry.Message}";
        }

        private void WriteToFile(string content)
        {
            try
            {
                // Check if rotation is needed
                if (File.Exists(_logFilePath))
                {
                    var fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Length >= _maxLogFileSizeBytes)
                    {
                        RotateLogFiles();
                    }
                }

                File.AppendAllText(_logFilePath, content, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Silently fail - we don't want logging to break the application
                System.Diagnostics.Debug.WriteLine($"[Logger] Failed to write to log file: {ex.Message}");
            }
        }

        private void RotateLogFiles()
        {
            try
            {
                // Delete oldest log file if we've reached the limit
                var oldestLog = Path.Combine(_logDirectory, $"app.{_maxLogFiles}.log");
                if (File.Exists(oldestLog))
                {
                    File.Delete(oldestLog);
                }

                // Rotate existing log files
                for (int i = _maxLogFiles - 1; i >= 1; i--)
                {
                    var currentLog = Path.Combine(_logDirectory, i == 1 ? "app.log" : $"app.{i}.log");
                    var nextLog = Path.Combine(_logDirectory, $"app.{i + 1}.log");

                    if (File.Exists(currentLog))
                    {
                        if (File.Exists(nextLog))
                        {
                            File.Delete(nextLog);
                        }
                        File.Move(currentLog, nextLog);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Logger] Failed to rotate log files: {ex.Message}");
            }
        }

        /// <summary>
        /// Flushes all pending log entries to disk.
        /// </summary>
        public void Flush()
        {
            // Process remaining entries
            var entries = new System.Collections.Generic.List<LogEntry>();
            while (_logQueue.TryDequeue(out var entry))
            {
                entries.Add(entry);
            }

            if (entries.Count > 0)
            {
                WriteEntries(entries);
            }
        }

        /// <summary>
        /// Disposes the logger and flushes all pending entries.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _cancellationTokenSource.Cancel();
            
            try
            {
                _writerTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch
            {
                // Ignore timeout
            }

            Flush();
            _cancellationTokenSource.Dispose();
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Message { get; set; } = string.Empty;
            public Exception? Exception { get; set; }
        }
    }
}
