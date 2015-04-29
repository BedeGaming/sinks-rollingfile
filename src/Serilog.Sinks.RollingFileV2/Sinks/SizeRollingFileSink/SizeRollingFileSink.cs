﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileV2.Sinks.SizeRollingFileSink
{
    /// <summary>
    /// Write log events to a series of files. Each file will be suffixed with a
    /// 5 digit sequence number. No special templating in the path specification is
    /// considered.
    /// </summary>
    public sealed class SizeRollingFileSink : ILogEventSink, IDisposable
    {
        private static readonly string ThisObjectName = (typeof (SizeLimitedFileSink).Name);
        private readonly string _filePathTemplate;
        private readonly ITextFormatter _formatter;
        private readonly long _fileSizeLimitBytes;
        private readonly Encoding _encoding;
        private SizeLimitedFileSink _currentSink;
        private readonly object _syncRoot = new object();
        private bool _disposed = false;
        private readonly string _folderPath;

        /// <summary>
        /// Construct a <see cref="SizeRollingFileSink"/>
        /// </summary>
        /// <param name="filePathTemplate"></param>
        /// <param name="formatter"></param>
        /// <param name="fileSizeLimitBytes">
        /// The size in bytes at which a new file should be created</param>
        /// <param name="encoding"></param>
        public SizeRollingFileSink(
            string filePathTemplate,
            ITextFormatter formatter,
            long fileSizeLimitBytes,
            Encoding encoding = null)
        {
            _filePathTemplate = filePathTemplate;
            _formatter = formatter;
            _fileSizeLimitBytes = fileSizeLimitBytes;
            _encoding = encoding;
            _folderPath = Path.GetDirectoryName(filePathTemplate);
            _currentSink = GetLatestSink();
        }

        internal SizeLimitedLogFileDescription CurrentLogFile { get { return _currentSink.LogFileDescription; } }

        /// <summary>
        /// Emits a log event to this sink
        /// </summary>
        /// <param name="logEvent">The <see cref="LogEvent"/> to emit</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Emit(LogEvent logEvent)
        {

            if (logEvent == null) throw new ArgumentNullException("logEvent");

            lock (_syncRoot)
            {
                if (_disposed)
                    throw new ObjectDisposedException(ThisObjectName, "The rolling file sink has been disposed");
                _currentSink = NewFileWhenLimitReached();

                if(_currentSink != null)
                    _currentSink.Emit(logEvent);
            }
        }

        private SizeLimitedFileSink GetLatestSink()
        {
            var latestFileDescription = GetLatestFileDescription();
            return new SizeLimitedFileSink(_formatter, _folderPath, latestFileDescription, _encoding);
        }

        internal SizeLimitedLogFileDescription GetLatestFileDescription()
        {
            IEnumerable<SizeLimitedLogFileDescription> existingFiles =
                GetExistingFiles(_filePathTemplate, _fileSizeLimitBytes);
            var latestFileDescription =
                existingFiles.OrderByDescending(x => x.FileNameComponents.Sequence).FirstOrDefault() ??
                ParseRollingLogfile(_filePathTemplate, _fileSizeLimitBytes);
            return latestFileDescription;
        }


        private SizeLimitedFileSink NewFileWhenLimitReached()
        {
            if(_currentSink.SizeLimitReached)
            {
                var next = _currentSink.LogFileDescription.Next();
                _currentSink.Dispose();
                return new SizeLimitedFileSink(_formatter, _folderPath, next,_encoding);
            }

            return _currentSink;
        }

        private static IEnumerable<SizeLimitedLogFileDescription> GetExistingFiles(string filePathTemplate, long fileSizeLimitBytes)
        {
            var directoryName = Path.GetDirectoryName(filePathTemplate);
            if (string.IsNullOrEmpty(directoryName))
            {
#if ASPNETCORE50
                directory = Directory.GetCurrentDirectory();
#else
                directoryName = Environment.CurrentDirectory;
#endif
            }

            directoryName = Path.GetFullPath(directoryName);
            return
                Directory.GetFiles(directoryName)
                .Select(x => ParseRollingLogfile(x, fileSizeLimitBytes))
                .Where(rollingFile => rollingFile != null);
        }

        private static SizeLimitedLogFileDescription ParseRollingLogfile(string path, long fileSizeLimitBytes)
        {
            var fileNameComponents = FileNameParser.ParseLogFileName(path);

            return new SizeLimitedLogFileDescription(fileNameComponents, fileSizeLimitBytes);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or 
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (!_disposed && _currentSink != null)
                {
                    _currentSink.Dispose();
                    _currentSink = null;
                    _disposed = true;
                }
            }
        }
    }
}
