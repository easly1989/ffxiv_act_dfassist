using System;
using System.IO;
using System.Linq;
using Splat;

namespace DFAssist.Core
{
    public abstract class FileSystemLogger : DebugLogger, IDisposable
    {
        private FileInfo _logFile;

        protected FileSystemLogger(string logPath, int logsToSave = 4)
        {
            if (logPath == null) throw new ArgumentNullException(nameof(logPath));

            _logFile = new FileInfo(logPath);

            // search for the logs directory
            var logDir = _logFile.DirectoryName;
            if (logDir == null)
                throw new Exception("log directory cannot be null...");

            // if it doesn't exists, we'll create one 
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            else // otherwise we have to check how many log files are there
            {
                var logfiles = Directory.GetFiles(logDir).Select(x => new FileInfo(x)).OrderBy(x => x.LastWriteTime);
                foreach (var logfile in logfiles)
                {
                    // delete the oldest, until we reach the number "logsToSave"
                    if (logsToSave <= 0)
                    {
                        logfile.Delete();
                        continue;
                    }

                    // preserve the newest one
                    if (logfile.Name.Equals("DFAssist.log"))
                    {
                        // if there is a log file named exactly DFAssist.log it needs to be renamed
                        logfile.MoveTo(Path.Combine(logDir, $"DFAssist-{logfile.LastWriteTime:ddMMyyyy_HHmm}.log"));
                    }

                    logsToSave--;
                }

            }

            // now we can create the new logfile
            using(var stream = _logFile.Create())
            using (var sw = new StreamWriter(stream))
            {
                sw.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm}][{LogLevel.Info}]: DFAssist Logs Started...");
                sw.Close();
            }
        }

        public new void Write(string message, LogLevel logLevel)
        {
            using (var sw = _logFile.AppendText())
            {
                sw.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm}][{logLevel}]: {message}");
                sw.Close();
            }
            base.Write(message, logLevel);
        }

        public new void Write(Exception exception, string message, LogLevel logLevel)
        {
            using (var sw = _logFile.AppendText())
            {
                sw.WriteLine($"[{DateTime.Now:MM/dd/yyyy HH:mm}][{logLevel}]: {message}");
                sw.WriteLine("---------------------- Exception ----------------------");
                sw.WriteLine($"{exception.Message}");
                sw.WriteLine("---------------------- ######### ----------------------");
                sw.Close();
            }
            base.Write(exception, message, logLevel);
        }

        public new void Write(string message, Type type, LogLevel logLevel)
        {
            Write(message, Level);
        }

        public new void Write(Exception exception, string message, Type type, LogLevel logLevel)
        {
            Write(exception, message, logLevel);
        }

        public void Dispose()
        {
            OnDisposeOwnedObjects();
        }

        protected virtual void OnDisposeOwnedObjects()
        {
            using (var sw = _logFile.AppendText())
            {
                sw.WriteLine($"[{DateTime.Now:dd/MM/yyyy HH:mm}][{LogLevel.Info}]: DFAssist Logs Ended.");
            }
            _logFile = null;
        }
    }
}
