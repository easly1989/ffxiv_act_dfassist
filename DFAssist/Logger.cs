using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using DFAssist.Core;
using Splat;

namespace DFAssist
{
    public class Logger : FileSystemLogger, IActLogger
    {
        private static readonly Regex EscapePattern = new Regex(@"\{(.+?)\}");
        private RichTextBox _richTextBox;
        public void SetLoggingLevel(LogLevel level)
        {
            Level = level;
        }

        public void SetTextBox(RichTextBox box)
        {
            _richTextBox = box;
        }

        private void Write(LogLevel level, object format)
        {
            if (_richTextBox == null || _richTextBox.IsDisposed)
                return;

            if(level < Level)
                return;

            Color color;
            switch (level)
            {
                case LogLevel.Info:
                    color = Color.Green;
                    break;
                case LogLevel.Warn:
                    color = Color.OrangeRed;
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    color = Color.Red;
                    break;
                case LogLevel.Debug:
                default:
                    color = Color.Gray;
                    break;
            }
            var formatted = format ?? "(null)";
            var datetime = DateTime.Now.ToString("HH:mm:ss");
            var message = $"[{datetime}] {formatted}{Environment.NewLine}";

            ActGlobals.oFormActMain.Invoke(new Action(() =>
            {
                _richTextBox.SelectionStart = _richTextBox.TextLength;
                _richTextBox.SelectionLength = 0;
                _richTextBox.SelectionColor = color;
                _richTextBox.AppendText(message);
                _richTextBox.SelectionColor = _richTextBox.ForeColor;
            }));
        }

        private static string Escape(string line)
        {
            return EscapePattern.Replace(line, "{{$1}}");
        }

        public new void Write(string message, LogLevel logLevel)
        {
            base.Write(message, logLevel);
            Write(logLevel, message);
        }

        public new void Write(Exception exception, string message, LogLevel logLevel)
        {
            base.Write(exception, message, logLevel);
            var exceptionMessage = Escape(exception.Message);
            Write(logLevel, $"{message}: {exceptionMessage}");
        }

        public new void Write(string message, Type type, LogLevel logLevel)
        {
            Write(message, logLevel);
        }

        public new void Write(Exception exception, string message, Type type, LogLevel logLevel)
        {
            Write(exception, message, logLevel);
        }

        public Logger() : base($"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "DFAssist.log")}", 5)
        {
            Level = LogLevel.Debug;
        }
    }
}
