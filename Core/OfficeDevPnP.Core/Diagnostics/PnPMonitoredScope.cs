﻿using OfficeDevPnP.Core.Diagnostics.Tree;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace OfficeDevPnP.Core.Diagnostics
{

    public sealed class PnPMonitoredScope : TreeNode<PnPMonitoredScope>, IDisposable
    {
        internal static LogLevel LogLevel;
        internal static PnPMonitoredScope TopScope;
        internal static ILogger Logger;
        private Stopwatch _stopWatch;
        private string _name;
        private Guid _correlationId;
        private int _threadId;

        public PnPMonitoredScope()
        {
            Guid g = Guid.NewGuid();
            StartScope(string.Format("Unnamed Scope {0}", g));
        }

        internal int ThreadId
        {
            get
            {
                return this._threadId;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }


        public PnPMonitoredScope(string name)
        {
            StartScope(name);
        }


        private void StartScope(string name)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains("PnPLogLevel"))
            {
                var logLevel = ConfigurationManager.AppSettings.Get("PnPLogLevel");
                LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), logLevel);
            }
            else
            {
                LogLevel = LogLevel.Information;
            }
            if (Logger == null)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains("PnPLogClass"))
                {
                    try
                    {
                        var classString = ConfigurationManager.AppSettings.Get("PnPLogClass");
                        var logAssemblyString = classString.Split(',')[0].Trim();
                        var logTypeString = classString.Split(',')[1].Trim();
                        Logger = (ILogger)Activator.CreateInstance(logAssemblyString, logTypeString).Unwrap();
                    }
                    catch (Exception ex)
                    {
                        // Something went wrong, fall back to the built-in PnPTraceLogger
                        Logger = new PnPTraceLogger();
                        Logger.Error(
                            new LogEntry()
                            {
                                Exception = ex,
                                Message = "Logger registration failed",
                                EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                                CorrelationId = TopScope.CorrelationId,
                                ThreadId = _threadId,
                                Source = Name
                            });
                    }
                }
                else
                {
                    Logger = new PnPTraceLogger();
                }
            }
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _stopWatch = new Stopwatch();
            _name = name;
            _stopWatch.Start();
            _correlationId = Guid.NewGuid();

            if (PnPMonitoredScope.TopScope == null)
            {
                PnPMonitoredScope.TopScope = this;
            }
            if (TopScope != this)
            {
                var lastnode = TopScope.Descendants.Any() ? TopScope.Descendants.LastOrDefault() : TopScope;
                ((PnPMonitoredScope)lastnode).Children.Add(this);
            }
            LogInfo(CoreResources.PnPMonitoredScope_Code_execution_started);
        }

        private void EndScope()
        {
            _stopWatch.Stop();
            LogInfo(CoreResources.PnPMonitoredScope_Code_execution_ended, _stopWatch.ElapsedMilliseconds);
            Trace.Flush();
            Parent = null;
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public void LogError(string message, params object[] args)
        {
            if (LogLevel == LogLevel.Error || LogLevel == LogLevel.Debug)
            {
                Logger.Error(new LogEntry()
                {
                    CorrelationId = TopScope.CorrelationId,
                    EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                    Message = string.Format(message, args),
                    Source = Name,
                    ThreadId = _threadId
                });
            }
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            if (LogLevel == LogLevel.Error || LogLevel == LogLevel.Debug)
            {
                Logger.Error(new LogEntry()
                {
                    CorrelationId = TopScope.CorrelationId,
                    EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                    Message = string.Format(message, args),
                    Source = Name,
                    Exception = ex,
                    ThreadId = _threadId
                });
            }
        }

        public void LogInfo(string message, params object[] args)
        {
            if (LogLevel == LogLevel.Information || LogLevel == LogLevel.Debug || LogLevel == LogLevel.Error || LogLevel == LogLevel.Warning)
            {
                Logger.Info(new LogEntry()
                {
                    CorrelationId = TopScope.CorrelationId,
                    EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                    Message = string.Format(message, args),
                    Source = Name,
                    ThreadId = _threadId
                });
            }
        }
        public void LogInfo(Exception ex, string message, params object[] args)
        {
            if (LogLevel == LogLevel.Information || LogLevel == LogLevel.Debug || LogLevel == LogLevel.Error || LogLevel == LogLevel.Warning)
            {
                Logger.Info(new LogEntry()
                {
                    CorrelationId = TopScope.CorrelationId,
                    EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                    Message = string.Format(message, args),
                    Source = Name,
                    Exception = ex,
                    ThreadId = _threadId
                });
            }
        }



        public void LogWarning(string message, params object[] args)
        {
            if (LogLevel == LogLevel.Warning || LogLevel == LogLevel.Information)
            {
                Logger.Warning(new LogEntry()
                {
                    CorrelationId = TopScope.CorrelationId,
                    EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                    Message = string.Format(message, args),
                    Source = Name,
                    ThreadId = _threadId
                });
            }
        }



        public void LogWarning(Exception ex, string message, params object[] args)
        {
            if (LogLevel == LogLevel.Warning || LogLevel == LogLevel.Information)
            {
                Logger.Warning(new LogEntry()
                {
                    CorrelationId = TopScope.CorrelationId,
                    EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                    Message = string.Format(message, args),
                    Source = Name,
                    Exception = ex,
                    ThreadId = _threadId
                });
            }
        }


        public void LogDebug(string message, params object[] args)
        {
            Logger.Debug(new LogEntry()
            {
                CorrelationId = TopScope.CorrelationId,
                EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                Message = string.Format(message, args),
                Source = Name,
                ThreadId = _threadId
            });
        }

        public void LogDebug(Exception ex, string message, params object[] args)
        {
            Logger.Debug(new LogEntry()
            {
                CorrelationId = TopScope.CorrelationId,
                EllapsedMilliseconds = _stopWatch.ElapsedMilliseconds,
                Message = string.Format(message, args),
                Source = Name,
                Exception = ex,
                ThreadId = _threadId
            });
        }

        private string GetLogEntry(string source, string message, params object[] args)
        {

            try
            {
                string msg = string.Empty;

                if (args == null || args.Length == 0)
                {
                    msg = message.Replace("{", "{{").Replace("}", "}}");
                }
                else
                {
                    msg = String.Format(CultureInfo.CurrentCulture, message, args);
                }
                string log = string.Format("{0}\t[{1}]:[{2}]\t{3}\t{4}\t{5}ms\t{6}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), source, this.Depth, ThreadId, msg, _stopWatch.ElapsedMilliseconds, TopScope.CorrelationId);
                //string log = string.Format(CultureInfo.CurrentCulture, "{0} [{1}] {2} {3}ms {4}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), source, msg, _stopWatch.ElapsedMilliseconds, _parentCorrelationId);
                return log;
            }
            catch (Exception e)
            {
                return string.Format("Error while generating log information, {0}", e);
            }
        }

        //private void PnPMonitoredScope_Disposing(object sender, EventArgs e)
        //{
        //    EndScope();
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object,System.Object)")]
        [Conditional("DEBUG")]
        private void WriteLogToConsole(string value)
        {
            Console.WriteLine("{0}{1}", new string(' ', this.Depth * 2), value);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    EndScope();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PnPMonitoredScope() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
